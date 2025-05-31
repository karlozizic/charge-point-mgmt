import { useState} from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {chargeSessionsApi} from "../../api/services/chargeSessions.ts";
import './ChargeSessionList.css';
import type {ChargeSession} from "../../types/chargeSession.ts";

function ChargeSessionList() {
    const [filters, setFilters] = useState({});
    const [showFilters, setShowFilters] = useState(false);

    const { data: sessions = [], isLoading } = useQuery({
        queryKey: ['chargeSessions', filters],
        queryFn: () => chargeSessionsApi.getAll(filters),
    });

    const { data: stats } = useQuery({
        queryKey: ['sessionStats'],
        queryFn: () => chargeSessionsApi.getStats(),
    });

    const calculateEnergy = (session : ChargeSession) => {
        if (session.energyDeliveredKWh && session.energyDeliveredKWh > 0) {
            return session.energyDeliveredKWh.toFixed(2);
        }

        return '0.00';
    };

    const formatDuration = (startTime: string, stopTime?: string) => {
        if (!stopTime) return 'Ongoing';
        const diff = new Date(stopTime).getTime() - new Date(startTime).getTime();
        const hours = Math.floor(diff / 3600000);
        const minutes = Math.floor((diff % 3600000) / 60000);
        return `${hours}h ${minutes}m`;
    };

    const getStatusClass = (status: string) => {
        return status === 'Started' ? 'status-active' : 'status-completed';
    };

    if (isLoading) return <div className="loading">Loading sessions...</div>;

    return (
        <div className="sessions-page">
            <div className="page-header">
                <h1>Sessions</h1>
                <button
                    className="btn btn-gray"
                    onClick={() => setShowFilters(!showFilters)}
                >
                    Filters
                </button>
            </div>

            {stats && (
                <div className="stats-row">
                    <div className="stat-box">
                        <span className="stat-number">{stats.totalSessions}</span>
                        <span className="stat-label">Total</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-number active">{stats.activeSessions}</span>
                        <span className="stat-label">Active</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-number">{stats.completedSessions}</span>
                        <span className="stat-label">Completed</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-number">{stats.totalEnergyDelivered?.toFixed(1) || '0'} kWh</span>
                        <span className="stat-label">Total Energy</span>
                    </div>
                </div>
            )}

            {showFilters && (
                <div className="filters">
                    <select onChange={(e) => setFilters({...filters, status: e.target.value})}>
                        <option value="">All Status</option>
                        <option value="Started">Active</option>
                        <option value="Stopped">Completed</option>
                    </select>
                    <input
                        type="text"
                        placeholder="Tag ID"
                        onChange={(e) => setFilters({...filters, tagId: e.target.value})}
                    />
                    <input
                        type="date"
                        onChange={(e) => setFilters({...filters, startDate: e.target.value})}
                    />
                </div>
            )}

            <div className="sessions-table">
                <table>
                    <thead>
                    <tr>
                        <th>ID</th>
                        <th>Tag</th>
                        <th>ChargePoint</th>
                        <th>Start Time</th>
                        <th>Duration</th>
                        <th>Energy</th>
                        <th>Status</th>
                        <th></th>
                    </tr>
                    </thead>
                    <tbody>
                    {sessions.map((session: ChargeSession) => (
                        <tr key={session.id}>
                            <td>{session.transactionId}</td>
                            <td>{session.tagId}</td>
                            <td>{session.chargePointId.slice(0, 8)}...</td>
                            <td>{new Date(session.startTime).toLocaleString()}</td>
                            <td>{formatDuration(session.startTime, session.stopTime)}</td>
                            <td>{calculateEnergy(session)} kWh</td>
                            <td>
                                    <span className={`status ${getStatusClass(session.status)}`}>
                                        {session.status}
                                    </span>
                            </td>
                            <td>
                                <Link to={`/charge-sessions/${session.id}`} className="btn btn-sm">
                                    View
                                </Link>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default ChargeSessionList;