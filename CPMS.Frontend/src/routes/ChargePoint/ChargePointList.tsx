import { useState } from 'react';
import { Link } from 'react-router-dom';
import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query';
import {chargePointsApi} from "../../api/services/chargePoints.ts";
import './ChargePointList.css';
import {chargeLocationsApi} from "../../api/services/chargeLocations.ts";
import type {CreateChargePointCommand} from "../../types/chargePoint.ts";

const ChargePointList = () => {
    const [showModal, setShowModal] = useState(false);
    const [newChargePoint, setNewChargePoint] = useState({
        ocppChargerId: '',
        locationId: '',
        maxPower: ''
    });
    const [error, setError] = useState('');

    const queryClient = useQueryClient();

    const { data: chargePoints = [], isLoading } = useQuery({
        queryKey: ['chargePoints'],
        queryFn: chargePointsApi.getAll,
    });

    const { data: locations = [] } = useQuery({
        queryKey: ['locations'],
        queryFn: () => chargeLocationsApi.getAll(),
    });

    const createChargePoint = useMutation({
        mutationFn: (command: CreateChargePointCommand) => chargePointsApi.create(command),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['chargePoints'] });
            setNewChargePoint({
                ocppChargerId: '',
                locationId: '',
                maxPower: ''
            });
            setShowModal(false);
            setError('');
        }
    });

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setNewChargePoint({
            ...newChargePoint,
            [name]: value
        });
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        if (!newChargePoint.ocppChargerId || !newChargePoint.locationId) {
            setError('OCPP ID and Location are required');
            return;
        }

        createChargePoint.mutate({
            ocppChargerId: newChargePoint.ocppChargerId,
            locationId: newChargePoint.locationId,
            maxPower: newChargePoint.maxPower ? parseFloat(newChargePoint.maxPower) : null
        });
    };

    const getLocationName = (locationId: string): string => {
        const location = locations.find(l => l.id === locationId);
        return location?.name || locationId.slice(0, 8) + '...';
    };

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
                            <td>{chargePoint.maxPower ? `${chargePoint.maxPower} kW` : 'N/A'}</td>
                            <td>{chargePoint.currentPower ? `${chargePoint.currentPower} kW` : 'N/A'}</td>
                            <td>
                                <Link to={`/charge-points/${chargePoint.id}`} className="btn btn-gray">Details</Link>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            )}

            {/* Modal */}
            {showModal && (
                <div className="overlay">
                    <div className="modal">
                        <div className="modal-header">
                            <h2>Add New ChargePoint</h2>
                            <button className="close" onClick={() => setShowModal(false)}>&times;</button>
                        </div>
                        <div className="modal-body">
                            {error && <div className="error-msg">{error}</div>}
                            <form onSubmit={handleSubmit}>
                                <div className="form-group">
                                    <label htmlFor="ocppChargerId">OCPP ID</label>
                                    <input
                                        type="text"
                                        id="ocppChargerId"
                                        name="ocppChargerId"
                                        value={newChargePoint.ocppChargerId}
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
                                        value={newChargePoint.locationId}
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
                                        value={newChargePoint.maxPower}
                                        onChange={handleInputChange}
                                        placeholder="e.g. 22"
                                        step="0.1"
                                        min="0"
                                    />
                                </div>
                                <div className="form-buttons">
                                    <button type="button" className="btn btn-gray" onClick={() => setShowModal(false)}>
                                        Cancel
                                    </button>
                                    <button type="submit" className="btn" disabled={createChargePoint.isPending}>
                                        {createChargePoint.isPending ? 'Creating...' : 'Create'}
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ChargePointList;