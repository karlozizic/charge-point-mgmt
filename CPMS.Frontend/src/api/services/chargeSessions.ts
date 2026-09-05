import apiClient from '../client';
import type { PagedResult } from '../../types/common';
import type { ChargeSession, SessionFilters, SessionStats } from '../../types/chargeSession';

// The list pages have no paging controls, so one page holds everything they show.
const PAGE_SIZE = 100;

export const chargeSessionsApi = {
    getAll: async (filters: SessionFilters = {}): Promise<ChargeSession[]> => {
        const response = await apiClient.get<PagedResult<ChargeSession>>('/chargeSessions', {
            params: { ...filters, pageSize: PAGE_SIZE },
        });
        return response.data.items;
    },

    getById: async (id: string): Promise<ChargeSession> => {
        const response = await apiClient.get<ChargeSession>(`/chargeSessions/${id}`);
        return response.data;
    },

    getActive: async (): Promise<ChargeSession[]> => {
        const response = await apiClient.get<ChargeSession[]>('/chargeSessions/active');
        return response.data;
    },

    getStats: async (): Promise<SessionStats> => {
        const response = await apiClient.get<SessionStats>('/chargeSessions/stats');
        return response.data;
    },

    exportToCsv: async (sessionId: string): Promise<void> => {
        const response = await apiClient.get<Blob>(`/chargeSessions/${sessionId}/export/csv`, {
            responseType: 'blob',
        });

        const url = window.URL.createObjectURL(response.data);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `session-${sessionId}.csv`);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    },
};
