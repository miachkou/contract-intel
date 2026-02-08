import axios from 'axios';

const API_BASE_URL = 'http://localhost:5058/api';

export const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Basic response interceptor for error handling
api.interceptors.response.use(
    (response) => response,
    (error) => {
        console.error('API Error:', error.response?.data || error.message);
        return Promise.reject(error);
    }
);

// Contract types
export interface Contract {
    id: string;
    title: string;
    vendor: string;
    startDate: string;
    endDate: string;
    renewalDate?: string;
    riskScore: number;
    status?: string;
    createdAt: string;
    updatedAt: string;
}

export interface Document {
    id: string;
    fileName: string;
    fileSize: number;
    mimeType?: string;
    uploadedAt: string;
}

export interface Clause {
    id: string;
    clauseType: string;
    excerpt: string;
    confidence: number;
    pageNumber?: number;
    extractedAt: string;
}

export interface CreateContractRequest {
    title: string;
    vendor: string;
    startDate: string;
    endDate: string;
    renewalDate?: string;
    status?: string;
}

// API helpers
export const contractsApi = {
    getAll: (params?: { vendor?: string; renewalBefore?: string; minRisk?: number }) =>
        api.get<Contract[]>('/contracts', { params }),

    getById: (id: string) =>
        api.get<Contract>(`/contracts/${id}`),

    create: (data: CreateContractRequest) =>
        api.post<Contract>('/contracts', data),

    uploadDocument: (contractId: string, file: File) => {
        const formData = new FormData();
        formData.append('file', file);
        return api.post<Document>(`/contracts/${contractId}/documents`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    },

    extractContract: (contractId: string) =>
        api.post(`/contracts/${contractId}/extract`),

    getClauses: (contractId: string) =>
        api.get<Clause[]>(`/contracts/${contractId}/clauses`),
};
