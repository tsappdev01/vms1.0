# Support request to ICP — offline configuration returns unsigned responses

Follow-up to [`icp-support-request.md`](icp-support-request.md), which covers the licence
activation this depends on. Replace every `<…>` placeholder before sending. Attach the
toolkit log from a reception PC (`EIDAToolkit_3.1.6_<date>.log`) — it holds toolkit
diagnostics, no cardholder data.

---

**Subject:** ID Card Toolkit 3.1.6 — offline configuration returns unsigned public-data responses; need Validation Gateway access for signed responses — SP `<service provider name>`

Dear ICP Support,

Further to our licence activation request, we now have a working deployment and one
remaining blocker, which we believe depends on the same licence.

**What we have working**

Emirates ID cards are read successfully at reception desks using the ID Card Toolkit
agent (`ICAToolkitService.msi`, 64-bit) driven from a browser via `eidatoolkit.js`, with
the application server elsewhere on our network. Reader detection, licence expiry
(14 Apr 2027) and public data read all work.

**The problem**

The `toolkit_response` returned by `readPublicData` contains **no XML signature**. The
document is otherwise well formed — `ValidationGatewayResponse`, `Message`, `Header` with
our `RequestID` echoed correctly, and `Body/PublicData` fully populated — but there is no
`<ds:Signature>` element anywhere in it.

We understand why: our configuration bundle is `IDCARDOFFLINE_config_2026-04-14` and the
toolkit is configured with `read_publicdata_offline = true`, so the read never reaches the
Validation Gateway, and the gateway is what signs the response. We adopted offline mode
because the gateway rejected our licence with **401 "License not found or not active"**
(licence type reported: PRE-PRODUCTION).

Setting `read_publicdata_offline = false` does not help, as the bundle we hold is an
offline bundle.

**Why it matters to us**

Our application server is not the machine with the card reader. The read is performed by
the toolkit agent on the reception PC and the response is sent to the server. The server
therefore has to establish that a response is a genuine card read rather than something
submitted by a browser, and the XML signature is the only thing that can establish it. We
already bind each read to a server-issued single-use request ID, which prevents an old
response being replayed, but without a signature nothing prevents one being fabricated.

**What we are asking for**

1. Activation of our Service Provider licence for the Validation Gateway, so that online
   reads succeed. (This is the request in our earlier ticket.)
2. A **non-offline production configuration bundle** for that licence, so that
   `readPublicData` returns a gateway-signed response.
3. Confirmation of which certificate signs the response — subject and SHA-1 thumbprint —
   so we can pin it. We intend to accept a response only when its signature verifies
   against a signer we have been told to expect, rather than against whatever certificate
   arrives inside the response.
4. Whether any outbound network access from the reception PCs is required for an online
   read, and to which hosts and ports, so we can have it opened.

**Environment**

| | |
|---|---|
| Toolkit | 3.1.6, Windows x64 |
| Deployment | `ICAToolkitService.msi` agent on reception PCs; browser SDK `eidatoolkit.js`; application server separate |
| Agent socket | `ws://127.0.0.1:9004` |
| Config bundle | `IDCARDOFFLINE_config_2026-04-14` (`config_ag`, `config_li`, `config_pg`, `config_vg_qa`, `config_lv_qa`, `config_tk_qa`) |
| Licence expiry reported | 14 Apr 2027 |
| Reader | ACS ACR39U ICC Reader |
| Read parameters | `readPublicData(requestId, nonModifiable=true, modifiable=false, photo=true, signatureImage=true, address=true)` |

**One defect to report while we are here**

In `eidatoolkit.js` v3.1.6, `CardPublicData` always constructs `ModifiablePublicData`,
including when the read was made with `readModifiableData = false`. Unlike `HomeAddress`
and `WorkAddress`, which both return null for a missing body, `ModifiablePublicData`
dereferences its argument immediately:

```js
function ModifiablePublicData(xmlModifiableDataBody) {
    this.occupationCode = xmlModifiableDataBody.OccupationCode;   // throws when absent
```

So any read that does not request modifiable data fails with
`Cannot read properties of undefined (reading 'OccupationCode')` — after the card has been
read and the signed response is already in hand. A guard matching the one in `HomeAddress`
would fix it. We have worked around it locally.

Kind regards,

`<name>`
`<title>`, Dubai Investments
`<email>` · `<phone>`
