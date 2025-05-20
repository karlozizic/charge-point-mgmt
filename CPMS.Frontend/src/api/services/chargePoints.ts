import apiClient from '../client';
import type {
    ChargePointDto,
    ChargePointSummaryDto,
    CreateChargePointCommand,
    AddConnectorCommand,
    UpdateConnectorStatusCommand
} from '../../types/chargePoint';

export const chargePointsApi = {
    getAll: async (): Promise<ChargePointSummaryDto[]> => {
        const response = await apiClient.get<ChargePointSummaryDto[]>('/chargePoints');
        return response.data;
    },

    getById: async (id: string): Promise<ChargePointDto> => {
        const response = await apiClient.get<ChargePointDto>(`/chargePoints/${id}`);
        return response.data;
    },

    create: async (command: CreateChargePointCommand): Promise<string> => {
        const response = await apiClient.post<string>('/chargePoints', command);
        return response.data;
    },

    addConnector: async (ocppChargerId: string, command: AddConnectorCommand): Promise<void> => {
        await apiClient.post(`/chargePoints/${ocppChargerId}/connectors`, command);
    },

    updateConnectorStatus: async (
        id: string,
        connectorId: number,
        command: UpdateConnectorStatusCommand
    ): Promise<void> => {
        await apiClient.put(`/chargePoints/${id}/connectors/${connectorId}/status`, command);
    }
};
