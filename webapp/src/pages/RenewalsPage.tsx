import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { contractsApi, type Contract } from '../lib/api';
import './RenewalsPage.css';

export function RenewalsPage() {
    const [contracts, setContracts] = useState<Contract[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadUpcomingRenewals();
    }, []);

    const loadUpcomingRenewals = async () => {
        try {
            setLoading(true);
            setError(null);

            // Get contracts with renewals in the next 90 days
            const ninetyDaysFromNow = new Date();
            ninetyDaysFromNow.setDate(ninetyDaysFromNow.getDate() + 90);

            const response = await contractsApi.getAll({
                beforeRenewal: ninetyDaysFromNow.toISOString().split('T')[0],
            });

            // Filter and sort by renewal date
            const withRenewals = response.data
                .filter(c => c.renewalDate)
                .sort((a, b) => new Date(a.renewalDate!).getTime() - new Date(b.renewalDate!).getTime());

            setContracts(withRenewals);
        } catch (err) {
            setError('Failed to load renewals');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div className="message">Loading renewals...</div>;
    if (error) return <div className="message error">{error}</div>;

    return (
        <div className="renewals-page">
            <div className="page-header">
                <h2>Upcoming Renewals</h2>
                <span className="subtitle">Next 90 days</span>
            </div>

            {contracts.length === 0 ? (
                <div className="message">No upcoming renewals in the next 90 days.</div>
            ) : (
                <div className="renewals-list">
                    {contracts.map((contract) => (
                        <Link key={contract.id} to={`/contracts/${contract.id}`} className="renewal-card">
                            <div className="renewal-card-header">
                                <div>
                                    <h3>{contract.title}</h3>
                                    <p className="vendor">{contract.vendor}</p>
                                </div>
                                <span className={`risk-badge risk-${getRiskLevel(contract.riskScore)}`}>
                                    Risk: {contract.riskScore}
                                </span>
                            </div>

                            <div className="renewal-card-body">
                                <div className="renewal-info">
                                    <div className="renewal-date">
                                        <span className="label">Renewal Date</span>
                                        <span className="date">{new Date(contract.renewalDate!).toLocaleDateString()}</span>
                                        <span className="days-until">{getDaysUntil(contract.renewalDate!)} days</span>
                                    </div>
                                    <div className="renewal-meta">
                                        {contract.status && (
                                            <span className="status-badge">{contract.status}</span>
                                        )}
                                        <span className="end-date">Ends: {new Date(contract.endDate).toLocaleDateString()}</span>
                                    </div>
                                </div>
                            </div>
                        </Link>
                    ))}
                </div>
            )}
        </div>
    );
}

function getRiskLevel(score: number): string {
    if (score >= 50) return 'high';
    if (score >= 25) return 'medium';
    return 'low';
}

function getDaysUntil(dateStr: string): number {
    const target = new Date(dateStr);
    const today = new Date();
    const diffTime = target.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
}
