import apiClient from '../client';
import type {
    PricingGroup,
    SessionBilling,
    CreatePricingGroupCommand,
    CheckoutSessionResponse
} from '../../types/billing';

export const billingApi = {
    getPricingGroups: async (): Promise<PricingGroup[]> => {
        const response = await apiClient.get<PricingGroup[]>('/pricing/groups');
        return response.data;
    },

    createPricingGroup: async (command: CreatePricingGroupCommand): Promise<string> => {
        const response = await apiClient.post<string>('/pricing/groups', command);
        return response.data;
    },

    assignChargePointToPricingGroup: async (pricingGroupId: string, chargePointId: string): Promise<void> => {
        await apiClient.post(`/pricing/groups/${pricingGroupId}/assign-chargepoint/${chargePointId}`);
    },

    getSessionBilling: async (sessionId: string): Promise<SessionBilling> => {
        const response = await apiClient.get<SessionBilling>(`/billing/sessions/${sessionId}`);
        return response.data;
    },

    createCheckoutSession: async (sessionId: string, customerEmail: string): Promise<CheckoutSessionResponse> => {
        const response = await apiClient.post<CheckoutSessionResponse>(
            `/billing/sessions/${sessionId}/checkout`,
            { customerEmail }
        );
        return response.data;
    },

    getUnpaidBillings: async (): Promise<SessionBilling[]> => {
        const response = await apiClient.get<SessionBilling[]>('/billing/unpaid');
        return response.data;
    }
};