import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '@/contexts/AuthContext';
import { Layout } from '@/components/Layout';
import { Card } from '@/components/Card';
import { Button } from '@/components/Button';
import { Input } from '@/components/Input';
import { accountService, transferService } from '@/services/api.service';
import { Account, Transfer, TopUpRequest } from '@/types/api';
import { ArrowLeft, RefreshCw, Plus, TrendingUp, TrendingDown, AlertCircle, CheckCircle } from 'lucide-react';
import { format } from 'date-fns';
import Head from 'next/head';

export default function AccountDetailPage() {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();
  const { iban } = router.query;
  const [account, setAccount] = useState<Account | null>(null);
  const [transfers, setTransfers] = useState<Transfer[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [showTopUp, setShowTopUp] = useState(false);
  const [topUpAmount, setTopUpAmount] = useState('');
  const [topUpLoading, setTopUpLoading] = useState(false);
  const [topUpError, setTopUpError] = useState('');
  const [topUpSuccess, setTopUpSuccess] = useState(false);
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
    if (iban && typeof iban === 'string') {
      loadAccountData();
    }
  }, [iban]);

  const loadAccountData = async () => {
    if (!iban || typeof iban !== 'string') return;

    try {
      setIsLoading(true);
      setError('');
      const [accountData, transfersData] = await Promise.all([
        accountService.getAccount(iban),
        transferService.listTransfers({ sourceAccount: iban, pageSize: 10 }),
      ]);
      setAccount(accountData);
      setTransfers(transfersData?.transfers || []);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load account details');
      setTransfers([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleTopUp = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!account) return;

    setTopUpError('');
    setTopUpSuccess(false);
    setTopUpLoading(true);

    try {
      const amount = parseFloat(topUpAmount);
      if (isNaN(amount) || amount <= 0) {
        setTopUpError('Please enter a valid amount');
        setTopUpLoading(false);
        return;
      }

      const request: TopUpRequest = {
        amount,
        currency: account.currency,
      };

      await accountService.topUpAccount(account.iban, request);
      setTopUpSuccess(true);
      setTopUpAmount('');
      
      setTimeout(() => {
        setShowTopUp(false);
        setTopUpSuccess(false);
        loadAccountData();
      }, 1500);
    } catch (err: any) {
      setTopUpError(err.response?.data?.message || 'Failed to top up account');
    } finally {
      setTopUpLoading(false);
    }
  };

  const formatCurrency = (amount: number, currency: string) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency,
    }).format(amount);
  };

  const getTransferTypeIcon = (transfer: Transfer) => {
    if (!account) return null;
    const isOutgoing = transfer.sourceAccount === account.iban;
    return isOutgoing ? (
      <TrendingUp className="w-5 h-5 text-red-600" />
    ) : (
      <TrendingDown className="w-5 h-5 text-green-600" />
    );
  };

  const getTransferTypeText = (transfer: Transfer) => {
    if (!account) return '';
    return transfer.sourceAccount === account.iban ? 'Sent' : 'Received';
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

  if (authLoading || isLoading) {
    return (
      <Layout>
        <div className="flex items-center justify-center h-64">
          <RefreshCw className="w-8 h-8 animate-spin text-primary-600" />
        </div>
      </Layout>
    );
  }

  if (error || !account) {
    return (
      <Layout>
        <Card className="bg-red-50 border-red-200">
          <p className="text-red-800">{error || 'Account not found'}</p>
        </Card>
      </Layout>
    );
  }

  return (
    <>
      <Head>
        <title>Account Details - TransferHub</title>
      </Head>

      <Layout>
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div>
              <Button
                variant="ghost"
                size="sm"
                leftIcon={<ArrowLeft className="w-4 h-4" />}
                onClick={() => router.push('/dashboard')}
              >
                Back to Dashboard
              </Button>
            </div>
            <Button
              variant="secondary"
              size="sm"
              leftIcon={<RefreshCw className="w-4 h-4" />}
              onClick={loadAccountData}
            >
              Refresh
            </Button>
          </div>

          <Card className="bg-gradient-to-br from-primary-600 to-primary-800 text-white">
            <div>
              <p className="text-primary-100 text-sm font-medium mb-1">Account Balance</p>
              <h2 className="text-4xl font-bold mb-4">
                {formatCurrency(account.balance, account.currency)}
              </h2>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-primary-100">IBAN</span>
                  <span className="font-mono">{account.iban}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-primary-100">Currency</span>
                  <span>{account.currency}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-primary-100">Account ID</span>
                  <span className="font-mono text-xs">{account.id.substring(0, 8)}...</span>
                </div>
              </div>
            </div>
          </Card>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Button
              variant="primary"
              size="lg"
              leftIcon={<Plus className="w-5 h-5" />}
              onClick={() => setShowTopUp(!showTopUp)}
            >
              Top Up Account
            </Button>
            <Button
              variant="secondary"
              size="lg"
              leftIcon={<TrendingUp className="w-5 h-5" />}
              onClick={() => router.push(`/transfers/new?from=${account.iban}`)}
            >
              Send Money
            </Button>
          </div>

          {showTopUp && (
            <Card>
              <h3 className="text-lg font-semibold mb-4">Top Up Account</h3>
              <form onSubmit={handleTopUp} className="space-y-4">
                {topUpError && (
                  <div className="flex items-start space-x-3 p-4 bg-red-50 border border-red-200 rounded-xl">
                    <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                    <p className="text-sm text-red-800">{topUpError}</p>
                  </div>
                )}

                {topUpSuccess && (
                  <div className="flex items-start space-x-3 p-4 bg-green-50 border border-green-200 rounded-xl">
                    <CheckCircle className="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" />
                    <p className="text-sm text-green-800">Top up successful!</p>
                  </div>
                )}

                <Input
                  label="Amount"
                  type="number"
                  placeholder="0.00"
                  value={topUpAmount}
                  onChange={(e) => setTopUpAmount(e.target.value)}
                  step="0.01"
                  min="0.01"
                  required
                  disabled={topUpLoading || topUpSuccess}
                />

                <div className="flex space-x-4">
                  <Button
                    type="button"
                    variant="secondary"
                    onClick={() => setShowTopUp(false)}
                    disabled={topUpLoading || topUpSuccess}
                  >
                    Cancel
                  </Button>
                  <Button
                    type="submit"
                    variant="primary"
                    isLoading={topUpLoading}
                    disabled={topUpSuccess}
                  >
                    Confirm Top Up
                  </Button>
                </div>
              </form>
            </Card>
          )}

          <div>
            <h2 className="text-2xl font-bold text-gray-900 mb-4">Recent Transfers</h2>
            {!transfers || transfers.length === 0 ? (
              <Card>
                <div className="text-center py-8">
                  <p className="text-gray-600">No transfers yet</p>
                </div>
              </Card>
            ) : (
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
                          <div className="bg-gray-100 rounded-full p-2">
                            {getTransferTypeIcon(transfer)}
                          </div>
                          <div>
                            <p className="font-medium text-gray-900">
                              {getTransferTypeText(transfer)}
                            </p>
                            <p className="text-sm text-gray-600 font-mono">
                              {transfer.sourceAccount === account.iban
                                ? `To: ${transfer.destinationAccount}`
                                : `From: ${transfer.sourceAccount}`}
                            </p>
                            {transfer.reference && (
                              <p className="text-xs text-gray-500 mt-1">{transfer.reference}</p>
                            )}
                          </div>
                        </div>
                        <div className="text-right">
                          <p className={`text-lg font-semibold ${
                            transfer.sourceAccount === account.iban
                              ? 'text-red-600'
                              : 'text-green-600'
                          }`}>
                            {transfer.sourceAccount === account.iban ? '-' : '+'}
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
            )}
          </div>
        </div>
      </Layout>
    </>
  );
}
