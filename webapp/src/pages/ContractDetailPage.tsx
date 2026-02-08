import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { contractsApi, type Contract, type Clause } from '../lib/api';
import './ContractDetailPage.css';

export function ContractDetailPage() {
    const { id } = useParams<{ id: string }>();
    const [contract, setContract] = useState<Contract | null>(null);
    const [clauses, setClauses] = useState<Clause[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (id) {
            loadContractDetails(id);
        }
    }, [id]);

    const loadContractDetails = async (contractId: string) => {
        try {
            setLoading(true);
            setError(null);

            const [contractRes, clausesRes] = await Promise.all([
                contractsApi.getById(contractId),
                contractsApi.getClauses(contractId),
            ]);

            setContract(contractRes.data);
            setClauses(clausesRes.data);
        } catch (err) {
            setError('Failed to load contract details');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div className="message">Loading contract details...</div>;
    if (error) return <div className="message error">{error}</div>;
    if (!contract) return <div className="message">Contract not found</div>;

    return (
        <div className="contract-detail-page">
            <div className="breadcrumb">
                <Link to="/contracts">← Back to Contracts</Link>
            </div>

            <div className="detail-header">
                <h2>{contract.title}</h2>
                <span className={`risk-badge risk-${getRiskLevel(contract.riskScore)}`}>
                    Risk Score: {contract.riskScore}
                </span>
            </div>

            <div className="detail-grid">
                <div className="detail-section">
                    <h3>Contract Information</h3>
                    <div className="detail-fields">
                        <div className="detail-field">
                            <span className="label">Vendor</span>
                            <span>{contract.vendor}</span>
                        </div>
                        <div className="detail-field">
                            <span className="label">Start Date</span>
                            <span>{new Date(contract.startDate).toLocaleDateString()}</span>
                        </div>
                        <div className="detail-field">
                            <span className="label">End Date</span>
                            <span>{new Date(contract.endDate).toLocaleDateString()}</span>
                        </div>
                        {contract.renewalDate && (
                            <div className="detail-field">
                                <span className="label">Renewal Date</span>
                                <span>{new Date(contract.renewalDate).toLocaleDateString()}</span>
                            </div>
                        )}
                        {contract.status && (
                            <div className="detail-field">
                                <span className="label">Status</span>
                                <span>{contract.status}</span>
                            </div>
                        )}
                    </div>
                </div>

                <div className="detail-section">
                    <h3>Detected Clauses ({clauses.length})</h3>
                    {clauses.length === 0 ? (
                        <div className="empty-state">No clauses detected yet. Upload and extract a document.</div>
                    ) : (
                        <div className="clauses-list">
                            {clauses.map((clause) => (
                                <div key={clause.id} className="clause-item">
                                    <div className="clause-header">
                                        <span className="clause-type">{formatClauseType(clause.clauseType)}</span>
                                        <span className="clause-confidence">{clause.confidence ? (clause.confidence * 100).toFixed(0) : 0}%</span>
                                    </div>
                                    <p className="clause-content">{clause.excerpt}</p>
                                    {clause.pageNumber && (
                                        <span className="clause-page">Page {clause.pageNumber}</span>
                                    )}
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            <div className="actions">
                <button className="btn btn-secondary">Upload Document</button>
                <button className="btn btn-primary">Run Extraction</button>
            </div>
        </div>
    );
}

function getRiskLevel(score: number): string {
    if (score >= 50) return 'high';
    if (score >= 25) return 'medium';
    return 'low';
}

function formatClauseType(type: string): string {
    return type
        .split('_')
        .map(word => word.charAt(0).toUpperCase() + word.slice(1))
        .join(' ');
}
