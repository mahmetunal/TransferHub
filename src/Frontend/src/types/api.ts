export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: string;
  email: string;
  expiresIn: number;
  tokenType: string;
}

export interface User {
  userId: string;
  email: string;
}

export interface Account {
  id: string;
  iban: string;
  balance: number;
  currency: string;
  ownerId: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateAccountRequest {
  initialBalance: number;
  currency: string;
}

export interface CreateAccountResponse {
  accountId: string;
  iban: string;
  balance: number;
  currency: string;
}

export interface AccountBalance {
  iban: string;
  balance: number;
  currency: string;
  lastUpdated: string;
}

export interface TopUpRequest {
  amount: number;
  currency: string;
}

export interface TopUpResponse {
  newBalance: number;
  currency: string;
}

export interface CreateTransferRequest {
  sourceAccount: string;
  destinationAccount: string;
  amount: number;
  currency: string;
  reference?: string;
}

export interface CreateTransferResponse {
  transferId: string;
  sourceAccount: string;
  destinationAccount: string;
  amount: number;
  currency: string;
  status: TransferStatus;
  reference?: string;
  createdAt: string;
}

export interface Transfer {
  id: string;
  sourceAccount: string;
  destinationAccount: string;
  amount: number;
  currency: string;
  status: TransferStatus;
  failureReason?: string;
  reference?: string;
  initiatedBy: string;
  createdAt: string;
  completedAt?: string;
}

export enum TransferStatus {
  Pending = 'Pending',
  Completed = 'Completed',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
}

export interface ListTransfersParams {
  pageNumber?: number;
  pageSize?: number;
  status?: TransferStatus;
  sourceAccount?: string;
  destinationAccount?: string;
  fromDate?: string;
  toDate?: string;
}

export interface ListTransfersResponse {
  transfers: Transfer[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
