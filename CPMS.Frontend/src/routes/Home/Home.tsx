import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {chargePointsApi} from "../../api/services/chargePoints.ts";
import {chargeTagsApi} from "../../api/services/chargeTags.ts";
import './Home.css';
import {chargeSessionsApi} from "../../api/services/chargeSessions.ts";
import type {ChargeSession} from "../../types/chargeSession.ts";

function Home() {
    const { data: points = [] } = useQuery({
        queryKey: ['chargePoints'],
        queryFn: chargePointsApi.getAll,
    });

    const { data: tags = [] } = useQuery({
        queryKey: ['chargeTags'],
        queryFn: chargeTagsApi.getAll,
    });

    const { data: stats } = useQuery({
        queryKey: ['sessionStats'],
        queryFn: chargeSessionsApi.getStats,
    });

    const { data: activeSessions = [] } = useQuery({
        queryKey: ['activeSessions'],
        queryFn: chargeSessionsApi.getActive,
    });

    const { data: locations = [] } = useQuery({
        queryKey: ['locations'],
        queryFn: chargePointsApi.getAll,
    });

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
                    <div className="stat-number">{stats?.totalSessions || 0}</div>
                    <div className="stat-info">{stats?.activeSessions || 0} active</div>
                    <Link to="/charge-sessions" className="box-link">View sessions</Link>
                </div>

                <div className="stat-box">
                    <h3>Energy</h3>
                    <div className="stat-number">{stats?.totalEnergyDelivered?.toFixed(1) || '0'} kWh</div>
                    <div className="stat-info">Total delivered</div>
                    <Link to="/charge-sessions" className="box-link">View details</Link>
                </div>
            </div>

            {activeSessions.length > 0 && (
                <div className="active-sessions">
                    <h2>Active Sessions ({activeSessions.length})</h2>
                    <div className="session-list">
                        {activeSessions.slice(0, 3).map((session : ChargeSession) => {
                            const duration = Math.floor((Date.now() - new Date(session.startTime).getTime()) / 60000);
                            return (
                                <div key={session.id} className="session-item">
                                    <div className="session-info">
                                        <strong>{session.tagId}</strong>
                                        <span>Connector {session.connectorId}</span>
                                        <span>{duration}min</span>
                                    </div>
                                    <Link to={`/charge-sessions/${session.id}`} className="btn btn-sm">
                                        View
                                    </Link>
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

            <div className="actions">
                <h2>Quick Actions</h2>
                <div className="action-buttons">
                    <Link to="/charge-points" className="btn">Manage ChargePoints</Link>
                    <Link to="/charge-tags" className="btn">Manage Tags</Link>
                    <Link to="/charge-sessions" className="btn">View Sessions</Link>
                </div>
            </div>
        </div>
    );
}

export default Home;