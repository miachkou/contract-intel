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
  id: number;
  title: string;
  vendor: string;
  startDate: string;
  endDate: string;
  renewalDate?: string;
  autoRenewEnabled: boolean;
  riskScore: number;
  createdAt: string;
}

export interface Document {
  id: number;
  contractId: number;
  fileName: string;
  filePath: string;
  uploadedAt: string;
}

export interface Clause {
  id: number;
  documentId: number;
  type: string;
  content: string;
  confidence: number;
  startPage?: number;
  endPage?: number;
}

export interface CreateContractRequest {
  title: string;
  vendor: string;
  startDate: string;
  endDate: string;
  renewalDate?: string;
  autoRenewEnabled: boolean;
}

// API helpers
export const contractsApi = {
  getAll: (params?: { vendor?: string; beforeRenewal?: string }) =>
    api.get<Contract[]>('/contracts', { params }),
  
  getById: (id: number) =>
    api.get<Contract>(`/contracts/${id}`),
  
  create: (data: CreateContractRequest) =>
    api.post<Contract>('/contracts', data),
  
  uploadDocument: (contractId: number, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post<Document>(`/contracts/${contractId}/documents`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  
  extractContract: (contractId: number) =>
    api.post(`/contracts/${contractId}/extract`),
  
  getClauses: (contractId: number) =>
    api.get<Clause[]>(`/contracts/${contractId}/clauses`),
};
