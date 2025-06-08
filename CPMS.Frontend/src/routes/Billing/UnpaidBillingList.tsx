import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { billingApi } from '../../api/services/billing';
import './UnpaidBillingList.css';

function UnpaidBillingsList() {
    const { data: unpaidBillings = [], isLoading } = useQuery({
        queryKey: ['unpaidBillings'],
        queryFn: billingApi.getUnpaidBillings,
        refetchInterval: 30000
    });

    const getStatusClass = (status: string) => {
        switch (status) {
            case 'succeeded': return 'succeeded';
            case 'pending': return 'pending';
            case 'failed': return 'failed';
            default: return '';
        }
    };

    if (isLoading) return <div className="loading">Loading unpaid billings...</div>;

    return (
        <div className="unpaid-billings">
            <h2>Unpaid Sessions ({unpaidBillings.length})</h2>

            {unpaidBillings.length === 0 ? (
                <div className="empty-state">
                    <p>No unpaid sessions found.</p>
                </div>
            ) : (
                <div className="billing-table">
                    <table>
                        <thead>
                        <tr>
                            <th>Session</th>
                            <th>Amount</th>
                            <th>Status</th>
                            <th>Created</th>
                        </tr>
                        </thead>
                        <tbody>
                        {unpaidBillings.map(billing => (
                            <tr key={billing.id}>
                                <td>
                                    <Link to={`/charge-sessions/${billing.sessionId}`}>
                                        Session {billing.sessionId.slice(0, 8)}...
                                    </Link>
                                </td>
                                <td>
                                    <strong>{billing.totalAmount} {billing.currency}</strong>
                                    <div className="amount-breakdown">
                                        Base: {billing.baseAmount} + Energy: {billing.energyAmount}
                                    </div>
                                </td>
                                <td>
                                    <span className={`status-badge status ${getStatusClass(billing.paymentStatus)}`}>
                                        {billing.paymentStatus}
                                    </span>
                                </td>
                                <td>{new Date(billing.createdAt).toLocaleDateString()}</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}

export default UnpaidBillingsList;