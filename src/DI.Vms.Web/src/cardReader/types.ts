/* The reception client reads Emirates ID through this interface, never through the
   toolkit directly. Two implementations exist: a simulator that needs no hardware,
   and an adapter over the ICP toolkit agent. Which one is used is a runtime
   decision, so the flow can be built and tested before a reader is attached. */

export type CardStatus = 'Valid' | 'Expired' | 'Lost' | 'Stolen' | 'Revoked' | 'Unknown';

export interface CardPublicData {
  idNumber: string;
  name: string;
  nationality: string | null;
  dateOfBirth: string | null;   // ISO yyyy-mm-dd
  expiryDate: string | null;    // ISO yyyy-mm-dd
  gender: string | null;
  photoBase64: string | null;
}

export interface CardVerification {
  /** Cryptographic proof the card is authentic. Null when the gateway is unreachable. */
  isGenuine: boolean | null;
  /** Lost / stolen / revoked check against the ICP Validation Gateway. */
  cardStatus: CardStatus;
  verifiedAtUtc: string | null;
  /** False when the Validation Gateway could not be reached; the read itself still works. */
  vgAvailable: boolean;
}

export interface CardReadResult {
  data: CardPublicData;
  verification: CardVerification;
}

export interface ReaderState {
  available: boolean;
  readerName: string | null;
  /** Why the reader is unavailable, for display at the desk. */
  detail: string;
  /** True when this is the simulator rather than real hardware. */
  simulated: boolean;
}

export interface ICardReader {
  /** Probe for the agent and an attached reader. Never throws. */
  probe(): Promise<ReaderState>;
  /** Read the chip. Throws with a message suitable for a security officer. */
  read(): Promise<CardReadResult>;
}
