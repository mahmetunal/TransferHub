import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '@/contexts/AuthContext';
import { Layout } from '@/components/Layout';
import { Card } from '@/components/Card';
import { Button } from '@/components/Button';
import { transferService } from '@/services/api.service';
import { Transfer } from '@/types/api';
import { ArrowLeft, RefreshCw, Clock, CheckCircle, XCircle, AlertCircle } from 'lucide-react';
import { format } from 'date-fns';
import Head from 'next/head';

export default function TransferDetailPage() {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();
  const { id } = router.query;
  const [transfer, setTransfer] = useState<Transfer | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
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
    if (id && typeof id === 'string') {
      loadTransfer();
    }
  }, [id]);

  const loadTransfer = async () => {
    if (!id || typeof id !== 'string') return;

    try {
      setIsLoading(true);
      setError('');
      const data = await transferService.getTransfer(id);
      setTransfer(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load transfer details');
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

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Completed':
        return <CheckCircle className="w-12 h-12 text-green-600" />;
      case 'Pending':
        return <Clock className="w-12 h-12 text-yellow-600" />;
      case 'Failed':
        return <XCircle className="w-12 h-12 text-red-600" />;
      default:
        return <AlertCircle className="w-12 h-12 text-gray-600" />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Completed':
        return 'bg-green-100 text-green-800';
      case 'Pending':
        return 'bg-yellow-100 text-yellow-800';
      case 'Failed':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'Completed':
        return 'Transfer completed successfully';
      case 'Pending':
        return 'Transfer is being processed';
      case 'Failed':
        return 'Transfer failed';
      default:
        return 'Unknown status';
    }
  };

  if (authLoading || isLoading) {
    return (
      <Layout>
        <div className="flex items-center justify-center h-64">
          <RefreshCw className="w-8 h-8 animate-spin text-primary-600" />
        </div>
      </Layout>
    );
  }

  if (error || !transfer) {
    return (
      <Layout>
        <Card className="bg-red-50 border-red-200">
          <p className="text-red-800">{error || 'Transfer not found'}</p>
        </Card>
      </Layout>
    );
  }

  return (
    <>
      <Head>
        <title>Transfer Details - TransferHub</title>
      </Head>

      <Layout>
        <div className="max-w-2xl mx-auto space-y-6">
          <div className="flex items-center justify-between">
            <Button
              variant="ghost"
              size="sm"
              leftIcon={<ArrowLeft className="w-4 h-4" />}
              onClick={() => router.push('/transfers')}
            >
              Back to Transfers
            </Button>
            <Button
              variant="secondary"
              size="sm"
              leftIcon={<RefreshCw className="w-4 h-4" />}
              onClick={loadTransfer}
            >
              Refresh
            </Button>
          </div>

          <Card className="text-center">
            <div className="flex justify-center mb-4">
              {getStatusIcon(transfer.status)}
            </div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">
              {formatCurrency(transfer.amount, transfer.currency)}
            </h2>
            <span className={`inline-block text-sm px-4 py-2 rounded-full ${getStatusColor(transfer.status)}`}>
              {transfer.status}
            </span>
            <p className="text-sm text-gray-600 mt-2">
              {getStatusText(transfer.status)}
            </p>
          </Card>

          <Card>
            <h3 className="text-lg font-semibold mb-4">Transfer Details</h3>
            <div className="space-y-4">
              <div>
                <label className="text-sm text-gray-600">Transfer ID</label>
                <p className="font-mono text-sm text-gray-900 mt-1">{transfer.id}</p>
              </div>

              <div className="border-t pt-4">
                <label className="text-sm text-gray-600">From Account</label>
                <p className="font-mono text-sm text-gray-900 mt-1">{transfer.sourceAccount}</p>
              </div>

              <div className="border-t pt-4">
                <label className="text-sm text-gray-600">To Account</label>
                <p className="font-mono text-sm text-gray-900 mt-1">{transfer.destinationAccount}</p>
              </div>

              <div className="border-t pt-4">
                <label className="text-sm text-gray-600">Amount</label>
                <p className="text-lg font-semibold text-gray-900 mt-1">
                  {formatCurrency(transfer.amount, transfer.currency)}
                </p>
              </div>

              {transfer.reference && (
                <div className="border-t pt-4">
                  <label className="text-sm text-gray-600">Reference</label>
                  <p className="text-sm text-gray-900 mt-1">{transfer.reference}</p>
                </div>
              )}

              <div className="border-t pt-4">
                <label className="text-sm text-gray-600">Created At</label>
                {mounted && (
                  <p className="text-sm text-gray-900 mt-1">
                    {format(new Date(transfer.createdAt), 'PPpp')}
                  </p>
                )}
              </div>

              {transfer.completedAt && (
                <div className="border-t pt-4">
                  <label className="text-sm text-gray-600">Completed At</label>
                  {mounted && (
                    <p className="text-sm text-gray-900 mt-1">
                      {format(new Date(transfer.completedAt), 'PPpp')}
                    </p>
                  )}
                </div>
              )}

              <div className="border-t pt-4">
                <label className="text-sm text-gray-600">Initiated By</label>
                <p className="font-mono text-sm text-gray-900 mt-1">{transfer.initiatedBy}</p>
              </div>
            </div>
          </Card>
                
          <div className="flex space-x-4">
            <Button
              variant="secondary"
              fullWidth
              onClick={() => router.push('/transfers/new')}
            >
              Make Another Transfer
            </Button>
            <Button
              variant="primary"
              fullWidth
              onClick={() => router.push('/dashboard')}
            >
              Back to Dashboard
            </Button>
          </div>
        </div>
      </Layout>
    </>
  );
}
