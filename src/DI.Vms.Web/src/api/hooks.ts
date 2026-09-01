import { useQuery } from '@tanstack/react-query';
import { api } from './index';
import type { VisitorSearchQuery } from './types';

export const useCurrentUser = () =>
  useQuery({ queryKey: ['me'], queryFn: () => api.getCurrentUser() });

export const useDashboardSummary = () =>
  useQuery({ queryKey: ['dashboard'], queryFn: () => api.getDashboardSummary(), refetchInterval: 30_000 });

export const useInside = () =>
  useQuery({ queryKey: ['inside'], queryFn: () => api.getInside(), refetchInterval: 15_000 });

export const useExpected = () =>
  useQuery({ queryKey: ['expected'], queryFn: () => api.getExpected() });

export const useOccupancy = () =>
  useQuery({ queryKey: ['occupancy'], queryFn: () => api.getOccupancy(), refetchInterval: 10_000 });

export const useVisitSearch = (q: VisitorSearchQuery) =>
  useQuery({ queryKey: ['search', q], queryFn: () => api.searchVisits(q) });

export const useEntities = () =>
  useQuery({ queryKey: ['entities'], queryFn: () => api.getEntities() });

export const useEmployees = () =>
  useQuery({ queryKey: ['employees'], queryFn: () => api.getEmployees() });

export const useUsers = () =>
  useQuery({ queryKey: ['users'], queryFn: () => api.getUsers() });

export const useAudit = () =>
  useQuery({ queryKey: ['audit'], queryFn: () => api.getAudit() });
