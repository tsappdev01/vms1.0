import { EidaAgentCardReader } from './eidaAgent';
import { SimulatedCardReader } from './simulator';
import type { ICardReader } from './types';

/* Simulator by default. Set VITE_CARD_READER=eida once ICAToolkitService.msi is
   installed and a reader is attached. */
const mode = import.meta.env.VITE_CARD_READER ?? 'simulator';

export const cardReader: ICardReader =
  mode === 'eida' ? new EidaAgentCardReader() : new SimulatedCardReader();

export * from './types';
