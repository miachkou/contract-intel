import { Link, Outlet } from 'react-router-dom';
import './Layout.css';

export function Layout() {
    return (
        <div className="layout">
            <header className="header">
                <div className="header-content">
                    <h1>Contract Intelligence</h1>
                    <nav className="nav">
                        <Link to="/contracts">Contracts</Link>
                        <Link to="/renewals">Renewals</Link>
                    </nav>
                </div>
            </header>
            <main className="main">
                <Outlet />
            </main>
        </div>
    );
}
