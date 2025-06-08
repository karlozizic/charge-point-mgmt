export interface PricingGroup {
    id: string;
    name: string;
    basePrice: number;
    pricePerKwh: number;
    currency: string;
    isActive: boolean;
    chargePointIds: string[];
}

export interface SessionBilling {
    id: string;
    sessionId: string;
    pricingGroupId: string;
    baseAmount: number;
    energyAmount: number;
    totalAmount: number;
    currency: string;
    stripePaymentIntentId?: string;
    paymentStatus: string;
    paidAt?: string;
    createdAt: string;
}

export interface CreatePricingGroupCommand {
    name: string;
    basePrice: number;
    pricePerKwh: number;
    currency: string;
}

export interface PaymentIntentResponse {
    paymentIntentId: string;
    clientSecret: string;
    amount: number;
    currency: string;
}

export interface CheckoutSessionResponse {
    sessionId: string;
    url: string;
}
