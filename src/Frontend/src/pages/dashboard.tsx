import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '@/contexts/AuthContext';
import { Layout } from '@/components/Layout';
import { Card } from '@/components/Card';
import { Button } from '@/components/Button';
import { accountService } from '@/services/api.service';
import { Account } from '@/types/api';
import { Wallet, Plus, TrendingUp, ArrowRight, RefreshCw } from 'lucide-react';
import Head from 'next/head';

export default function DashboardPage() {
  const { isAuthenticated, isLoading: authLoading, user } = useAuth();
  const router = useRouter();
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push('/login');
    }
  }, [isAuthenticated, authLoading, router]);

  useEffect(() => {
    if (isAuthenticated) {
      loadAccounts();
    }
  }, [isAuthenticated]);

  const loadAccounts = async () => {
    try {
      setIsLoading(true);
      setError('');
      const data = await accountService.getAccounts();
      setAccounts(data || []);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load accounts');
      setAccounts([]);
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

  const EXCHANGE_RATES: Record<string, number> = {
    USD: 1.0,
    EUR: 1.09,
    GBP: 1.27,
    TRY: 0.029,
  };

  const convertToUSD = (amount: number, currency: string): number => {
    const rate = EXCHANGE_RATES[currency] || 1.0;
    return amount * rate;
  };

  const getTotalBalance = () => {
    if (!accounts || accounts.length === 0) return 0;
    return accounts.reduce((sum, account) => {
      return sum + convertToUSD(account.balance, account.currency);
    }, 0);
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

  return (
    <>
      <Head>
        <title>Dashboard - TransferHub</title>
      </Head>

      <Layout>
        <div className="space-y-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
            <p className="mt-1 text-gray-600">Welcome back, {user?.email}</p>
          </div>

          {error && (
            <Card className="bg-red-50 border-red-200">
              <p className="text-red-800">{error}</p>
            </Card>
          )}

          <Card className="bg-gradient-to-br from-primary-600 to-primary-800 text-white">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-primary-100 text-sm font-medium mb-1">Total Balance</p>
                <h2 className="text-4xl font-bold">
                  {formatCurrency(getTotalBalance(), 'USD')}
                </h2>
                <p className="text-primary-100 text-sm mt-2">
                  Across {accounts.length} {accounts.length === 1 ? 'account' : 'accounts'}
                </p>
                <p className="text-primary-100 text-xs mt-3 opacity-90">
                  💱 Converted to USD using fixed exchange rates
                </p>
              </div>
              <div className="bg-white/10 rounded-full p-4">
                <Wallet className="w-10 h-10" />
              </div>
            </div>
          </Card>

          <Card className="bg-blue-50 border-blue-200" padding="sm">
            <div className="flex items-start space-x-3">
              <div className="text-blue-600 mt-0.5">ℹ️</div>
              <div className="flex-1">
                <p className="text-sm text-blue-900 font-medium mb-1">Exchange Rate Information</p>
                <p className="text-xs text-blue-800">
                  All balances are converted to USD using fixed exchange rates: 
                  <span className="font-mono"> EUR: 1.09, GBP: 1.27, TRY: 0.029</span>
                  . These are approximate rates and may differ from real-time market rates.
                </p>
              </div>
            </div>
          </Card>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Card hover className="group" padding="md">
              <button
                onClick={() => router.push('/accounts/new')}
                className="w-full text-left"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <h3 className="font-semibold text-gray-900 mb-1">Create Account</h3>
                    <p className="text-sm text-gray-600">Open a new account</p>
                  </div>
                  <div className="bg-primary-50 group-hover:bg-primary-100 transition-colors rounded-full p-3">
                    <Plus className="w-5 h-5 text-primary-600" />
                  </div>
                </div>
              </button>
            </Card>

            <Card hover className="group" padding="md">
              <button
                onClick={() => router.push('/transfers/new')}
                className="w-full text-left"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <h3 className="font-semibold text-gray-900 mb-1">Send Money</h3>
                    <p className="text-sm text-gray-600">Make a transfer</p>
                  </div>
                  <div className="bg-green-50 group-hover:bg-green-100 transition-colors rounded-full p-3">
                    <TrendingUp className="w-5 h-5 text-green-600" />
                  </div>
                </div>
              </button>
            </Card>

            <Card hover className="group" padding="md">
              <button
                onClick={() => router.push('/transfers')}
                className="w-full text-left"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <h3 className="font-semibold text-gray-900 mb-1">View Transfers</h3>
                    <p className="text-sm text-gray-600">Transaction history</p>
                  </div>
                  <div className="bg-purple-50 group-hover:bg-purple-100 transition-colors rounded-full p-3">
                    <ArrowRight className="w-5 h-5 text-purple-600" />
                  </div>
                </div>
              </button>
            </Card>
          </div>

          <div>
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-2xl font-bold text-gray-900">Your Accounts</h2>
              <Button
                variant="secondary"
                size="sm"
                leftIcon={<RefreshCw className="w-4 h-4" />}
                onClick={loadAccounts}
              >
                Refresh
              </Button>
            </div>

            {!accounts || accounts.length === 0 ? (
              <Card>
                <div className="text-center py-12">
                  <Wallet className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                  <h3 className="text-lg font-semibold text-gray-900 mb-2">No accounts yet</h3>
                  <p className="text-gray-600 mb-6">Create your first account to get started</p>
                  <Button
                    variant="primary"
                    leftIcon={<Plus className="w-5 h-5" />}
                    onClick={() => router.push('/accounts/new')}
                  >
                    Create Account
                  </Button>
                </div>
              </Card>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {accounts.map((account) => (
                  <div
                    key={account.id}
                    onClick={() => router.push(`/accounts/${account.iban}`)}
                  >
                    <Card hover className="cursor-pointer">
                      <div className="flex items-center justify-between mb-4">
                        <div className="bg-primary-50 rounded-full p-3">
                          <Wallet className="w-6 h-6 text-primary-600" />
                        </div>
                        <span className="text-xs font-medium text-gray-500 bg-gray-100 px-3 py-1 rounded-full">
                          {account.currency}
                        </span>
                      </div>
                      <div>
                        <p className="text-sm text-gray-600 mb-1">IBAN</p>
                        <p className="font-mono text-sm text-gray-900 mb-3">{account.iban}</p>
                        <p className="text-2xl font-bold text-gray-900">
                          {formatCurrency(account.balance, account.currency)}
                        </p>
                      </div>
                    </Card>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </Layout>
    </>
  );
}
