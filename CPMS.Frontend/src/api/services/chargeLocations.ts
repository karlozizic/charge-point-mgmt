import apiClient from "../client.ts";
import type {ChargeLocation, CreateLocationCommand, UpdateLocationCommand} from "../../types/chargeLocation.ts";


export const chargeLocationsApi = {
    getAll: async (): Promise<ChargeLocation[]> => {
        const response = await apiClient.get<{ items?: ChargeLocation[] } | ChargeLocation[]>('/locations');
        const data = response.data;
        return Array.isArray(data) ? data : (data.items || []);
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
    }
};