import { apiClient } from '@/lib/api-client';
import {
  LoginRequest,
  LoginResponse,
  User,
  Account,
  CreateAccountRequest,
  CreateAccountResponse,
  AccountBalance,
  TopUpRequest,
  TopUpResponse,
  CreateTransferRequest,
  CreateTransferResponse,
  Transfer,
  ListTransfersParams,
  ListTransfersResponse,
} from '@/types/api';

export const authService = {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>('/api/auth/login', credentials);
    apiClient.setToken(response.token);
    return response;
  },

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
  },

  getStoredUser(): User | null {
    if (typeof window === 'undefined') return null;
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  storeUser(user: User): void {
    if (typeof window === 'undefined') return;
    localStorage.setItem('user', JSON.stringify(user));
  },

  isAuthenticated(): boolean {
    if (typeof window === 'undefined') return false;
    return !!localStorage.getItem('token');
  },
};

export const accountService = {
  async getAccounts(): Promise<Account[]> {
    return apiClient.get<Account[]>('/api/v1/accounts');
  },

  async getAccount(iban: string): Promise<Account> {
    return apiClient.get<Account>(`/api/v1/accounts/${iban}`);
  },

  async getAccountBalance(iban: string): Promise<AccountBalance> {
    return apiClient.get<AccountBalance>(`/api/v1/accounts/${iban}/balance`);
  },

  async createAccount(request: CreateAccountRequest): Promise<CreateAccountResponse> {
    const idempotencyKey = `create-account-${Date.now()}-${Math.random()}`;
    return apiClient.post<CreateAccountResponse>('/api/v1/accounts', request, idempotencyKey);
  },

  async topUpAccount(iban: string, request: TopUpRequest): Promise<TopUpResponse> {
    const idempotencyKey = `topup-${iban}-${Date.now()}-${Math.random()}`;
    return apiClient.post<TopUpResponse>(`/api/v1/accounts/${iban}/topup`, request, idempotencyKey);
  },
};

export const transferService = {
  async createTransfer(request: CreateTransferRequest): Promise<CreateTransferResponse> {
    const idempotencyKey = `transfer-${Date.now()}-${Math.random()}`;
    return apiClient.post<CreateTransferResponse>('/api/v1/transfers', request, idempotencyKey);
  },

  async getTransfer(transferId: string): Promise<Transfer> {
    return apiClient.get<Transfer>(`/api/v1/transfers/${transferId}`);
  },

  async listTransfers(params?: ListTransfersParams): Promise<ListTransfersResponse> {
    return apiClient.get<ListTransfersResponse>('/api/v1/transfers', params);
  },
};
