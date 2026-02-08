import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { contractsApi, type Contract } from '../lib/api';
import './ContractsPage.css';

export function ContractsPage() {
  const [contracts, setContracts] = useState<Contract[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadContracts();
  }, []);

  const loadContracts = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await contractsApi.getAll();
      setContracts(response.data);
    } catch (err) {
      setError('Failed to load contracts');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="message">Loading contracts...</div>;
  if (error) return <div className="message error">{error}</div>;

  return (
    <div className="contracts-page">
      <div className="page-header">
        <h2>All Contracts</h2>
        <button className="btn btn-primary">+ New Contract</button>
      </div>
      
      {contracts.length === 0 ? (
        <div className="message">No contracts found. Create your first contract to get started.</div>
      ) : (
        <div className="contracts-grid">
          {contracts.map((contract) => (
            <Link key={contract.id} to={`/contracts/${contract.id}`} className="contract-card">
              <div className="contract-card-header">
                <h3>{contract.title}</h3>
                <span className={`risk-badge risk-${getRiskLevel(contract.riskScore)}`}>
                  Risk: {contract.riskScore}
                </span>
              </div>
              <div className="contract-card-body">
                <div className="contract-field">
                  <span className="label">Vendor:</span>
                  <span>{contract.vendor}</span>
                </div>
                <div className="contract-field">
                  <span className="label">End Date:</span>
                  <span>{new Date(contract.endDate).toLocaleDateString()}</span>
                </div>
                {contract.renewalDate && (
                  <div className="contract-field">
                    <span className="label">Renewal:</span>
                    <span>{new Date(contract.renewalDate).toLocaleDateString()}</span>
                  </div>
                )}
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
