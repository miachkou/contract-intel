import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { contractsApi, type UpdateClauseRequest } from '../lib/api';
import './ContractDetailPage.css';

export function ContractDetailPage() {
    const { id } = useParams<{ id: string }>();
    const queryClient = useQueryClient();
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [uploadSuccess, setUploadSuccess] = useState(false);
    const [extractSuccess, setExtractSuccess] = useState(false);
    const [editingClauseId, setEditingClauseId] = useState<string | null>(null);
    const [editForm, setEditForm] = useState<{ clauseType: string; excerpt: string }>({ clauseType: '', excerpt: '' });

    // Fetch contract details
    const { data: contract, isLoading: loadingContract, error: contractError } = useQuery({
        queryKey: ['contract', id],
        queryFn: async () => {
            const response = await contractsApi.getById(id!);
            return response.data;
        },
        enabled: !!id,
    });

    // Fetch clauses
    const { data: clauses = [], isLoading: loadingClauses } = useQuery({
        queryKey: ['clauses', id],
        queryFn: async () => {
            const response = await contractsApi.getClauses(id!);
            return response.data;
        },
        enabled: !!id,
    });

    // Upload document mutation
    const uploadMutation = useMutation({
        mutationFn: (file: File) => contractsApi.uploadDocument(id!, file),
        onSuccess: () => {
            setUploadSuccess(true);
            setSelectedFile(null);
            setTimeout(() => setUploadSuccess(false), 3000);
        },
    });

    // Extract contract mutation
    const extractMutation = useMutation({
        mutationFn: () => contractsApi.extractContract(id!),
        onSuccess: () => {
            setExtractSuccess(true);
            // Refresh contract and clauses after extraction
            queryClient.invalidateQueries({ queryKey: ['contract', id] });
            queryClient.invalidateQueries({ queryKey: ['clauses', id] });
            setTimeout(() => setExtractSuccess(false), 3000);
        },
    });

    // Update clause mutation
    const updateClauseMutation = useMutation({
        mutationFn: ({ clauseId, data }: { clauseId: string; data: UpdateClauseRequest }) =>
            contractsApi.updateClause(id!, clauseId, data),
        onSuccess: () => {
            // Refresh contract and clauses after update
            queryClient.invalidateQueries({ queryKey: ['contract', id] });
            queryClient.invalidateQueries({ queryKey: ['clauses', id] });
            setEditingClauseId(null);
        },
    });

    const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file && file.type === 'application/pdf') {
            setSelectedFile(file);
            setUploadSuccess(false);
        } else if (file) {
            alert('Please select a PDF file');
        }
    };

    const handleUpload = () => {
        if (selectedFile) {
            uploadMutation.mutate(selectedFile);
        }
    };

    const handleExtract = () => {
        extractMutation.mutate();
    };

    const handleEditClause = (clause: any) => {
        setEditingClauseId(clause.id);
        setEditForm({ clauseType: clause.clauseType, excerpt: clause.excerpt });
    };

    const handleCancelEdit = () => {
        setEditingClauseId(null);
        setEditForm({ clauseType: '', excerpt: '' });
    };

    const handleSaveClause = (clauseId: string) => {
        updateClauseMutation.mutate({
            clauseId,
            data: {
                clauseType: editForm.clauseType,
                excerpt: editForm.excerpt,
            },
        });
    };

    const handleApproveClause = (clauseId: string) => {
        updateClauseMutation.mutate({
            clauseId,
            data: {
                approved: true,
                approvedBy: 'User', // In real app, get from auth context
            },
        });
    };

    if (loadingContract) return <div className="message">Loading contract details...</div>;
    if (contractError) return <div className="message error">Failed to load contract details</div>;
    if (!contract) return <div className="message">Contract not found</div>;

    return (
        <div className="contract-detail-page">
            <div className="breadcrumb">
                <Link to="/contracts">← Back to Contracts</Link>
            </div>

            <div className="detail-header">
                <h2>{contract.title}</h2>
                <span className={`risk-badge risk-${getRiskLevel(contract.riskScore ?? 0)}`}>
                    Risk Score: {contract.riskScore?.toFixed(1) ?? 'N/A'}
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
                                <span className="status-badge">{contract.status}</span>
                            </div>
                        )}
                    </div>
                </div>

                <div className="detail-section">
                    <h3>Upload Document</h3>
                    <div className="upload-section">
                        <input
                            type="file"
                            accept="application/pdf"
                            onChange={handleFileSelect}
                            id="file-input"
                            className="file-input"
                        />
                        <label htmlFor="file-input" className="file-label">
                            {selectedFile ? selectedFile.name : 'Choose PDF file...'}
                        </label>
                        <button
                            className="btn btn-primary"
                            onClick={handleUpload}
                            disabled={!selectedFile || uploadMutation.isPending}
                        >
                            {uploadMutation.isPending ? 'Uploading...' : 'Upload'}
                        </button>
                    </div>

                    {uploadSuccess && (
                        <div className="success-message">✓ Document uploaded successfully!</div>
                    )}
                    {uploadMutation.isError && (
                        <div className="error-message">
                            Failed to upload document. Please try again.
                        </div>
                    )}

                    <button
                        className="btn btn-secondary extract-btn"
                        onClick={handleExtract}
                        disabled={extractMutation.isPending}
                    >
                        {extractMutation.isPending ? 'Extracting...' : '🔍 Run Extraction'}
                    </button>

                    {extractSuccess && (
                        <div className="success-message">✓ Extraction completed successfully!</div>
                    )}
                    {extractMutation.isError && (
                        <div className="error-message">
                            Failed to run extraction. Please try again.
                        </div>
                    )}
                </div>
            </div>

            <div className="clauses-section">
                <h3>Detected Clauses ({clauses.length})</h3>
                {loadingClauses ? (
                    <div className="message">Loading clauses...</div>
                ) : clauses.length === 0 ? (
                    <div className="empty-state">
                        No clauses detected yet. Upload a document and run extraction to analyze the contract.
                    </div>
                ) : (
                    <div className="clauses-list">
                        {clauses.map((clause) => (
                            <div key={clause.id} className="clause-item">
                                {editingClauseId === clause.id ? (
                                    // Edit mode
                                    <div className="clause-edit-form">
                                        <div className="form-group">
                                            <label htmlFor={`type-${clause.id}`}>Clause Type</label>
                                            <input
                                                id={`type-${clause.id}`}
                                                type="text"
                                                value={editForm.clauseType}
                                                onChange={(e) => setEditForm({ ...editForm, clauseType: e.target.value })}
                                                className="form-input"
                                            />
                                        </div>
                                        <div className="form-group">
                                            <label htmlFor={`excerpt-${clause.id}`}>Excerpt</label>
                                            <textarea
                                                id={`excerpt-${clause.id}`}
                                                value={editForm.excerpt}
                                                onChange={(e) => setEditForm({ ...editForm, excerpt: e.target.value })}
                                                className="form-textarea"
                                                rows={4}
                                            />
                                        </div>
                                        <div className="form-actions">
                                            <button
                                                className="btn btn-primary btn-sm"
                                                onClick={() => handleSaveClause(clause.id)}
                                                disabled={updateClauseMutation.isPending}
                                            >
                                                {updateClauseMutation.isPending ? 'Saving...' : 'Save'}
                                            </button>
                                            <button
                                                className="btn btn-secondary btn-sm"
                                                onClick={handleCancelEdit}
                                                disabled={updateClauseMutation.isPending}
                                            >
                                                Cancel
                                            </button>
                                        </div>
                                    </div>
                                ) : (
                                    // View mode
                                    <>
                                        <div className="clause-header">
                                            <span className="clause-type">{formatClauseType(clause.clauseType)}</span>
                                            <div className="clause-meta">
                                                <span className="clause-confidence">
                                                    {clause.confidence ? (clause.confidence * 100).toFixed(0) : 0}% confidence
                                                </span>
                                                {clause.approvedAt && (
                                                    <span className="approved-badge">✓ Approved</span>
                                                )}
                                            </div>
                                        </div>
                                        <p className="clause-content">{clause.excerpt}</p>
                                        <div className="clause-footer">
                                            {clause.pageNumber && (
                                                <span className="clause-page">Page {clause.pageNumber}</span>
                                            )}
                                            <div className="clause-actions">
                                                <button
                                                    className="btn btn-link btn-sm"
                                                    onClick={() => handleEditClause(clause)}
                                                >
                                                    Edit
                                                </button>
                                                {!clause.approvedAt && (
                                                    <button
                                                        className="btn btn-link btn-sm"
                                                        onClick={() => handleApproveClause(clause.id)}
                                                        disabled={updateClauseMutation.isPending}
                                                    >
                                                        Approve
                                                    </button>
                                                )}
                                            </div>
                                        </div>
                                    </>
                                )}
                            </div>
                        ))}
                    </div>
                )}
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
