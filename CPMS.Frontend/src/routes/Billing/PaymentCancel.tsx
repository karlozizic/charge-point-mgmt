import { Link } from 'react-router-dom';

function PaymentCancel() {
    return (
        <div className="payment-cancel-page">
            <div className="cancel-container">
                <div className="cancel-icon">❌</div>
                <h1>Payment Cancelled</h1>
                <p>Your payment was cancelled. No charges were made to your account.</p>

                <div className="actions">
                    <Link to="/billing" className="btn btn-gray">
                        Back to Billing
                    </Link>
                    <Link to="/charge-sessions" className="btn">
                        View Sessions
                    </Link>
                </div>

                <div className="help-text">
                    <p>If you experienced any issues, please contact our support.</p>
                </div>
            </div>
        </div>
    );
}

export default PaymentCancel;