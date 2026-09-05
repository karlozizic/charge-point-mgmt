import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { chargePointsApi } from '../../api/services/chargePoints';
import { chargeTagsApi } from '../../api/services/chargeTags';
import { chargeSessionsApi } from '../../api/services/chargeSessions';
import { chargeLocationsApi } from '../../api/services/chargeLocations';
import './Home.css';

function Home() {
    // Captured once per mount so render stays pure; the page refetches data, not the clock.
    const [now] = useState(() => Date.now());

    const { data: points = [] } = useQuery({ queryKey: ['chargePoints'], queryFn: chargePointsApi.getAll });
    const { data: tags = [] } = useQuery({ queryKey: ['chargeTags'], queryFn: chargeTagsApi.getAll });
    const { data: stats } = useQuery({ queryKey: ['sessionStats'], queryFn: chargeSessionsApi.getStats });
    const { data: activeSessions = [] } = useQuery({ queryKey: ['activeSessions'], queryFn: chargeSessionsApi.getActive });
    const { data: locations = [] } = useQuery({ queryKey: ['locations'], queryFn: chargeLocationsApi.getAll });

    const activePoints = points.filter(cp => cp.totalConnectors > 0).length;
    const blockedTags = tags.filter(tag => tag.blocked).length;

    return (
        <div className="home">
            <h1>ChargePoint Management System</h1>

            <div className="stats">
                <div className="stat-box">
                    <h3>Locations</h3>
                    <div className="stat-number">{locations.length}</div>
                    <div className="stat-info">{points.length} ChargePoints</div>
                    <Link to="/locations" className="box-link">Manage locations</Link>
                </div>
            </div>

            <div className="stats">
                <div className="stat-box">
                    <h3>ChargePoints</h3>
                    <div className="stat-number">{points.length}</div>
                    <div className="stat-info">{activePoints} with connectors</div>
                    <Link to="/charge-points" className="box-link">Manage ChargePoints</Link>
                </div>

                <div className="stat-box">
                    <h3>Tags</h3>
                    <div className="stat-number">{tags.length}</div>
                    <div className="stat-info">{blockedTags} blocked</div>
                    <Link to="/charge-tags" className="box-link">Manage tags</Link>
                </div>

                <div className="stat-box">
                    <h3>Sessions</h3>
                    <div className="stat-number">{stats?.totalSessions ?? 0}</div>
                    <div className="stat-info">{stats?.activeSessions ?? 0} active</div>
                    <Link to="/charge-sessions" className="box-link">View sessions</Link>
                </div>

                <div className="stat-box">
                    <h3>Energy</h3>
                    <div className="stat-number">{(stats?.totalEnergyDelivered ?? 0).toFixed(1)} kWh</div>
                    <div className="stat-info">Total delivered</div>
                    <Link to="/charge-sessions" className="box-link">View details</Link>
                </div>
            </div>

            {activeSessions.length > 0 && (
                <div className="active-sessions">
                    <h2>Active Sessions ({activeSessions.length})</h2>
                    <div className="session-list">
                        {activeSessions.slice(0, 3).map(session => {
                            const minutes = Math.floor((now - new Date(session.startTime).getTime()) / 60000);
                            return (
                                <div key={session.id} className="session-item">
                                    <div className="session-info">
                                        <strong>{session.tagId}</strong>
                                        <span>Connector {session.connectorId}</span>
                                        <span>{minutes}min</span>
                                    </div>
                                    <Link to={`/charge-sessions/${session.id}`} className="btn btn-sm">View</Link>
                                </div>
                            );
                        })}
                    </div>
                    {activeSessions.length > 3 && (
                        <Link to="/charge-sessions" className="view-all">
                            View all {activeSessions.length} active sessions →
                        </Link>
                    )}
                </div>
            )}
        </div>
    );
}

export default Home;
