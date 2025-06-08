import { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { billingApi } from '../../api/services/billing';
import './SimpleSessionPayment.css';

interface SessionPaymentProps {
    sessionId: string;
}

function SimpleSessionPayment({ sessionId }: SessionPaymentProps) {
    const [customerEmail, setCustomerEmail] = useState('');
    const [error, setError] = useState('');

    const { data: billing, isLoading } = useQuery({
        queryKey: ['sessionBilling', sessionId],
        queryFn: () => billingApi.getSessionBilling(sessionId),
        retry: false
    });

    const createPayment = useMutation({
        mutationFn: (email: string) => billingApi.createCheckoutSession(sessionId, email),
        onSuccess: (response) => {
            window.location.href = response.url;
        },
        onError: () => {
            setError('Payment failed. Please try again.');
        }
    });

    const handlePayment = (e: React.FormEvent) => {
        e.preventDefault();

        if (!customerEmail) {
            setError('Please enter your email address');
            return;
        }

        setError('');
        createPayment.mutate(customerEmail);
    };

    if (isLoading) return <div className="loading">Loading billing information...</div>;

    if (!billing) {
        return (
            <div className="payment-container">
                <div className="info-message">
                    <h3>No billing information available</h3>
                    <p>Billing will be calculated when the session ends.</p>
                </div>
            </div>
        );
    }

    if (billing.paymentStatus === 'succeeded') {
        return (
            <div className="payment-container">
                <div className="payment-success">
                    <h3>✅ Payment Completed</h3>
                    <div className="payment-details">
                        <div>Amount Paid: <strong>{billing.totalAmount} {billing.currency}</strong></div>
                        <div>Paid At: <strong>{new Date(billing.paidAt!).toLocaleString()}</strong></div>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="payment-container">
            <div className="billing-summary">
                <h3>Session Billing</h3>
                <div className="cost-breakdown">
                    <div className="cost-item">
                        <span>Session Fee:</span>
                        <span>{billing.baseAmount} {billing.currency}</span>
                    </div>
                    <div className="cost-item">
                        <span>Energy Cost:</span>
                        <span>{billing.energyAmount} {billing.currency}</span>
                    </div>
                    <div className="cost-total">
                        <span>Total:</span>
                        <span><strong>{billing.totalAmount} {billing.currency}</strong></span>
                    </div>
                </div>
            </div>

            <div className="payment-form">
                <h3>Complete Payment</h3>
                {error && <div className="error-msg">{error}</div>}
                <form onSubmit={handlePayment}>
                    <div className="form-group">
                        <label htmlFor="email">Email Address</label>
                        <input
                            type="email"
                            id="email"
                            value={customerEmail}
                            onChange={(e) => setCustomerEmail(e.target.value)}
                            placeholder="your.email@example.com"
                            required
                        />
                        <small>Receipt will be sent to this email</small>
                    </div>

                    <button
                        type="submit"
                        className="btn payment-btn"
                        disabled={createPayment.isPending}
                    >
                        {createPayment.isPending ? 'Redirecting to payment...' : `Pay ${billing.totalAmount} ${billing.currency}`}
                    </button>
                </form>
            </div>

            <div className="payment-info">
                <p>🔒 You will be redirected to Stripe's secure payment page</p>
            </div>
        </div>
    );
}

export default SimpleSessionPayment;