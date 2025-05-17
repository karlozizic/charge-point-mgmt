export interface ChargePointSummaryDto {
    id: string;
    ocppChargerId: string;
    locationId: string;
    totalConnectors: number;
    maxPower: number | null;
    currentPower: number | null;
}

export interface ConnectorDto {
    id: number;
    name: string;
    status: string;
}

export interface ChargePointDto {
    id: string;
    ocppChargerId: string;
    locationId: string;
    maxPower: number | null;
    currentPower: number | null;
    connectors: ConnectorDto[] | null;
}

export interface CreateChargePointCommand {
    ocppChargerId: string;
    locationId: string;
    maxPower: number | null;
}

export interface AddConnectorCommand {
    ocppChargerId: string;
    name: string;
}

export interface UpdateConnectorStatusCommand {
    ocppChargerId: string;
    chargePointId: string;
    connectorId: number;
    status: string;
    errorCode: string;
    info: string;
    timestamp: string | null;
}