import { useEffect, useState } from 'react';
import { PageHeader } from '../components/PageHeader';
import { SignaturePad } from '../components/SignaturePad';
import { api } from '../api';
import { useEntities, useEmployees } from '../api/hooks';
import { cardReader, type CardReadResult, type ReaderState } from '../cardReader';
import type { VisitorType } from '../api/types';
import { formatDate } from '../lib/format';

/* The BRD 24 registration flow:
   IDENTIFY -> SCAN -> VISITOR INFO -> VISIT DETAILS -> SIGNATURE -> CONFIRM -> CHECK-IN */

const STEPS = ['Identify', 'Visitor', 'Visit', 'Signature', 'Confirm'] as const;
type Step = 0 | 1 | 2 | 3 | 4;

const VISIT_TYPES: VisitorType[] = [
  'Guest', 'Customer', 'Supplier', 'Contractor', 'Consultant',
  'GovernmentOfficial', 'InterviewCandidate', 'Delivery', 'ServiceProvider', 'Other',
];

export function Registration() {
  const [step, setStep] = useState<Step>(0);
  const [reader, setReader] = useState<ReaderState | null>(null);
  const [scanning, setScanning] = useState(false);
  const [card, setCard] = useState<CardReadResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  // Visitor
  const [name, setName] = useState('');
  const [company, setCompany] = useState('');
  const [idNumber, setIdNumber] = useState('');
  const [idExpiry, setIdExpiry] = useState('');
  const [nationality, setNationality] = useState('');
  const [manual, setManual] = useState(false);

  // Visit
  const [entityId, setEntityId] = useState('');
  const [hostId, setHostId] = useState('');
  const [purpose, setPurpose] = useState('');
  const [visitType, setVisitType] = useState<VisitorType>('Guest');

  const [signature, setSignature] = useState<string | null>(null);
  const [result, setResult] = useState<{ visitNumber: string; host: string } | null>(null);

  const { data: entities } = useEntities();
  const { data: hosts } = useEmployees();
  const host = hosts?.find((h) => h.id === hostId);

  useEffect(() => { cardReader.probe().then(setReader).catch(() => setReader(null)); }, []);

  async function scan() {
    setScanning(true);
    setError(null);
    try {
      const read = await cardReader.read();
      setCard(read);
      setName(read.data.name);
      setIdNumber(read.data.idNumber);
      setIdExpiry(read.data.expiryDate ?? '');
      setNationality(read.data.nationality ?? '');
      setStep(1);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'The card could not be read.');
    } finally {
      setScanning(false);
    }
  }

  async function submit() {
    setBusy(true);
    setError(null);
    try {
      const created = await api.createVisit({
        visitor: {
          name,
          company: company || null,
          idType: 'EmiratesId',
          idNumber,
          idExpiryDate: idExpiry || null,
          nationality: nationality || null,
          dateOfBirth: card?.data.dateOfBirth ?? null,
          photo: card?.data.photoBase64 ?? null,
          captureMethod: card ? 'CardReader' : 'Manual',
        },
        visitorId: null,
        diEntityId: entityId,
        hostEmployeeId: hostId,
        purpose: purpose || null,
        visitType,
        expectedDate: null,
        expectedTime: null,
      });

      const checkedIn = await api.checkIn(created.id, signature ?? '', 'PORTAL');
      setResult({ visitNumber: checkedIn.visitNumber, host: checkedIn.host.name });
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Check-in failed.');
    } finally {
      setBusy(false);
    }
  }

  function reset() {
    setStep(0); setCard(null); setName(''); setCompany(''); setIdNumber('');
    setIdExpiry(''); setNationality(''); setEntityId(''); setHostId('');
    setPurpose(''); setVisitType('Guest'); setSignature(null); setResult(null);
    setError(null); setManual(false);
  }

  if (result) {
    return (
      <>
        <PageHeader title="Checked In" />
        <section className="card" style={{ maxWidth: 560 }}>
          <div className="card-body">
            <div className="status" style={{ fontSize: 15, marginBottom: 14 }}>
              <span className="dot" style={{ background: 'var(--status-good)' }} aria-hidden="true" />
              Inside
            </div>
            <dl className="readout">
              <dt>Visit number</dt><dd>{result.visitNumber}</dd>
              <dt>Visitor</dt><dd>{name}</dd>
              <dt>Meeting</dt><dd>{result.host}</dd>
              <dt>Floor / office</dt><dd>{host?.floor ?? '—'} / {host?.office ?? '—'}</dd>
            </dl>
            <div className="banner" style={{ marginTop: 16, marginBottom: 0 }}>
              <strong>Host not notified.</strong>
              <span>Host notification (BRD §8) is not built yet — tell the host manually.</span>
            </div>
            <div className="actions">
              <button className="primary" onClick={reset}>Register another visitor</button>
            </div>
          </div>
        </section>
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Visitor Registration"
        description="Identify the visitor, record the visit, capture the acknowledgement, check in."
      />

      <div className="steps">
        {STEPS.map((label, i) => (
          <div key={label} className={`step ${i === step ? 'active' : ''} ${i < step ? 'done' : ''}`}>
            <span className="num">{i < step ? '✓' : i + 1}</span>{label}
          </div>
        ))}
      </div>

      {error && (
        <div className="banner" style={{ borderColor: 'var(--status-critical)' }}>
          <strong>Problem.</strong><span>{error}</span>
        </div>
      )}

      <section className="card" style={{ maxWidth: 720 }}>
        <div className="card-body">

          {step === 0 && (
            <>
              {reader && (
                <div className={`banner ${reader.simulated ? 'warn' : ''}`}>
                  <strong>{reader.simulated ? 'Simulated reader.' : reader.available ? 'Reader ready.' : 'No reader.'}</strong>
                  <span>{reader.detail}</span>
                </div>
              )}
              <p style={{ marginTop: 0, color: 'var(--text-secondary)' }}>
                Ask the visitor for their Emirates ID and place it in the reader.
              </p>
              <div className="actions" style={{ justifyContent: 'flex-start' }}>
                <button className="primary" onClick={scan} disabled={scanning || !reader?.available}>
                  {scanning ? 'Reading card…' : 'Read Emirates ID'}
                </button>
                <button onClick={() => { setManual(true); setStep(1); }}>
                  Enter details manually
                </button>
              </div>
              <p className="hint" style={{ fontSize: 12, color: 'var(--text-muted)', marginBottom: 0 }}>
                Manual entry covers passports, GCC IDs and driving licences, which the toolkit
                does not read, and keeps reception working when a reader fails.
              </p>
            </>
          )}

          {step === 1 && (
            <>
              {card && (
                <div className="banner">
                  <strong>
                    {card.verification.cardStatus === 'Valid' ? '🟢 Verified'
                      : card.verification.cardStatus === 'Expired' ? '🟡 ID expired'
                      : '🔴 ' + card.verification.cardStatus}
                  </strong>
                  <span>
                    {card.verification.vgAvailable
                      ? `Card checked against the Validation Gateway. Status: ${card.verification.cardStatus}.`
                      : 'Validation Gateway unreachable — the visit will be flagged for later verification.'}
                  </span>
                </div>
              )}
              <div className="field-row">
                <div className="field">
                  <label htmlFor="name">Visitor name</label>
                  <input id="name" value={name} onChange={(e) => setName(e.target.value)} readOnly={!!card} />
                  {card && <div className="hint">From the card chip — not editable.</div>}
                </div>
                <div className="field">
                  <label htmlFor="company">Company</label>
                  <input id="company" value={company} onChange={(e) => setCompany(e.target.value)}
                    placeholder="ABC Trading LLC" />
                </div>
              </div>
              <div className="field-row">
                <div className="field">
                  <label htmlFor="idnum">ID number</label>
                  <input id="idnum" value={idNumber} onChange={(e) => setIdNumber(e.target.value)}
                    readOnly={!!card} placeholder="784XXXXXXXXXXXX" />
                </div>
                <div className="field">
                  <label htmlFor="exp">ID expiry</label>
                  <input id="exp" type="date" value={idExpiry} onChange={(e) => setIdExpiry(e.target.value)} readOnly={!!card} />
                </div>
              </div>
              {manual && (
                <div className="banner warn">
                  <strong>Manual entry.</strong>
                  <span>Recorded as unverified — reports distinguish this from a chip-verified identity.</span>
                </div>
              )}
              <div className="actions">
                <button onClick={() => setStep(0)}>Back</button>
                <button className="primary" disabled={!name || !idNumber} onClick={() => setStep(2)}>Continue</button>
              </div>
            </>
          )}

          {step === 2 && (
            <>
              <div className="field">
                <label htmlFor="entity">DI entity</label>
                <select id="entity" value={entityId} onChange={(e) => setEntityId(e.target.value)}>
                  <option value="">Select…</option>
                  {entities?.map((e) => <option key={e.id} value={e.id}>{e.entityName}</option>)}
                </select>
              </div>
              <div className="field">
                <label htmlFor="host">Person to visit</label>
                <select id="host" value={hostId} onChange={(e) => setHostId(e.target.value)}>
                  <option value="">Select…</option>
                  {hosts?.map((h) => <option key={h.id} value={h.id}>{h.name} — {h.department}</option>)}
                </select>
                {host && (
                  <div className="hint">
                    {host.entityName} · {host.department} · Floor {host.floor} · Office {host.office}
                    {' '}— filled in from the host record.
                  </div>
                )}
              </div>
              <div className="field-row">
                <div className="field">
                  <label htmlFor="type">Visitor type</label>
                  <select id="type" value={visitType} onChange={(e) => setVisitType(e.target.value as VisitorType)}>
                    {VISIT_TYPES.map((t) => <option key={t} value={t}>{t.replace(/([a-z])([A-Z])/g, '$1 $2')}</option>)}
                  </select>
                </div>
                <div className="field">
                  <label htmlFor="purpose">Purpose</label>
                  <input id="purpose" value={purpose} onChange={(e) => setPurpose(e.target.value)}
                    placeholder="Business Meeting" />
                </div>
              </div>
              <div className="actions">
                <button onClick={() => setStep(1)}>Back</button>
                <button className="primary" disabled={!entityId || !hostId} onClick={() => setStep(3)}>Continue</button>
              </div>
            </>
          )}

          {step === 3 && (
            <>
              <p style={{ marginTop: 0, color: 'var(--text-secondary)' }}>
                The visitor signs to acknowledge the visit. This is required — check-in cannot
                proceed without it.
              </p>
              <SignaturePad onChange={setSignature} />
              <div className="actions">
                <button onClick={() => setStep(2)}>Back</button>
                <button className="primary" disabled={!signature} onClick={() => setStep(4)}>Continue</button>
              </div>
            </>
          )}

          {step === 4 && (
            <>
              <dl className="readout">
                <dt>Visitor</dt><dd>{name}</dd>
                <dt>Company</dt><dd>{company || '—'}</dd>
                <dt>ID expiry</dt><dd>{idExpiry ? formatDate(idExpiry) : '—'}</dd>
                <dt>Identity</dt><dd>{card ? 'Read from card chip' : 'Entered manually'}</dd>
                <dt>DI entity</dt><dd>{entities?.find((e) => e.id === entityId)?.entityName ?? '—'}</dd>
                <dt>Person to visit</dt><dd>{host?.name ?? '—'}</dd>
                <dt>Department</dt><dd>{host?.department ?? '—'}</dd>
                <dt>Floor / office</dt><dd>{host?.floor ?? '—'} / {host?.office ?? '—'}</dd>
                <dt>Purpose</dt><dd>{purpose || '—'}</dd>
                <dt>Signature</dt><dd>Captured</dd>
              </dl>
              <div className="actions">
                <button onClick={() => setStep(3)}>Back</button>
                <button className="primary" onClick={submit} disabled={busy}>
                  {busy ? 'Checking in…' : 'CHECK IN'}
                </button>
              </div>
            </>
          )}
        </div>
      </section>
    </>
  );
}
