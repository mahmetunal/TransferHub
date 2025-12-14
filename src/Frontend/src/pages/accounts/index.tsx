import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '@/contexts/AuthContext';
import { Layout } from '@/components/Layout';
import { Card } from '@/components/Card';
import { Button } from '@/components/Button';
import { accountService } from '@/services/api.service';
import { Account } from '@/types/api';
import { Plus, RefreshCw, Wallet } from 'lucide-react';
import Head from 'next/head';

export default function AccountsPage() {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  React.useEffect(() => {
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

  if (authLoading) {
    return <Layout><div>Loading...</div></Layout>;
  }

  return (
    <>
      <Head>
        <title>Accounts - TransferHub</title>
      </Head>

      <Layout>
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">Accounts</h1>
              <p className="mt-1 text-gray-600">Manage your accounts and balances</p>
            </div>
            <Button
              variant="primary"
              leftIcon={<Plus className="w-5 h-5" />}
              onClick={() => router.push('/accounts/new')}
            >
              New Account
            </Button>
          </div>

          {error && (
            <Card className="bg-red-50 border-red-200">
              <p className="text-red-800">{error}</p>
            </Card>
          )}

          {isLoading ? (
            <div className="flex items-center justify-center h-64">
              <RefreshCw className="w-8 h-8 animate-spin text-primary-600" />
            </div>
          ) : !accounts || accounts.length === 0 ? (
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
            <>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
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
                        <p className="font-mono text-sm text-gray-900 mb-3 break-all">{account.iban}</p>
                        <p className="text-2xl font-bold text-gray-900">
                          {formatCurrency(account.balance, account.currency)}
                        </p>
                      </div>
                    </Card>
                  </div>
                ))}
              </div>

              <div className="flex justify-center">
                <Button
                  variant="secondary"
                  leftIcon={<RefreshCw className="w-4 h-4" />}
                  onClick={loadAccounts}
                >
                  Refresh Accounts
                </Button>
              </div>
            </>
          )}
        </div>
      </Layout>
    </>
  );
}
