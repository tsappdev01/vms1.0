# eidatoolkit.js

`eidatoolkit.js` is **ICP's file, unmodified**, copied from

    id-card-toolkit-windows-web-javascript-sdk-v3.1.6/
      id-card-toolkit-windows-jws-sdk-v3.1.6/lib/web/eidatoolkit.js

of ID Card Toolkit v3.1.6. Do not edit it. If ICP ships a new toolkit, replace the file
from that SDK rather than patching this copy, and re-check `../../js/card-agent.js`
against it — that wrapper is the only thing here that is ours.

It is served to the browser because the card reader is on the reception desk and this app
runs on a server. It opens a WebSocket to the ICP agent on the desk's own machine
(`127.0.0.1`, or `toolkitagent.emiratesid.ae` under TLS, on ports 9004, 9005 and 9020 in
turn) and the agent talks to the reader. See `docs/deployment.md`.

Nothing it returns is trusted by the server. The card is taken from the signed Validation
Gateway XML, verified server-side — `Services/AgentCardReader.cs` says why.
