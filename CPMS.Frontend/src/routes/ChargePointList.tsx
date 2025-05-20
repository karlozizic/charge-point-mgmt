import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {chargePointsApi} from "../api/services/chargePoints.ts";
import './ChargePointList.css';

const ChargePointList = () => {
    const [showModal, setShowModal] = useState(false);
    const [newChargePoint, setNewChargePoint] = useState({
        ocppChargerId: '',
        locationId: '',
        maxPower: ''
    });
    const [error, setError] = useState('');

    const { data: chargePoints, isLoading, refetch } = useQuery({
        queryKey: ['chargePoints'],
        queryFn: () => chargePointsApi.getAll(),
    });

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setNewChargePoint({
            ...newChargePoint,
            [name]: value
        });
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        try {
            setError('');

            if (!newChargePoint.ocppChargerId || !newChargePoint.locationId) {
                setError('OCPP ID and Location ID are required');
                return;
            }

            await chargePointsApi.create({
                ocppChargerId: newChargePoint.ocppChargerId,
                locationId: newChargePoint.locationId,
                maxPower: newChargePoint.maxPower ? parseFloat(newChargePoint.maxPower) : null
            });

            setNewChargePoint({
                ocppChargerId: '',
                locationId: '',
                maxPower: ''
            });
            setShowModal(false);

            refetch();
        } catch (err) {
            console.error('Error creating charge point:', err);
            setError('Failed to create charge point');
        }
    };

    if (isLoading) return <div className="loading">Loading charge points...</div>;

    return (
        <div className="cp-list">
            <div className="flex-between">
                <h1>Charge Points</h1>
                <button className="btn" onClick={() => setShowModal(true)}>Add Charge Point</button>
            </div>

            {chargePoints?.length === 0 ? (
                <div className="empty">
                    <p>No charge points found. Add a new charge point to get started.</p>
                </div>
            ) : (
                <table>
                    <thead>
                    <tr>
                        <th>ID</th>
                        <th>OCPP ID</th>
                        <th>Location</th>
                        <th>Connectors</th>
                        <th>Max Power</th>
                        <th>Current Power</th>
                        <th>Actions</th>
                    </tr>
                    </thead>
                    <tbody>
                    {chargePoints?.map(chargePoint => (
                        <tr key={chargePoint.id}>
                            <td>{chargePoint.id.substring(0, 8)}...</td>
                            <td>{chargePoint.ocppChargerId}</td>
                            <td>{chargePoint.locationId}</td>
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
                            <h2>Add New Charge Point</h2>
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
                                    />
                                </div>
                                <div className="form-group">
                                    <label htmlFor="locationId">Location ID</label>
                                    <input
                                        type="text"
                                        id="locationId"
                                        name="locationId"
                                        value={newChargePoint.locationId}
                                        onChange={handleInputChange}
                                    />
                                </div>
                                <div className="form-group">
                                    <label htmlFor="maxPower">Max Power (kW)</label>
                                    <input
                                        type="number"
                                        id="maxPower"
                                        name="maxPower"
                                        value={newChargePoint.maxPower}
                                        onChange={handleInputChange}
                                    />
                                </div>
                                <div className="form-buttons">
                                    <button type="button" className="btn btn-gray" onClick={() => setShowModal(false)}>Cancel</button>
                                    <button type="submit" className="btn">Save</button>
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