# UAT Sign-off Sheet

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — UAT Sign-off Sheet (Controlled, required)
Signed by the **Business Owner and the IT Head**.

This sheet records that the testing in §4.5 was executed and what it found. It is not a
formality: policy §10 is explicit that no application handling employee, financial or
customer data goes live without the documentation set for its classification, and §4.5 is
explicit that the testing loop repeats **until the application passes all scenarios**.

---

## 1. What was tested

| | |
|---|---|
| Application | Visitor Management System (VMS) |
| Version tested | `[ ]` — web build / commit, and Android versionName (versionCode) |
| Environment | `[ ]` — staging |
| Test period | `[ ]` to `[ ]` |

## 2. Results

| Scenario | Document | Run by | Total | Passed | Failed | Outstanding |
|---|---|---|---|---|---|---|
| 1 — Functional validation | `12-uat-test-cases.md` F1–F18 | IT + Digital Champion | 18 | `[ ]` | `[ ]` | `[ ]` |
| 2 — Edge case and boundary | `12-uat-test-cases.md` E1–E34 | IT | 34 | `[ ]` | `[ ]` | `[ ]` |
| 3 — Security and access | `07-security-review-checklist.md` §E | IT | 20 | `[ ]` | `[ ]` | `[ ]` |
| Regression after fixes | standing set F1, F4, F16, E2, E6, E7, E12 | IT | 7 | `[ ]` | `[ ]` | `[ ]` |

## 3. Failures and their resolution

| # | Scenario | What failed | Fix | Re-tested | Date |
|---|---|---|---|---|---|
| | | | | | |

## 4. Accepted with known limitations

Anything signed off **despite** a failure or a gap is listed here, with who accepted it.
A limitation not written here is not accepted.

| # | Limitation | Risk ID | Accepted by | Date |
|---|---|---|---|---|
| 1 | Tablet card reads are not cryptographically signed, so a tablet check-in is lower assurance than a desk check-in | R-01 | `[ ]` | |
| 2 | Emirates ID numbers are displayed and exported in full | R-03 | `[ ]` | |
| 3 | No record is kept of who read or exported the report | R-04 | `[ ]` | |
| 4 | No retention period; nothing deletes a visit | R-05 | `[ ]` | |
| 5 | One tablet returns toolkit error 233 and cannot read cards | R-08 | `[ ]` | |
| 6 | Input validation is not implemented with FluentValidation | A4 | `[ ]` | |
| 7 | No audit trail table | A3 | `[ ]` | |
| 8 | No Semgrep SAST step in CI | R-07 | `[ ]` | |
| | *(add any other)* | | | |

> **These eight are open as this sheet is written.** Each is either closed before sign-off
> or accepted above with a named person against it. Signing the sheet with this table blank
> and the items still open would be signing something untrue.

## 5. Conditions of sign-off

Tick each. Sign-off is not valid without all four.

- [ ] Every scenario in Scenarios 1, 2 and 3 has been executed and recorded
- [ ] Every failure has been fixed and re-tested, or appears in §4 with a named acceptor
- [ ] `07-security-review-checklist.md` §E19 — the OWASP ZAP baseline scan — returned no High findings
- [ ] The Digital Champion confirms the application matches the real reception workflow

## 6. Signatures

By signing, each party confirms the statement beside their name.

| Role | Confirms | Name | Signature | Date |
|---|---|---|---|---|
| **Digital Champion** (reception) | The application does what reception actually does, in the order reception actually does it | | | |
| **IT** (tester) | Scenarios 1, 2 and 3 were executed as recorded, and the failures listed in §3 are the only ones found | | | |
| **IT Head** | The security review is complete and the limitations in §4 are understood and accepted | | | |
| **Business Owner** | The limitations in §4 are accepted on behalf of the business, including the retention decision still outstanding in `08-data-classification.md` §5 | | | |

---

Once signed, this sheet is the precondition for `14-go-live-approval.md`.
