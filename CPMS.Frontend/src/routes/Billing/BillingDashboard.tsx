import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { billingApi } from '../../api/services/billing';
import { chargeSessionsApi } from '../../api/services/chargeSessions';
import UnpaidBillingsList from "./UnpaidBillingList";
import './BillingDashboard.css';

function BillingDashboard() {
    const { data: pricingGroups = [] } = useQuery({
        queryKey: ['pricingGroups'],
        queryFn: billingApi.getPricingGroups,
    });

    const { data: unpaidBillings = [] } = useQuery({
        queryKey: ['unpaidBillings'],
        queryFn: billingApi.getUnpaidBillings,
    });

    const { data: sessionStats } = useQuery({
        queryKey: ['sessionStats'],
        queryFn: chargeSessionsApi.getStats,
    });

    const totalUnpaidAmount = unpaidBillings.reduce((sum, billing) => sum + billing.totalAmount, 0);
    const currency = unpaidBillings[0]?.currency || 'EUR';

    return (
        <div className="billing-dashboard">
            <div className="dashboard-header">
                <h1>Billing Dashboard</h1>
                <div className="header-actions">
                    <Link to="/billing/pricing-groups" className="btn">
                        Manage Pricing
                    </Link>
                </div>
            </div>

            <div className="billing-stats">
                <div className="stat-card">
                    <h3>Pricing Groups</h3>
                    <div className="stat-number">{pricingGroups.length}</div>
                    <div className="stat-info">Active groups</div>
                    <Link to="/billing/pricing-groups" className="stat-link">Manage →</Link>
                </div>

                <div className="stat-card">
                    <h3>Unpaid Sessions</h3>
                    <div className="stat-number">{unpaidBillings.length}</div>
                    <div className="stat-info">{totalUnpaidAmount.toFixed(2)} {currency} pending</div>
                </div>

                <div className="stat-card">
                    <h3>Total Sessions</h3>
                    <div className="stat-number">{sessionStats?.totalSessions || 0}</div>
                    <div className="stat-info">{sessionStats?.totalEnergyDelivered?.toFixed(1) || 0} kWh delivered</div>
                </div>
            </div>

            <div className="billing-sections">
                <UnpaidBillingsList />

                <div className="quick-actions">
                    <h2>Quick Actions</h2>
                    <div className="action-grid">
                        <Link to="/billing/pricing-groups" className="action-card">
                            <h4>Pricing Groups</h4>
                            <p>Configure pricing for different locations</p>
                        </Link>
                        <Link to="/charge-sessions" className="action-card">
                            <h4>All Sessions</h4>
                            <p>View complete session history</p>
                        </Link>
                        <Link to="/charge-points" className="action-card">
                            <h4>ChargePoints</h4>
                            <p>Manage charging infrastructure</p>
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default BillingDashboard;