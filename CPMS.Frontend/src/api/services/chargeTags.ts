import apiClient from '../client';
import type {
    ChargeTagReadModel,
    CreateChargeTagCommand,
    UpdateChargeTagCommand,
    UpdateChargeTagExpiryCommand,
    AuthorizeTagCommand
} from '../../types/chargeTag';

export const chargeTagsApi = {
    getAll: async (): Promise<ChargeTagReadModel[]> => {
        const response = await apiClient.get<ChargeTagReadModel[]>('/chargeTags');
        return response.data;
    },

    getById: async (id: string): Promise<ChargeTagReadModel> => {
        const response = await apiClient.get<ChargeTagReadModel>(`/chargeTags/${id}`);
        return response.data;
    },

    getByTagId: async (tagId: string): Promise<ChargeTagReadModel> => {
        const response = await apiClient.get<ChargeTagReadModel>(`/chargeTags/byTagId/${tagId}`);
        return response.data;
    },

    create: async (command: CreateChargeTagCommand): Promise<string> => {
        const response = await apiClient.post<string>('/chargeTags', command);
        return response.data;
    },

    update: async (id: string, command: UpdateChargeTagCommand): Promise<void> => {
        await apiClient.put(`/chargeTags/${id}`, command);
    },

    block: async (id: string): Promise<void> => {
        await apiClient.post(`/chargeTags/${id}/block`);
    },

    unblock: async (id: string): Promise<void> => {
        await apiClient.post(`/chargeTags/${id}/unblock`);
    },

    updateExpiry: async (id: string, command: UpdateChargeTagExpiryCommand): Promise<void> => {
        await apiClient.put(`/chargeTags/${id}/expiry`, command);
    },

    authorize: async (command: AuthorizeTagCommand): Promise<boolean> => {
        const response = await apiClient.post<boolean>('/chargeTags/authorize', command);
        return response.data;
    }
};