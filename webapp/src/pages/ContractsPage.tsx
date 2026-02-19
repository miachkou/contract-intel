import { useState } from 'react';
import { Link, useSearchParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { contractsApi, type CreateContractRequest } from '../lib/api';
import './ContractsPage.css';

export function ContractsPage() {
    const [searchParams, setSearchParams] = useSearchParams();
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    // Read active filters from URL (these are the applied filters)
    const activeRenewalDays = searchParams.get('renewalDays') || '';
    const activeMinRisk = searchParams.get('minRisk') || '';

    // Local state for filter inputs (what user is typing)
    const [renewalDays, setRenewalDays] = useState(activeRenewalDays);
    const [minRisk, setMinRisk] = useState(activeMinRisk);

    // Modal and form state
    const [showModal, setShowModal] = useState(false);
    const [formData, setFormData] = useState<CreateContractRequest>({
        title: '',
        vendor: '',
        startDate: '',
        endDate: '',
        renewalDate: '',
        status: 'Active',
    });

    // Calculate query parameters from active filters only
    const renewalBefore = activeRenewalDays
        ? new Date(Date.now() + parseInt(activeRenewalDays) * 24 * 60 * 60 * 1000).toISOString()
        : undefined;
    const minRiskScore = activeMinRisk ? parseFloat(activeMinRisk) : undefined;

    // Use React Query for data fetching - queryKey only changes when active filters change
    const { data: contracts = [], isLoading, error } = useQuery({
        queryKey: ['contracts', activeRenewalDays, activeMinRisk],
        queryFn: async () => {
            const response = await contractsApi.getAll({
                renewalBefore: renewalBefore,
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

    // Create contract mutation
    const createMutation = useMutation({
        mutationFn: (data: CreateContractRequest) => contractsApi.create(data),
        onSuccess: (response) => {
            queryClient.invalidateQueries({ queryKey: ['contracts'] });
            setShowModal(false);
            setFormData({
                title: '',
                vendor: '',
                startDate: '',
                endDate: '',
                renewalDate: '',
                status: 'Active',
            });
            navigate(`/contracts/${response.data.id}`);
        },
        onError: (error) => {
            console.error('Failed to create contract:', error);
            alert('Failed to create contract. Please try again.');
        },
    });

    const handleOpenModal = () => {
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setFormData({
            title: '',
            vendor: '',
            startDate: '',
            endDate: '',
            renewalDate: '',
            status: 'Active',
        });
    };

    const handleFormChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData((prev) => ({ ...prev, [name]: value }));
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!formData.title || !formData.vendor || !formData.startDate || !formData.endDate) {
            alert('Please fill in all required fields');
            return;
        }
        createMutation.mutate(formData);
    };

    if (isLoading) return <div className="message">Loading contracts...</div>;
    if (error) return <div className="message error">Failed to load contracts</div>;

    return (
        <div className="contracts-page">
            <div className="page-header">
                <h2>All Contracts</h2>
                <button className="btn btn-primary" onClick={handleOpenModal}>
                    Create Contract
                </button>
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
                                        {contract.renewalDate ? new Date(contract.renewalDate).toLocaleDateString() : 'N/A'}
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

            {/* Create Contract Modal */}
            {showModal && (
                <div className="modal-overlay" onClick={handleCloseModal}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h3>Create New Contract</h3>
                            <button className="modal-close" onClick={handleCloseModal}>
                                ×
                            </button>
                        </div>
                        <form onSubmit={handleSubmit} className="contract-form">
                            <div className="form-group">
                                <label htmlFor="title">Title *</label>
                                <input
                                    id="title"
                                    name="title"
                                    type="text"
                                    required
                                    value={formData.title}
                                    onChange={handleFormChange}
                                    placeholder="e.g., Software License Agreement"
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="vendor">Vendor *</label>
                                <input
                                    id="vendor"
                                    name="vendor"
                                    type="text"
                                    required
                                    value={formData.vendor}
                                    onChange={handleFormChange}
                                    placeholder="e.g., Acme Corp"
                                />
                            </div>
                            <div className="form-row">
                                <div className="form-group">
                                    <label htmlFor="startDate">Start Date *</label>
                                    <input
                                        id="startDate"
                                        name="startDate"
                                        type="date"
                                        required
                                        value={formData.startDate}
                                        onChange={handleFormChange}
                                    />
                                </div>
                                <div className="form-group">
                                    <label htmlFor="endDate">End Date *</label>
                                    <input
                                        id="endDate"
                                        name="endDate"
                                        type="date"
                                        required
                                        value={formData.endDate}
                                        onChange={handleFormChange}
                                    />
                                </div>
                            </div>
                            <div className="form-row">
                                <div className="form-group">
                                    <label htmlFor="renewalDate">Renewal Date</label>
                                    <input
                                        id="renewalDate"
                                        name="renewalDate"
                                        type="date"
                                        value={formData.renewalDate}
                                        onChange={handleFormChange}
                                    />
                                </div>
                                <div className="form-group">
                                    <label htmlFor="status">Status</label>
                                    <input
                                        id="status"
                                        name="status"
                                        type="text"
                                        value={formData.status}
                                        onChange={handleFormChange}
                                        placeholder="Active"
                                    />
                                </div>
                            </div>
                            <div className="modal-actions">
                                <button
                                    type="button"
                                    className="btn btn-secondary"
                                    onClick={handleCloseModal}
                                    disabled={createMutation.isPending}
                                >
                                    Cancel
                                </button>
                                <button
                                    type="submit"
                                    className="btn btn-primary"
                                    disabled={createMutation.isPending}
                                >
                                    {createMutation.isPending ? 'Creating...' : 'Create Contract'}
                                </button>
                            </div>
                        </form>
                    </div>
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
