import { Link } from 'react-router-dom';
import './Header.css';

function Header() {
    return (
        <header className="header">
            <div className="brand">
                <h1>Electrify</h1>
            </div>
            <nav className="nav">
                <ul>
                    <li><Link to="/">Home</Link></li>
                    <li><Link to="/charge-points">Charge Points</Link></li>
                    <li><Link to="/charge-tags">Charge Tags</Link></li>
                    <li><Link to="/charge-sessions">Charge Sessions</Link></li>
                </ul>
            </nav>
        </header>
    );
}

export default Header;