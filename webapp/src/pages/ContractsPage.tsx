import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { contractsApi, type Contract } from '../lib/api';
import './ContractsPage.css';

export function ContractsPage() {
    const [searchParams, setSearchParams] = useSearchParams();

    // Read filters from URL
    const [renewalDays, setRenewalDays] = useState(searchParams.get('renewalDays') || '');
    const [minRisk, setMinRisk] = useState(searchParams.get('minRisk') || '');

    // Calculate renewal date from days
    const renewalBefore = renewalDays
        ? new Date(Date.now() + parseInt(renewalDays) * 24 * 60 * 60 * 1000).toISOString()
        : undefined;
    const minRiskScore = minRisk ? parseFloat(minRisk) : undefined;

    // Use React Query for data fetching
    const { data: contracts = [], isLoading, error } = useQuery({
        queryKey: ['contracts', renewalBefore, minRiskScore],
        queryFn: async () => {
            const response = await contractsApi.getAll({
                beforeRenewal: renewalBefore,
                minRisk: minRiskScore,
            });
            return response.data;
        },
    });

    const handleApplyFilters = () => {
        const params = new URLSearchParams();
        if (renewalDays) params.set('renewalDays', renewalDays);
        if (minRisk) params.set('minRisk', minRisk);
        setSearchParams(params);
    };

    const handleClearFilters = () => {
        setRenewalDays('');
        setMinRisk('');
        setSearchParams(new URLSearchParams());
    };

    if (isLoading) return <div className="message">Loading contracts...</div>;
    if (error) return <div className="message error">Failed to load contracts</div>;

    return (
        <div className="contracts-page">
            <div className="page-header">
                <h2>All Contracts</h2>
            </div>

            <div className="filters-section">
                <div className="filter-group">
                    <label htmlFor="renewalDays">Renewal Within (days)</label>
                    <input
                        id="renewalDays"
                        type="number"
                        min="0"
                        placeholder="e.g., 90"
                        value={renewalDays}
                        onChange={(e) => setRenewalDays(e.target.value)}
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
                <div className="message">No contracts found matching your filters.</div>
            ) : (
                <div className="contracts-table-wrapper">
                    <table className="contracts-table">
                        <thead>
                            <tr>
                                <th>Vendor</th>
                                <th>Title</th>
                                <th>Renewal Date</th>
                                <th>Risk Score</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {contracts.map((contract) => (
                                <tr key={contract.id}>
                                    <td className="vendor-cell">{contract.vendor}</td>
                                    <td className="title-cell">{contract.title}</td>
                                    <td className="date-cell">
                                        {new Date(contract.renewalDate).toLocaleDateString()}
                                    </td>
                                    <td className="risk-cell">
                                        <span className={`risk-badge risk-${getRiskLevel(contract.riskScore ?? 0)}`}>
                                            {contract.riskScore?.toFixed(1) ?? 'N/A'}
                                        </span>
                                    </td>
                                    <td className="status-cell">
                                        <span className="status-badge">{contract.status || 'Active'}</span>
                                    </td>
                                    <td className="actions-cell">
                                        <Link to={`/contracts/${contract.id}`} className="btn btn-link">
                                            View Details →
                                        </Link>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
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
