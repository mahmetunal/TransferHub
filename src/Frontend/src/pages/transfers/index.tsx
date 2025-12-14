import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '@/contexts/AuthContext';
import { Layout } from '@/components/Layout';
import { Card } from '@/components/Card';
import { Button } from '@/components/Button';
import { transferService } from '@/services/api.service';
import { Transfer, TransferStatus } from '@/types/api';
import { Plus, RefreshCw, TrendingUp, Filter } from 'lucide-react';
import { format } from 'date-fns';
import Head from 'next/head';

export default function TransfersPage() {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();
  const [transfers, setTransfers] = useState<Transfer[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [statusFilter, setStatusFilter] = useState<TransferStatus | undefined>(undefined);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  React.useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push('/login');
    }
  }, [isAuthenticated, authLoading, router]);

  useEffect(() => {
    if (isAuthenticated) {
      loadTransfers();
    }
  }, [isAuthenticated, currentPage, statusFilter]);

  const loadTransfers = async () => {
    try {
      setIsLoading(true);
      setError('');
      const data = await transferService.listTransfers({
        pageNumber: currentPage,
        pageSize: 10,
        status: statusFilter,
      });
      setTransfers(data?.transfers || []);
      setTotalPages(data?.totalPages || 1);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load transfers');
      setTransfers([]);
    } finally {
      setIsLoading(false);
    }
  };

  const formatCurrency = (amount: number, currency: string) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency,
    }).format(amount);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Completed':
        return 'bg-green-100 text-green-800';
      case 'Pending':
        return 'bg-yellow-100 text-yellow-800';
      case 'Failed':
        return 'bg-red-100 text-red-800';
      case 'Cancelled':
        return 'bg-gray-100 text-gray-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  if (authLoading) {
    return <Layout><div>Loading...</div></Layout>;
  }

  return (
    <>
      <Head>
        <title>Transfers - TransferHub</title>
      </Head>

      <Layout>
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">Transfers</h1>
              <p className="mt-1 text-gray-600">Manage and track your money transfers</p>
            </div>
            <Button
              variant="primary"
              leftIcon={<Plus className="w-5 h-5" />}
              onClick={() => router.push('/transfers/new')}
            >
              New Transfer
            </Button>
          </div>

          {error && (
            <Card className="bg-red-50 border-red-200">
              <p className="text-red-800">{error}</p>
            </Card>
          )}

          <Card>
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-4">
                <Filter className="w-5 h-5 text-gray-600" />
                <span className="text-sm font-medium text-gray-700">Status:</span>
                <div className="flex space-x-2">
                  <button
                    onClick={() => setStatusFilter(undefined)}
                    className={`px-3 py-1 rounded-lg text-sm font-medium transition-colors ${
                      statusFilter === undefined
                        ? 'bg-primary-600 text-white'
                        : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                    }`}
                  >
                    All
                  </button>
                  {Object.values(TransferStatus).map((status) => (
                    <button
                      key={status}
                      onClick={() => setStatusFilter(status)}
                      className={`px-3 py-1 rounded-lg text-sm font-medium transition-colors ${
                        statusFilter === status
                          ? 'bg-primary-600 text-white'
                          : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                      }`}
                    >
                      {status}
                    </button>
                  ))}
                </div>
              </div>
              <Button
                variant="secondary"
                size="sm"
                leftIcon={<RefreshCw className="w-4 h-4" />}
                onClick={loadTransfers}
              >
                Refresh
              </Button>
            </div>
          </Card>

          {isLoading ? (
            <div className="flex items-center justify-center h-64">
              <RefreshCw className="w-8 h-8 animate-spin text-primary-600" />
            </div>
          ) : !transfers || transfers.length === 0 ? (
            <Card>
              <div className="text-center py-12">
                <TrendingUp className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-gray-900 mb-2">No transfers yet</h3>
                <p className="text-gray-600 mb-6">Create your first transfer to get started</p>
                <Button
                  variant="primary"
                  leftIcon={<Plus className="w-5 h-5" />}
                  onClick={() => router.push('/transfers/new')}
                >
                  Create Transfer
                </Button>
              </div>
            </Card>
          ) : (
            <>
              <Card padding="none">
                <div className="divide-y divide-gray-100">
                  {transfers.map((transfer) => (
                    <div
                      key={transfer.id}
                      className="p-4 hover:bg-gray-50 transition-colors cursor-pointer"
                      onClick={() => router.push(`/transfers/${transfer.id}`)}
                    >
                      <div className="flex items-center justify-between">
                        <div className="flex items-center space-x-4">
                          <div className="bg-primary-50 rounded-full p-3">
                            <TrendingUp className="w-5 h-5 text-primary-600" />
                          </div>
                          <div>
                            <p className="font-medium text-gray-900">Transfer</p>
                            <div className="text-sm text-gray-600 space-y-1">
                              <p className="font-mono">From: {transfer.sourceAccount}</p>
                              <p className="font-mono">To: {transfer.destinationAccount}</p>
                              {transfer.reference && (
                                <p className="text-xs text-gray-500">{transfer.reference}</p>
                              )}
                            </div>
                          </div>
                        </div>
                        <div className="text-right">
                          <p className="text-lg font-semibold text-gray-900">
                            {formatCurrency(transfer.amount, transfer.currency)}
                          </p>
                          <span className={`inline-block text-xs px-2 py-1 rounded-full ${getStatusColor(transfer.status)}`}>
                            {transfer.status}
                          </span>
                          {mounted && (
                            <p className="text-xs text-gray-500 mt-1">
                              {format(new Date(transfer.createdAt), 'MMM dd, yyyy HH:mm')}
                            </p>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </Card>
                  
              {totalPages > 1 && (
                <div className="flex justify-center space-x-2">
                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                    disabled={currentPage === 1}
                  >
                    Previous
                  </Button>
                  <div className="flex items-center px-4 py-2 text-sm text-gray-700">
                    Page {currentPage} of {totalPages}
                  </div>
                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                    disabled={currentPage === totalPages}
                  >
                    Next
                  </Button>
                </div>
              )}
            </>
          )}
        </div>
      </Layout>
    </>
  );
}
