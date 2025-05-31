export interface ChargeLocation {
    id: string;
    name: string;
    address: string;
    city: string;
    postalCode: string;
    country: string;
    latitude?: number;
    longitude?: number;
    description?: string;
    createdAt: string;
    totalChargePoints: number;
    availableConnectors: number;
    chargePoints?: ChargePointSummary[];
}

export interface ChargePointSummary {
    id: string;
    ocppChargerId: string;
    locationId: string;
    totalConnectors: number;
    maxPower: number | null;
    currentPower: number | null;
}

export interface CreateLocationCommand {
    name: string;
    address: string;
    city: string;
    postalCode: string;
    country: string;
    latitude?: number;
    longitude?: number;
    description?: string;
}

export interface UpdateLocationCommand {
    id: string;
    name: string;
    address: string;
    city: string;
    postalCode: string;
    country: string;
    latitude?: number;
    longitude?: number;
    description?: string;
}