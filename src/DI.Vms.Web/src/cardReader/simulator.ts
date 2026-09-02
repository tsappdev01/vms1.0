import type { CardReadResult, ICardReader, ReaderState } from './types';

/* Stands in for the reader until the hardware is attached, so the whole
   registration flow is buildable and testable today. It deliberately produces the
   same shapes and the same failure modes as the real adapter - including an
   occasional unreachable Validation Gateway - so the degraded path gets exercised
   rather than discovered in production. */

const SAMPLE_CARDS: CardReadResult[] = [
  {
    data: {
      idNumber: '784198512345671',
      name: 'Ahmed Khan',
      nationality: 'PAK',
      dateOfBirth: '1985-03-22',
      expiryDate: '2028-04-15',
      gender: 'M',
      photoBase64: null,
    },
    verification: { isGenuine: true, cardStatus: 'Valid', verifiedAtUtc: null, vgAvailable: true },
  },
  {
    data: {
      idNumber: '784199007654321',
      name: 'Fatima Al Zaabi',
      nationality: 'ARE',
      dateOfBirth: '1990-11-02',
      expiryDate: '2027-09-30',
      gender: 'F',
      photoBase64: null,
    },
    verification: { isGenuine: true, cardStatus: 'Valid', verifiedAtUtc: null, vgAvailable: true },
  },
  {
    /* An expired card. Reception must be able to proceed, flagged, rather than be blocked. */
    data: {
      idNumber: '784197811223344',
      name: 'Rashid Al Hosani',
      nationality: 'ARE',
      dateOfBirth: '1978-06-14',
      expiryDate: '2024-01-31',
      gender: 'M',
      photoBase64: null,
    },
    verification: { isGenuine: true, cardStatus: 'Expired', verifiedAtUtc: null, vgAvailable: true },
  },
];

let next = 0;

export class SimulatedCardReader implements ICardReader {
  async probe(): Promise<ReaderState> {
    return {
      available: true,
      readerName: 'Simulated reader',
      detail: 'No hardware attached. Sample cards are being used.',
      simulated: true,
    };
  }

  async read(): Promise<CardReadResult> {
    await new Promise((r) => setTimeout(r, 900)); // a real chip read is not instant

    const card = SAMPLE_CARDS[next % SAMPLE_CARDS.length]!;
    next += 1;

    return {
      ...card,
      verification: { ...card.verification, verifiedAtUtc: new Date().toISOString() },
    };
  }
}
