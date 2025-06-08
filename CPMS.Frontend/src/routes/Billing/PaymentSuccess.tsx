import { useEffect, useState } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import { useQuery } from "@tanstack/react-query";
import { billingApi } from "../../api/services/billing";
import './PaymentSuccess.css';

function PaymentSuccess() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const [countdown, setCountdown] = useState(5);

    const stripeSessionId = searchParams.get('session_id');
    const billingSessionId = searchParams.get('billing_session');

    const { data: billing } = useQuery({
        queryKey: ['sessionBilling', billingSessionId],
        queryFn: () => billingApi.getSessionBilling(billingSessionId!),
        enabled: !!billingSessionId,
        refetchInterval: 2000,
        refetchIntervalInBackground: false
    });

    useEffect(() => {
        const timer = setInterval(() => {
            setCountdown(prev => {
                if (prev <= 1) {
                    clearInterval(timer);
                    navigate('/billing');
                    return 0;
                }
                return prev - 1;
            });
        }, 1000);

        return () => clearInterval(timer);
    }, [navigate]);

    const isPaymentConfirmed = billing?.paymentStatus === 'succeeded';

    return (
        <div className="payment-success-page">
            <div className="success-container">
                <div className="success-content">
                    {isPaymentConfirmed ? (
                        <>
                            <div className="success-icon">✅</div>
                            <h1>Payment Successful!</h1>
                            <p>Thank you for your payment. Your charging session has been completed and paid.</p>
                        </>
                    ) : (
                        <>
                            <div className="processing-icon">⏳</div>
                            <h1>Payment Processing...</h1>
                            <p>We're confirming your payment. This should only take a few seconds.</p>
                        </>
                    )}

                    {billing && (
                        <div className="payment-details">
                            <h3>Payment Details</h3>
                            <div className="detail-grid">
                                <div className="detail-item">
                                    <span>Amount Paid:</span>
                                    <strong>{billing.totalAmount} {billing.currency}</strong>
                                </div>
                                <div className="detail-item">
                                    <span>Session ID:</span>
                                    <code>{billing.sessionId.slice(0, 8)}...</code>
                                </div>
                                <div className="detail-item">
                                    <span>Status:</span>
                                    <span className={`status ${billing.paymentStatus}`}>
                                        {billing.paymentStatus}
                                    </span>
                                </div>
                                {billing.paidAt && (
                                    <div className="detail-item">
                                        <span>Paid At:</span>
                                        <span>{new Date(billing.paidAt).toLocaleString()}</span>
                                    </div>
                                )}
                            </div>
                        </div>
                    )}

                    {stripeSessionId && (
                        <div className="stripe-info">
                            <p className="stripe-session">
                                Stripe Session: <code>{stripeSessionId.slice(0, 20)}...</code>
                            </p>
                        </div>
                    )}

                    <div className="actions">
                        <Link to={`/charge-sessions/${billingSessionId}`} className="btn btn-gray">
                            View Session Details
                        </Link>
                        <Link to="/billing" className="btn">
                            Go to Billing Dashboard
                        </Link>
                    </div>

                    <div className="auto-redirect">
                        <p>You will be automatically redirected to billing in <strong>{countdown}</strong> seconds</p>
                        <button
                            className="btn-link"
                            onClick={() => navigate('/billing')}
                        >
                            Go now
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default PaymentSuccess;