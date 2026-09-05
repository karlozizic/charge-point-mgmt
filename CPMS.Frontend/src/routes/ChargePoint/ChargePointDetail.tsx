import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { chargePointsApi } from '../../api/services/chargePoints';
import Modal from '../../components/common/Modal';
import { formatPower } from '../../utils/format';
import './ChargePointDetail.css';

const CONNECTOR_STATUSES = [
    ['Available', 'Available'],
    ['Preparing', 'Preparing'],
    ['Charging', 'Charging'],
    ['SuspendedEVSE', 'Suspended (EVSE)'],
    ['SuspendedEV', 'Suspended (EV)'],
    ['Finishing', 'Finishing'],
    ['Reserved', 'Reserved'],
    ['Unavailable', 'Unavailable'],
    ['Faulted', 'Faulted'],
] as const;

const emptyStatusForm = { status: 'Available', errorCode: '', info: '' };

const ChargePointDetail = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    const [showAddConnectorModal, setShowAddConnectorModal] = useState(false);
    const [selectedConnector, setSelectedConnector] = useState<number | null>(null);
    const [connectorName, setConnectorName] = useState('');
    const [statusForm, setStatusForm] = useState(emptyStatusForm);
    const [error, setError] = useState('');

    const { data: chargePoint, isLoading } = useQuery({
        queryKey: ['chargePoint', id],
        queryFn: () => chargePointsApi.getById(id!),
        enabled: !!id,
    });

    const invalidate = () => queryClient.invalidateQueries({ queryKey: ['chargePoint', id] });

    const addConnector = useMutation({
        mutationFn: (name: string) =>
            chargePointsApi.addConnector(chargePoint!.ocppChargerId, { ocppChargerId: chargePoint!.ocppChargerId, name }),
        onSuccess: () => {
            invalidate();
            setShowAddConnectorModal(false);
            setConnectorName('');
        },
        onError: () => setError('Failed to add connector'),
    });

    const updateStatus = useMutation({
        mutationFn: (connectorId: number) =>
            chargePointsApi.updateConnectorStatus(id!, connectorId, {
                ocppChargerId: chargePoint!.ocppChargerId,
                chargePointId: id!,
                connectorId,
                ...statusForm,
                timestamp: new Date().toISOString(),
            }),
        onSuccess: () => {
            invalidate();
            setSelectedConnector(null);
            setStatusForm(emptyStatusForm);
        },
        onError: () => setError('Failed to update connector status'),
    });

    const handleAddConnector = (e: React.FormEvent) => {
        e.preventDefault();
        if (!connectorName) {
            setError('Connector name is required');
            return;
        }
        addConnector.mutate(connectorName);
    };

    const handleUpdateStatus = (e: React.FormEvent) => {
        e.preventDefault();
        if (selectedConnector !== null) updateStatus.mutate(selectedConnector);
    };

    const openUpdateStatusModal = (connectorId: number) => {
        const connector = chargePoint?.connectors?.find(c => c.id === connectorId);
        setSelectedConnector(connectorId);
        setStatusForm({ ...emptyStatusForm, status: connector?.status || 'Available' });
    };

    if (isLoading) return <div className="loading">Loading ChargePoint details...</div>;
    if (!chargePoint) return <div className="empty">ChargePoint not found</div>;

    return (
        <div className="cp-detail">
            <button className="btn btn-gray back" onClick={() => navigate(-1)}>Back to List</button>

            <h1>ChargePoint Details</h1>

            <div className="card">
                <div className="card-header">
                    <h2>{chargePoint.ocppChargerId}</h2>
                </div>
                <div className="card-body">
                    <div className="info-grid">
                        <div className="info-item"><span className="label">ID:</span><span className="value">{chargePoint.id}</span></div>
                        <div className="info-item"><span className="label">OCPP ID:</span><span className="value">{chargePoint.ocppChargerId}</span></div>
                        <div className="info-item"><span className="label">Location:</span><span className="value">{chargePoint.locationId}</span></div>
                        <div className="info-item"><span className="label">Max Power:</span><span className="value">{formatPower(chargePoint.maxPower)}</span></div>
                        <div className="info-item"><span className="label">Current Power:</span><span className="value">{formatPower(chargePoint.currentPower)}</span></div>
                    </div>
                </div>
            </div>

            <div className="connectors-section">
                <div className="section-header">
                    <h2>Connectors</h2>
                    <button className="btn" onClick={() => setShowAddConnectorModal(true)}>Add Connector</button>
                </div>

                {!chargePoint.connectors?.length ? (
                    <div className="empty">
                        <p>No connectors available. Add a connector to get started.</p>
                    </div>
                ) : (
                    <table>
                        <thead>
                        <tr>
                            <th>ID</th>
                            <th>Name</th>
                            <th>Status</th>
                            <th>Actions</th>
                        </tr>
                        </thead>
                        <tbody>
                        {chargePoint.connectors.map(connector => (
                            <tr key={connector.id}>
                                <td>{connector.id}</td>
                                <td>{connector.name}</td>
                                <td>
                                    <span className={`status-badge ${connector.status?.toLowerCase() || 'available'}`}>
                                        {connector.status || 'Available'}
                                    </span>
                                </td>
                                <td>
                                    <button className="btn btn-gray" onClick={() => openUpdateStatusModal(connector.id)}>
                                        Update Status
                                    </button>
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                )}
            </div>

            {showAddConnectorModal && (
                <Modal title="Add New Connector" onClose={() => setShowAddConnectorModal(false)} error={error}>
                    <form onSubmit={handleAddConnector}>
                        <div className="form-group">
                            <label htmlFor="connectorName">Connector Name</label>
                            <input
                                type="text"
                                id="connectorName"
                                value={connectorName}
                                onChange={(e) => setConnectorName(e.target.value)}
                            />
                        </div>
                        <div className="form-buttons">
                            <button type="button" className="btn btn-gray" onClick={() => setShowAddConnectorModal(false)}>Cancel</button>
                            <button type="submit" className="btn" disabled={addConnector.isPending}>
                                {addConnector.isPending ? 'Saving...' : 'Save'}
                            </button>
                        </div>
                    </form>
                </Modal>
            )}

            {selectedConnector !== null && (
                <Modal title="Update Connector Status" onClose={() => setSelectedConnector(null)} error={error}>
                    <form onSubmit={handleUpdateStatus}>
                        <div className="form-group">
                            <label htmlFor="status">Status</label>
                            <select
                                id="status"
                                value={statusForm.status}
                                onChange={(e) => setStatusForm({ ...statusForm, status: e.target.value })}
                            >
                                {CONNECTOR_STATUSES.map(([value, label]) => (
                                    <option key={value} value={value}>{label}</option>
                                ))}
                            </select>
                        </div>
                        <div className="form-group">
                            <label htmlFor="errorCode">Error Code (optional)</label>
                            <input
                                type="text"
                                id="errorCode"
                                value={statusForm.errorCode}
                                onChange={(e) => setStatusForm({ ...statusForm, errorCode: e.target.value })}
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="info">Additional Info (optional)</label>
                            <textarea
                                id="info"
                                rows={3}
                                value={statusForm.info}
                                onChange={(e) => setStatusForm({ ...statusForm, info: e.target.value })}
                            />
                        </div>
                        <div className="form-buttons">
                            <button type="button" className="btn btn-gray" onClick={() => setSelectedConnector(null)}>Cancel</button>
                            <button type="submit" className="btn" disabled={updateStatus.isPending}>
                                {updateStatus.isPending ? 'Updating...' : 'Update'}
                            </button>
                        </div>
                    </form>
                </Modal>
            )}
        </div>
    );
};

export default ChargePointDetail;
