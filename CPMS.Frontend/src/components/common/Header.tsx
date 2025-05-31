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
                    <li><Link to="/locations">Locations</Link></li>
                    <li><Link to="/charge-points">ChargePoints</Link></li>
                    <li><Link to="/charge-tags">Tags</Link></li>
                    <li><Link to="/charge-sessions">Sessions</Link></li>
                </ul>
            </nav>
        </header>
    );
}

export default Header;