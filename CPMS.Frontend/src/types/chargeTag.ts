export interface ChargeTagReadModel {
    id: string;
    tagId: string;
    expiryDate: string | null | undefined;
    blocked: boolean;
}

export interface CreateChargeTagCommand {
    tagId: string;
    expiryDate: string | null;
}

export interface UpdateChargeTagCommand {
    id: string;
    tagId: string;
    expiryDate: string | null | undefined;
}

export interface UpdateChargeTagExpiryCommand {
    id: string;
    expiryDate: string | null;
}

export interface AuthorizeTagCommand {
    tagId: string;
}