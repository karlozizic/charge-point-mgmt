import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {chargePointsApi} from "../api/services/chargePoints.ts";
import {chargeTagsApi} from "../api/services/chargeTags.ts";
import './Home.css';

function Home() {
    const { data: points, isLoading: loadingPoints } = useQuery({
        queryKey: ['chargePoints'],
        queryFn: () => chargePointsApi.getAll(),
    });

    const { data: tags, isLoading: loadingTags } = useQuery({
        queryKey: ['chargeTags'],
        queryFn: () => chargeTagsApi.getAll(),
    });

    const totalPoints = points?.length || 0;
    const activePoints = points?.filter(cp =>
        cp.totalConnectors > 0).length || 0;

    const totalTags = tags?.length || 0;
    const blockedTags = tags?.filter(tag =>
        tag.blocked).length || 0;

    if (loadingPoints || loadingTags) {
        return <div className="loading">Loading...</div>;
    }

    return (
        <div className="home">
            <h1>Charge Point Management System</h1>
            <div className="stats">
                <div className="stat-box">
                    <h3>Charge Points</h3>
                    <div className="stat-number">{totalPoints}</div>
                    <div className="stat-info">
                        <span>{activePoints} active</span>
                    </div>
                    <Link to="/charge-points" className="box-link">View all charge points</Link>
                </div>

                <div className="stat-box">
                    <h3>Charge Tags</h3>
                    <div className="stat-number">{totalTags}</div>
                    <div className="stat-info">
                        <span>{blockedTags} blocked</span>
                    </div>
                    <Link to="/charge-tags" className="box-link">View all charge tags</Link>
                </div>
            </div>

            <div className="actions">
                <h2>Quick Actions</h2>
                <div className="btn-list">
                    <Link to="/charge-points" className="btn">
                        Manage Charge Points
                    </Link>
                    <Link to="/charge-tags" className="btn">
                        Manage Charge Tags
                    </Link>
                </div>
            </div>
        </div>
    );
}

export default Home;