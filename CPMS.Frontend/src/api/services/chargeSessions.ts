import apiClient from "../client.ts";
import type {SessionFilters} from "../../types/chargeSession.ts";

export const chargeSessionsApi = {
    getAll: async (filters: SessionFilters = {}) => {
        const params = new URLSearchParams();
        if (filters.status) params.append('status', filters.status);
        if (filters.tagId) params.append('tagId', filters.tagId);
        if (filters.startDate) params.append('startDate', filters.startDate);
        if (filters.endDate) params.append('endDate', filters.endDate);

        const response = await apiClient.get(`/chargeSessions?${params}`);
        return response.data.items || response.data;
    },

    getById: async (id: string | undefined) => {
        const response = await apiClient.get(`/chargeSessions/${id}`);
        return response.data;
    },

    getActive: async () => {
        const response = await apiClient.get('/chargeSessions/active');
        return response.data;
    },

    getStats: async () => {
        const response = await apiClient.get('/chargeSessions/stats');
        return response.data;
    }
};