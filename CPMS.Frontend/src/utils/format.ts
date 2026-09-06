export function formatDuration(startTime: string, stopTime?: string): string {
    if (!stopTime) return 'Ongoing';
    const diff = new Date(stopTime).getTime() - new Date(startTime).getTime();
    const hours = Math.floor(diff / 3600000);
    const minutes = Math.floor((diff % 3600000) / 60000);
    return `${hours}h ${minutes}m`;
}

export function formatEnergy(kWh?: number): string {
    return kWh && kWh > 0 ? kWh.toFixed(2) : '0.00';
}

export function formatPower(kW: number | null | undefined): string {
    return kW ? `${kW} kW` : 'N/A';
}

export function shortId(id: string): string {
    return `${id.slice(0, 8)}...`;
}
