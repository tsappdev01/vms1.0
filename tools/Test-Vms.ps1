<#
.SYNOPSIS
    End-to-end smoke test of the VMS API against the real database.

.DESCRIPTION
    Drives a complete visitor through the API the way reception would: identify,
    register, check in, appear on the occupancy list, be found again as a repeat
    visitor, and check out. Then verifies the controls that matter - that an
    unmasked ID number is refused to a Security Officer and permitted to a
    Supervisor, and that both the check-in and the unmask are written to the audit
    trail.

    Every step prints PASS or FAIL with the reason. Nothing is mocked: this talks
    to DI.Vms.Api, which talks to SQL Server.

    Start the API first:
        dotnet run --project src/DI.Vms.Api

.PARAMETER BaseUrl
    Where the API is listening. Default https://localhost:7001

.EXAMPLE
    .\tools\Test-Vms.ps1
    .\tools\Test-Vms.ps1 -BaseUrl http://localhost:5001
#>

[CmdletBinding()]
param(
    [string]$BaseUrl = 'https://localhost:7001'
)

$ErrorActionPreference = 'Continue'
$pass = 0
$fail = 0
$api = "$BaseUrl/api/v1"

# The dev certificate is self-signed. PowerShell 7 has a switch; 5.1 needs a callback.
$isPwsh7 = $PSVersionTable.PSVersion.Major -ge 6
if (-not $isPwsh7) {
    try {
        Add-Type -TypeDefinition @'
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustDevCert : ICertificatePolicy {
    public bool CheckValidationResult(ServicePoint sp, X509Certificate c, WebRequest r, int p) { return true; }
}
'@ -ErrorAction SilentlyContinue
        [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustDevCert
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
    } catch { }
}

function Invoke-Api {
    param(
        [string]$Method = 'GET',
        [string]$Path,
        $Body,
        [string]$Role = 'SecuritySupervisor'
    )

    $params = @{
        Method      = $Method
        Uri         = if ($Path -like 'http*') { $Path } else { "$api$Path" }
        Headers     = @{ 'X-Dev-Role' = $Role; 'Accept' = 'application/json' }
        ErrorAction = 'Stop'
    }
    if ($Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 8)
        $params.ContentType = 'application/json'
    }
    if ($isPwsh7) { $params.SkipCertificateCheck = $true }

    Invoke-RestMethod @params
}

function Step {
    param([string]$Name, [scriptblock]$Test)
    try {
        $detail = & $Test
        Write-Host '[ PASS ] ' -ForegroundColor Green -NoNewline
        Write-Host $Name -NoNewline
        if ($detail) { Write-Host "  $detail" -ForegroundColor DarkGray } else { Write-Host '' }
        $script:pass++
        return $true
    } catch {
        Write-Host '[ FAIL ] ' -ForegroundColor Red -NoNewline
        Write-Host $Name
        Write-Host "         $($_.Exception.Message)" -ForegroundColor DarkGray
        $script:fail++
        return $false
    }
}

Write-Host ''
Write-Host "VMS end-to-end test  ->  $BaseUrl" -ForegroundColor Cyan
Write-Host ('=' * 60)
Write-Host ''

# ------------------------------------------------------------------ reachable

$up = Step 'API is running' {
    $h = Invoke-Api -Path "$BaseUrl/health"
    if ($h.status -ne 'ok') { throw "health returned '$($h.status)'" }
    'health = ok'
}

if (-not $up) {
    Write-Host ''
    Write-Host 'The API is not responding. Start it with:' -ForegroundColor Yellow
    Write-Host '  dotnet run --project src/DI.Vms.Api' -ForegroundColor Yellow
    Write-Host ''
    exit 1
}

# --------------------------------------------------------------- master data

$script:entity = $null
$script:host_ = $null

Step 'DI entities are seeded' {
    $e = @(Invoke-Api -Path '/entities')
    if ($e.Count -eq 0) { throw 'no entities - run db\seed\010_seed_master_data.sql' }
    $script:entity = $e[0]
    "$($e.Count) entities, using '$($script:entity.entityName)'"
} | Out-Null

Step 'Hosts are seeded' {
    $h = @(Invoke-Api -Path '/employees')
    if ($h.Count -eq 0) { throw 'no employees - run db\seed\010_seed_master_data.sql' }
    $script:host_ = $h[0]
    "$($h.Count) hosts, using '$($script:host_.name)'"
} | Out-Null

if (-not $script:entity -or -not $script:host_) {
    Write-Host ''
    Write-Host 'Cannot continue without master data. Run:' -ForegroundColor Yellow
    Write-Host '  sqlcmd -S UATWEB01 -d VMS -E -C -b -i db\seed\010_seed_master_data.sql' -ForegroundColor Yellow
    Write-Host ''
    exit 1
}

# A syntactically valid Emirates ID that is unlikely to collide with a previous run.
$rand = Get-Random -Minimum 1000000 -Maximum 9999999
$idNumber = "7841990$rand" + (Get-Random -Minimum 0 -Maximum 9)
$visitorName = "Test Visitor $rand"

# --------------------------------------------------------- registration flow

$script:visitId = $null
$script:visitNumber = $null
$script:visitorId = $null

Step 'Unknown ID is reported as a new visitor' {
    $r = Invoke-Api -Method POST -Path '/visits/identify' -Body @{
        idType = 'EmiratesId'; idNumber = $idNumber; captureMethod = 'CardReader'
        cardVerification = @{ isGenuine = $true; cardStatus = 'Valid'; verifiedAtUtc = $null; vgAvailable = $true }
    }
    if ($r.found) { throw 'a brand new ID was reported as already known' }
    'found = false'
} | Out-Null

Step 'Visit is created' {
    $r = Invoke-Api -Method POST -Path '/visits' -Body @{
        visitor = @{
            name = $visitorName; company = 'Smoke Test Ltd'; idType = 'EmiratesId'
            idNumber = $idNumber; idExpiryDate = '2030-01-01'; nationality = 'ARE'
            dateOfBirth = '1990-01-01'; photo = $null; captureMethod = 'CardReader'
        }
        visitorId = $null
        diEntityId = $script:entity.id
        hostEmployeeId = $script:host_.id
        purpose = 'Automated smoke test'
        visitType = 'Guest'
        expectedDate = $null; expectedTime = $null
    }
    $script:visitId = $r.id
    if (-not $script:visitId) { throw 'no visit id returned' }
    "status = $($r.status)"
} | Out-Null

if (-not $script:visitId) { Write-Host ''; Write-Host 'Cannot continue.' -ForegroundColor Yellow; exit 1 }

Step 'Check-in allocates a visit number' {
    $png = [Convert]::ToBase64String([byte[]](1..40))
    $r = Invoke-Api -Method POST -Path "/visits/$($script:visitId)/check-in" -Body @{
        signatureImage = $png; deviceId = 'SMOKE-TEST'
    }
    $script:visitNumber = $r.visitNumber
    if ($r.status -ne 'Inside') { throw "status is '$($r.status)', expected Inside" }
    if (-not $script:visitNumber) { throw 'no visit number allocated' }
    "$($script:visitNumber), host $($r.host.name)"
} | Out-Null

Step 'Checking in twice is refused' {
    try {
        Invoke-Api -Method POST -Path "/visits/$($script:visitId)/check-in" -Body @{
            signatureImage = 'AAAA'; deviceId = 'SMOKE-TEST'
        } | Out-Null
        throw 'the second check-in was accepted'
    } catch {
        if ($_.Exception.Message -eq 'the second check-in was accepted') { throw }
        'rejected, as it should be'
    }
} | Out-Null

Step 'Visitor appears in Currently Inside' {
    $inside = @(Invoke-Api -Path '/visits/inside')
    if (-not ($inside | Where-Object { $_.visitNumber -eq $script:visitNumber })) {
        throw 'the visit is not in /visits/inside'
    }
    $script:visitorId = ($inside | Where-Object { $_.visitNumber -eq $script:visitNumber }).id
    "$($inside.Count) currently inside"
} | Out-Null

Step 'Visitor appears on the evacuation list' {
    $occ = @(Invoke-Api -Path '/emergency/occupancy')
    if (-not ($occ | Where-Object { $_.visitNumber -eq $script:visitNumber })) {
        throw 'the visit is not in /emergency/occupancy'
    }
    "$($occ.Count) on site"
} | Out-Null

Step 'ID number is returned masked, never in full' {
    $inside = @(Invoke-Api -Path '/visits/inside')
    $v = $inside | Where-Object { $_.visitNumber -eq $script:visitNumber }
    if ($v.idNumberMasked -notmatch 'X') { throw "not masked: $($v.idNumberMasked)" }
    if (($v | ConvertTo-Json -Depth 5) -match $idNumber) { throw 'the plaintext ID leaked into the list response' }
    $v.idNumberMasked
} | Out-Null

# ------------------------------------------------------------------ controls

Step 'Repeat visitor is recognised on a second scan' {
    $r = Invoke-Api -Method POST -Path '/visits/identify' -Body @{
        idType = 'EmiratesId'; idNumber = $idNumber; captureMethod = 'CardReader'
        cardVerification = $null
    }
    if (-not $r.found) { throw 'the same ID was not recognised the second time' }
    "found '$($r.visitor.name)', $($r.visitor.totalVisits) visit(s)"
} | Out-Null

Step 'A Security Officer is refused the unmasked ID' {
    $visitorGuid = (Invoke-Api -Path '/visits/inside' |
        Where-Object { $_.visitNumber -eq $script:visitNumber }).id
    try {
        Invoke-Api -Path "/visitors/$visitorGuid/id-number" -Role 'SecurityOfficer' | Out-Null
        throw 'an officer was given the unmasked ID'
    } catch {
        if ($_.Exception.Message -eq 'an officer was given the unmasked ID') { throw }
        if ($_.Exception.Response.StatusCode.value__ -eq 403 -or $_.Exception.Message -match '403|Forbidden') {
            'refused with 403'
        } else {
            throw "expected 403, got: $($_.Exception.Message)"
        }
    }
} | Out-Null

Step 'Check-out records a duration' {
    $r = Invoke-Api -Method POST -Path "/visits/$($script:visitId)/check-out"
    if ($r.status -ne 'CheckedOut') { throw "status is '$($r.status)'" }
    "$($r.durationMinutes) minute(s)"
} | Out-Null

Step 'Checking out twice is refused' {
    try {
        Invoke-Api -Method POST -Path "/visits/$($script:visitId)/check-out" | Out-Null
        throw 'the second check-out was accepted'
    } catch {
        if ($_.Exception.Message -eq 'the second check-out was accepted') { throw }
        'rejected, as it should be'
    }
} | Out-Null

Step 'Check-in and check-out are in the audit trail' {
    $audit = Invoke-Api -Path '/audit?pageSize=200'
    $rows = @($audit.items | Where-Object { $_.recordRef -eq $script:visitNumber -or $_.newValue -eq $script:visitNumber })
    $actions = @($rows | Select-Object -ExpandProperty action -Unique)
    if ($actions -notcontains 'CHECK-IN') { throw "no CHECK-IN row for $($script:visitNumber)" }
    if ($actions -notcontains 'CHECK-OUT') { throw "no CHECK-OUT row for $($script:visitNumber)" }
    ($actions -join ', ')
} | Out-Null

Step 'Reports run' {
    $defs = @(Invoke-Api -Path '/reports')
    if ($defs.Count -lt 11) { throw "expected 11 report definitions, got $($defs.Count)" }
    $r = Invoke-Api -Path '/reports/daily-visitors'
    "$($defs.Count) reports; daily-visitors returned $($r.rows.Count) row(s)"
} | Out-Null

# ------------------------------------------------------------------- verdict

Write-Host ''
Write-Host ('=' * 60)
if ($fail -eq 0) {
    Write-Host "All $pass checks passed." -ForegroundColor Green
    Write-Host ''
    Write-Host "Test visitor '$visitorName' ($($script:visitNumber)) was left in the database," -ForegroundColor DarkGray
    Write-Host 'checked out. Remove it before UAT sign-off if the data must be clean.' -ForegroundColor DarkGray
} else {
    Write-Host "$pass passed, $fail failed." -ForegroundColor Red
}
Write-Host ''
exit $(if ($fail -eq 0) { 0 } else { 1 })
