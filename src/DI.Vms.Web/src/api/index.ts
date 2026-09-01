import { httpClient } from './client';
import { mockClient, type ApiClient } from './mock/mockClient';

/* Mock is the default: .env is gitignored, so a fresh clone has no VITE_USE_MOCK
   and must still run. Point at the real API by setting VITE_USE_MOCK=false
   explicitly - see .env.example. */
export const usingMock = import.meta.env.VITE_USE_MOCK !== 'false';

export const api: ApiClient = usingMock ? mockClient : httpClient;
