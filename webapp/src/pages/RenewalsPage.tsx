import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { contractsApi, type Contract } from '../lib/api';
import './RenewalsPage.css';

export function RenewalsPage() {
    const [searchParams, setSearchParams] = useSearchParams();

    // Read active filters from URL
    const activeWindowDays = searchParams.get('days') || '90';
    const activeMinRisk = searchParams.get('minRisk') || '';

    // Local state for filter inputs
    const [windowDays, setWindowDays] = useState(activeWindowDays);
    const [minRisk, setMinRisk] = useState(activeMinRisk);

    // Calculate renewal date from window
    const renewalBefore = activeWindowDays
        ? new Date(Date.now() + parseInt(activeWindowDays) * 24 * 60 * 60 * 1000).toISOString()
        : undefined;
    const minRiskScore = activeMinRisk ? parseFloat(activeMinRisk) : undefined;

    // Fetch contracts with React Query
    const { data: contracts = [], isLoading, error } = useQuery({
        queryKey: ['renewals', activeWindowDays, activeMinRisk],
        queryFn: async () => {
            const response = await contractsApi.getAll({
                renewalBefore: renewalBefore,
                minRisk: minRiskScore,
            });

            // Filter and sort by renewal date
            return response.data
                .filter(c => c.renewalDate)
                .sort((a, b) => new Date(a.renewalDate!).getTime() - new Date(b.renewalDate!).getTime());
        },
    });

    const handleApplyFilters = () => {
        const params = new URLSearchParams();
        if (windowDays) params.set('days', windowDays);
        if (minRisk) params.set('minRisk', minRisk);
        setSearchParams(params);
    };

    const handleClearFilters = () => {
        setWindowDays('90');
        setMinRisk('');
        setSearchParams(new URLSearchParams({ days: '90' }));
    };

    if (isLoading) return <div className="message">Loading renewals...</div>;
    if (error) return <div className="message error">Failed to load renewals</div>;

    return (
        <div className="renewals-page">
            <div className="page-header">
                <h2>Upcoming Renewals</h2>
            </div>

            <div className="filters-section">
                <div className="filter-group">
                    <label htmlFor="windowDays">Renewal Window (days)</label>
                    <input
                        id="windowDays"
                        type="number"
                        min="1"
                        placeholder="e.g., 90"
                        value={windowDays}
                        onChange={(e) => setWindowDays(e.target.value)}
                        onKeyDown={(e) => e.key === 'Enter' && handleApplyFilters()}
                    />
                </div>
                <div className="filter-group">
                    <label htmlFor="minRisk">Minimum Risk Score</label>
                    <input
                        id="minRisk"
                        type="number"
                        min="0"
                        max="100"
                        placeholder="e.g., 25"
                        value={minRisk}
                        onChange={(e) => setMinRisk(e.target.value)}
                        onKeyDown={(e) => e.key === 'Enter' && handleApplyFilters()}
                    />
                </div>
                <div className="filter-actions">
                    <button className="btn btn-primary" onClick={handleApplyFilters}>
                        Apply Filters
                    </button>
                    <button className="btn btn-secondary" onClick={handleClearFilters}>
                        Clear
                    </button>
                </div>
            </div>

            {contracts.length === 0 ? (
                <div className="message">
                    No renewals found within {activeWindowDays} days{activeMinRisk ? ` with risk score ≥ ${activeMinRisk}` : ''}.
                </div>
            ) : (
                <div className="renewals-list">
                    {contracts.map((contract) => (
                        <Link key={contract.id} to={`/contracts/${contract.id}`} className="renewal-card">
                            <div className="renewal-card-header">
                                <div>
                                    <h3>{contract.title}</h3>
                                    <p className="vendor">{contract.vendor}</p>
                                </div>
                                <span className={`risk-badge risk-${getRiskLevel(contract.riskScore ?? 0)}`}>
                                    Risk: {contract.riskScore?.toFixed(1) ?? 'N/A'}
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
