import { useParams, useNavigate, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {chargeLocationsApi} from "../../api/services/chargeLocations.ts";

function LocationDetail() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const { data: location, isLoading, error } = useQuery({
        queryKey: ['location', id],
        queryFn: () => chargeLocationsApi.getById(id!),
        enabled: !!id
    });

    if (isLoading) return <div className="loading">Loading...</div>;
    if (error) return <div className="empty">Error loading location</div>;
    if (!location) return <div className="empty">Location not found</div>;

    return (
        <div className="location-detail">
            <button className="btn btn-gray back" onClick={() => navigate(-1)}>
                Back to List
            </button>

            <h1>Location Details</h1>

            <div className="card">
                <div className="card-header">
                    <h2>{location.name}</h2>
                </div>

                <div className="card-body">
                    <div className="info-grid">
                        <div className="info-item">
                            <span className="label">ID:</span>
                            <span className="value">{location.id}</span>
                        </div>
                        <div className="info-item">
                            <span className="label">Name:</span>
                            <span className="value">{location.name}</span>
                        </div>
                        <div className="info-item">
                            <span className="label">Address:</span>
                            <span className="value">{location.address}</span>
                        </div>
                        <div className="info-item">
                            <span className="label">City:</span>
                            <span className="value">{location.city}</span>
                        </div>
                        <div className="info-item">
                            <span className="label">Postal Code:</span>
                            <span className="value">{location.postalCode || 'N/A'}</span>
                        </div>
                        <div className="info-item">
                            <span className="label">Country:</span>
                            <span className="value">{location.country}</span>
                        </div>
                        <div className="info-item">
                            <span className="label">Total Charge Points:</span>
                            <span className="value">{location.totalChargePoints || 0}</span>
                        </div>
                        <div className="info-item">
                            <span className="label">Available Connectors:</span>
                            <span className="value">{location.availableConnectors || 0}</span>
                        </div>
                        <div className="info-item">
                            <span className="label">Created:</span>
                            <span className="value">
                                {new Date(location.createdAt).toLocaleDateString()}
                            </span>
                        </div>
                    </div>

                    <div className="actions">
                        <Link to="/charge-points" className="btn">
                            View Charge Points
                        </Link>
                        <Link to="/charge-points" className="btn btn-gray">
                            Add Charge Point
                        </Link>
                    </div>
                </div>
            </div>

            {location.chargePoints && location.chargePoints.length > 0 && (
                <div className="card">
                    <div className="card-header">
                        <h2>Charge Points</h2>
                    </div>
                    <div className="card-body">
                        <table>
                            <thead>
                            <tr>
                                <th>OCPP ID</th>
                                <th>Connectors</th>
                                <th>Max Power</th>
                                <th>Current Power</th>
                                <th>Actions</th>
                            </tr>
                            </thead>
                            <tbody>
                            {location.chargePoints.map(cp => (
                                <tr key={cp.id}>
                                    <td>{cp.ocppChargerId}</td>
                                    <td>{cp.totalConnectors}</td>
                                    <td>{cp.maxPower ? `${cp.maxPower} kW` : 'N/A'}</td>
                                    <td>{cp.currentPower ? `${cp.currentPower} kW` : 'N/A'}</td>
                                    <td>
                                        <Link to={`/charge-points/${cp.id}`} className="btn btn-gray">
                                            Details
                                        </Link>
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    </div>
                </div>
            )}
        </div>
    );
}

export default LocationDetail;