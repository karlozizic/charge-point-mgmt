import { Outlet } from 'react-router-dom';
import './Layout.css';
import Header from "../common/Header.tsx";

function Layout() {
    return (
        <div className="wrapper">
            <Header />
            <main className="content">
                <Outlet />
            </main>
            <footer className="footer">
                <p>2025 - CPMS</p>
            </footer>
        </div>
    );
}

export default Layout;