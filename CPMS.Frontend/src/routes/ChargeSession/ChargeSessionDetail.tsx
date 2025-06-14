import { useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { chargeSessionsApi } from "../../api/services/chargeSessions.ts";
import type { MeterValue } from "../../types/chargeSession.ts";
import SimpleSessionPayment from "../Billing/SimpleSessionPayment.tsx";

function ChargeSessionDetail() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const { data: session, isLoading } = useQuery({
        queryKey: ['chargeSession', id],
        queryFn: () => chargeSessionsApi.getById(id!),
        enabled: !!id
    });

    if (isLoading) return <div className="loading">Loading...</div>;
    if (!session) return <div className="empty">Session not found</div>;

    const calculateEnergy = (): string => {
        if (session.energyDeliveredKWh && session.energyDeliveredKWh > 0) {
            return session.energyDeliveredKWh.toFixed(2);
        }
        return '0.00';
    };

    const formatDuration = (): string => {
        if (!session.stopTime) return 'Ongoing';
        const diff = new Date(session.stopTime).getTime() - new Date(session.startTime).getTime();
        const hours = Math.floor(diff / 3600000);
        const minutes = Math.floor((diff % 3600000) / 60000);
        return `${hours}h ${minutes}m`;
    };


    const handleExportToCsv = async () => {
        const response = await fetch(`/api/chargesessions/${id}/export/csv`);

        if (!response.ok) {
            alert('Export failed. Please check console for details.');
            return;
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `session-${session.transactionId}-${new Date().toISOString().split('T')[0]}.csv`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
    };


    const isSessionCompleted = session.status === 'Stopped';

    return (
        <div className="session-detail">
            <button className="btn btn-gray" onClick={() => navigate(-1)}>
                ← Back
            </button>

            <div className="session-header">
                <h1>Session #{session.transactionId}</h1>
                <span className={`status ${session.status === 'Started' ? 'active' : 'completed'}`}>
                    {session.status}
                </span>
                <button className="btn btn-primary" onClick={handleExportToCsv}>
                    Export CSV
                </button>
            </div>

            <div className="session-content">
                <div className="session-info">
                    <div className="info-section">
                        <h3>Basic Info</h3>
                        <div className="info-grid">
                            <div>Tag ID: <strong>{session.tagId}</strong></div>
                            <div>ChargePoint: <strong>{session.chargePointId}</strong></div>
                            <div>Connector: <strong>{session.connectorId}</strong></div>
                            <div>Start: <strong>{new Date(session.startTime).toLocaleString()}</strong></div>
                            <div>Stop: <strong>{session.stopTime ? new Date(session.stopTime).toLocaleString() : 'Ongoing'}</strong></div>
                            <div>Duration: <strong>{formatDuration()}</strong></div>
                        </div>
                    </div>

                    <div className="info-section">
                        <h3>Energy</h3>
                        <div className="energy-info">
                            <div className="energy-main">{calculateEnergy()} kWh</div>
                            <div className="energy-details">
                                <div>Start: {session.startMeterValue.toFixed(2)} Wh</div>
                                {session.stopMeterValue && (
                                    <div>Stop: {session.stopMeterValue.toFixed(2)} Wh</div>
                                )}
                            </div>
                        </div>
                    </div>
                </div>

                {isSessionCompleted && (
                    <div className="payment-section">
                        <SimpleSessionPayment sessionId={id!} />
                    </div>
                )}

                {session.meterValues?.length > 0 && (
                    <div className="meter-values">
                        <h3>Meter Readings ({session.meterValues.length})</h3>
                        <div className="meter-table">
                            <table>
                                <thead>
                                <tr>
                                    <th>Time</th>
                                    <th>Power (kW)</th>
                                    <th>Energy (kWh)</th>
                                    <th>SoC (%)</th>
                                </tr>
                                </thead>
                                <tbody>
                                {session.meterValues.map((reading: MeterValue, i: number) => (
                                    <tr key={i}>
                                        <td>{new Date(reading.timestamp).toLocaleString()}</td>
                                        <td>{reading.currentPower?.toFixed(1) || '-'}</td>
                                        <td>{reading.energyConsumed?.toFixed(3) || '-'}</td>
                                        <td>{reading.stateOfCharge?.toFixed(1) || '-'}</td>
                                    </tr>
                                ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}

export default ChargeSessionDetail;