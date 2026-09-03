import { BridgeCardReader } from './bridge';
import { EidaAgentCardReader } from './eidaAgent';
import { SimulatedCardReader } from './simulator';
import type { ICardReader } from './types';

/* simulator - no hardware, the default so a fresh clone runs
   bridge    - DI.Vms.CardBridge on the reception machine (the proven path)
   eida      - the ICP JavaScript agent over WebSocket (not wired up) */
const mode = import.meta.env.VITE_CARD_READER ?? 'simulator';

export const cardReader: ICardReader =
  mode === 'bridge' ? new BridgeCardReader()
    : mode === 'eida' ? new EidaAgentCardReader()
    : new SimulatedCardReader();

export * from './types';
