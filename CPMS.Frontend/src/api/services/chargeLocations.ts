import apiClient from '../client';
import type { PagedResult } from '../../types/common';
import type { ChargeLocation, CreateLocationCommand, UpdateLocationCommand } from '../../types/chargeLocation';

const PAGE_SIZE = 100;

export const chargeLocationsApi = {
    getAll: async (): Promise<ChargeLocation[]> => {
        const response = await apiClient.get<PagedResult<ChargeLocation>>('/locations', {
            params: { pageSize: PAGE_SIZE },
        });
        return response.data.items;
    },

    getById: async (id: string): Promise<ChargeLocation> => {
        const response = await apiClient.get<ChargeLocation>(`/locations/${id}`);
        return response.data;
    },

    create: async (command: CreateLocationCommand): Promise<string> => {
        const response = await apiClient.post<string>('/locations', command);
        return response.data;
    },

    update: async (command: UpdateLocationCommand): Promise<void> => {
        await apiClient.put('/locations', command);
    },
};
