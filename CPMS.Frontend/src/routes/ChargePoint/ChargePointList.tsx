import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { chargePointsApi } from '../../api/services/chargePoints';
import { chargeLocationsApi } from '../../api/services/chargeLocations';
import type { CreateChargePointCommand } from '../../types/chargePoint';
import Modal from '../../components/common/Modal';
import { formatPower, shortId } from '../../utils/format';
import './ChargePointList.css';

const emptyForm = { ocppChargerId: '', locationId: '', maxPower: '' };

const ChargePointList = () => {
    const [showModal, setShowModal] = useState(false);
    const [form, setForm] = useState(emptyForm);
    const [error, setError] = useState('');

    const queryClient = useQueryClient();

    const { data: chargePoints = [], isLoading } = useQuery({
        queryKey: ['chargePoints'],
        queryFn: chargePointsApi.getAll,
    });

    const { data: locations = [] } = useQuery({
        queryKey: ['locations'],
        queryFn: chargeLocationsApi.getAll,
    });

    const createChargePoint = useMutation({
        mutationFn: (command: CreateChargePointCommand) => chargePointsApi.create(command),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['chargePoints'] });
            closeModal();
        },
    });

    const closeModal = () => {
        setShowModal(false);
        setForm(emptyForm);
        setError('');
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        setForm({ ...form, [e.target.name]: e.target.value });
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        if (!form.ocppChargerId || !form.locationId) {
            setError('OCPP ID and Location are required');
            return;
        }

        createChargePoint.mutate({
            ocppChargerId: form.ocppChargerId,
            locationId: form.locationId,
            maxPower: form.maxPower ? parseFloat(form.maxPower) : null,
        });
    };

    const getLocationName = (locationId: string): string =>
        locations.find(l => l.id === locationId)?.name || shortId(locationId);

    if (isLoading) return <div className="loading">Loading ChargePoints...</div>;

    return (
        <div className="cp-list">
            <div className="flex-between">
                <h1>ChargePoints</h1>
                <div className="header-actions">
                    <Link to="/locations" className="btn btn-gray">Manage Locations</Link>
                    <button className="btn" onClick={() => setShowModal(true)}>Add ChargePoint</button>
                </div>
            </div>

            {chargePoints.length === 0 ? (
                <div className="empty">
                    <p>No ChargePoints found.</p>
                    <p>Add a location first, then create ChargePoints.</p>
                </div>
            ) : (
                <table>
                    <thead>
                    <tr>
                        <th>OCPP ID</th>
                        <th>Location</th>
                        <th>Connectors</th>
                        <th>Max Power</th>
                        <th>Current Power</th>
                        <th>Actions</th>
                    </tr>
                    </thead>
                    <tbody>
                    {chargePoints.map(chargePoint => (
                        <tr key={chargePoint.id}>
                            <td>{chargePoint.ocppChargerId}</td>
                            <td>{getLocationName(chargePoint.locationId)}</td>
                            <td>{chargePoint.totalConnectors}</td>
                            <td>{formatPower(chargePoint.maxPower)}</td>
                            <td>{formatPower(chargePoint.currentPower)}</td>
                            <td>
                                <Link to={`/charge-points/${chargePoint.id}`} className="btn btn-gray">Details</Link>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            )}

            {showModal && (
                <Modal title="Add New ChargePoint" onClose={closeModal} error={error}>
                    <form onSubmit={handleSubmit}>
                        <div className="form-group">
                            <label htmlFor="ocppChargerId">OCPP ID</label>
                            <input
                                type="text"
                                id="ocppChargerId"
                                name="ocppChargerId"
                                value={form.ocppChargerId}
                                onChange={handleInputChange}
                                placeholder="e.g. CP-001"
                                required
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="locationId">Location</label>
                            <select
                                id="locationId"
                                name="locationId"
                                value={form.locationId}
                                onChange={handleInputChange}
                                required
                            >
                                <option value="">Select location...</option>
                                {locations.map(location => (
                                    <option key={location.id} value={location.id}>
                                        {location.name} - {location.city}
                                    </option>
                                ))}
                            </select>
                            {locations.length === 0 && (
                                <p className="helper-text">
                                    <Link to="/locations">Create a location first</Link>
                                </p>
                            )}
                        </div>
                        <div className="form-group">
                            <label htmlFor="maxPower">Max Power (kW)</label>
                            <input
                                type="number"
                                id="maxPower"
                                name="maxPower"
                                value={form.maxPower}
                                onChange={handleInputChange}
                                placeholder="e.g. 22"
                                step="0.1"
                                min="0"
                            />
                        </div>
                        <div className="form-buttons">
                            <button type="button" className="btn btn-gray" onClick={closeModal}>Cancel</button>
                            <button type="submit" className="btn" disabled={createChargePoint.isPending}>
                                {createChargePoint.isPending ? 'Creating...' : 'Create'}
                            </button>
                        </div>
                    </form>
                </Modal>
            )}
        </div>
    );
};

export default ChargePointList;
