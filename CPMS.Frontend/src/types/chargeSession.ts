
export interface ChargeSession {
    id: string;
    transactionId: number;
    chargePointId: string;
    connectorId: number;
    tagId: string;
    startTime: string;
    stopTime?: string;
    startMeterValue: number;
    stopMeterValue?: number;
    energyDeliveredKWh?: number;
    status: string;
    stopReason?: string;
    meterValues: MeterValue[];
}

export interface MeterValue {
    timestamp: string;
    currentPower?: number;
    energyConsumed?: number;
    stateOfCharge?: number;
}

export interface SessionStats {
    totalSessions: number;
    activeSessions: number;
    completedSessions: number;
    totalEnergyDelivered: number;
}

export interface SessionFilters {
    status?: string;
    tagId?: string;
    startDate?: string;
    endDate?: string;
}