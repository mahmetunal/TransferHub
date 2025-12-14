import React, { useState, FormEvent, useEffect } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '@/contexts/AuthContext';
import { Layout } from '@/components/Layout';
import { Card } from '@/components/Card';
import { Button } from '@/components/Button';
import { Input } from '@/components/Input';
import { Select } from '@/components/Select';
import { accountService, transferService } from '@/services/api.service';
import { Account, CreateTransferRequest } from '@/types/api';
import { ArrowLeft, AlertCircle, CheckCircle, Send } from 'lucide-react';
import Head from 'next/head';

export default function NewTransferPage() {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();
  const { from } = router.query;
  
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [sourceAccount, setSourceAccount] = useState('');
  const [destinationAccount, setDestinationAccount] = useState('');
  const [amount, setAmount] = useState('');
  const [currency, setCurrency] = useState('USD');
  const [reference, setReference] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const [transferId, setTransferId] = useState('');

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

  useEffect(() => {
    if (from && typeof from === 'string') {
      setSourceAccount(from);
      const account = accounts.find((a) => a.iban === from);
      if (account) {
        setCurrency(account.currency);
      }
    }
  }, [from, accounts]);

  const loadAccounts = async () => {
    try {
      const data = await accountService.getAccounts();
      setAccounts(data || []);
      if (data && data.length > 0 && !sourceAccount) {
        setSourceAccount(data[0].iban);
        setCurrency(data[0].currency);
      }
    } catch (err: any) {
      setError('Failed to load accounts');
      setAccounts([]);
    }
  };

  const handleSourceAccountChange = (iban: string) => {
    setSourceAccount(iban);
    const account = accounts.find((a) => a.iban === iban);
    if (account) {
      setCurrency(account.currency);
    }
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess(false);
    setIsLoading(true);

    try {
      const transferAmount = parseFloat(amount);
      if (isNaN(transferAmount) || transferAmount <= 0) {
        setError('Please enter a valid amount');
        setIsLoading(false);
        return;
      }

      if (sourceAccount === destinationAccount) {
        setError('Source and destination accounts must be different');
        setIsLoading(false);
        return;
      }

      const sourceAcc = accounts.find((a) => a.iban === sourceAccount);
      if (sourceAcc && transferAmount > sourceAcc.balance) {
        setError('Insufficient balance');
        setIsLoading(false);
        return;
      }

      const request: CreateTransferRequest = {
        sourceAccount,
        destinationAccount,
        amount: transferAmount,
        currency,
        reference: reference || undefined,
      };

      const result = await transferService.createTransfer(request);
      setTransferId(result.transferId);
      setSuccess(true);

      setTimeout(() => {
        router.push(`/transfers/${result.transferId}`);
      }, 2000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create transfer');
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

  const selectedAccount = accounts.find((a) => a.iban === sourceAccount);

  return (
    <>
      <Head>
        <title>New Transfer - TransferHub</title>
      </Head>

      <Layout>
        <div className="max-w-2xl mx-auto">
          <div className="mb-6">
            <Button
              variant="ghost"
              size="sm"
              leftIcon={<ArrowLeft className="w-4 h-4" />}
              onClick={() => router.back()}
            >
              Back
            </Button>
          </div>

          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900">New Transfer</h1>
            <p className="mt-2 text-gray-600">
              Send money to another account instantly
            </p>
          </div>

          <Card>
            <form onSubmit={handleSubmit} className="space-y-6">
              {error && (
                <div className="flex items-start space-x-3 p-4 bg-red-50 border border-red-200 rounded-xl">
                  <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                  <p className="text-sm text-red-800">{error}</p>
                </div>
              )}

              {success && (
                <div className="flex items-start space-x-3 p-4 bg-green-50 border border-green-200 rounded-xl">
                  <CheckCircle className="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" />
                  <div className="text-sm text-green-800">
                    <p className="font-medium">Transfer created successfully!</p>
                    <p className="text-xs mt-1">Transfer ID: {transferId}</p>
                  </div>
                </div>
              )}

              {!accounts || accounts.length === 0 ? (
                <div className="text-center py-8">
                  <p className="text-gray-600 mb-4">You need at least one account to make a transfer</p>
                  <Button
                    variant="primary"
                    onClick={() => router.push('/accounts/new')}
                  >
                    Create Account
                  </Button>
                </div>
              ) : (
                <>
                  <Select
                    label="From Account"
                    value={sourceAccount}
                    onChange={(e) => handleSourceAccountChange(e.target.value)}
                    options={accounts.map((account) => ({
                      value: account.iban,
                      label: `${account.iban} - ${formatCurrency(account.balance, account.currency)}`,
                    }))}
                    helperText={selectedAccount ? `Available: ${formatCurrency(selectedAccount.balance, selectedAccount.currency)}` : ''}
                    required
                    disabled={isLoading || success}
                  />

                  <Input
                    label="To Account (IBAN)"
                    type="text"
                    placeholder="Enter destination IBAN"
                    value={destinationAccount}
                    onChange={(e) => setDestinationAccount(e.target.value)}
                    helperText="Enter the IBAN of the destination account"
                    required
                    disabled={isLoading || success}
                  />

                  <Input
                    label="Amount"
                    type="number"
                    placeholder="0.00"
                    value={amount}
                    onChange={(e) => setAmount(e.target.value)}
                    helperText={`Currency: ${currency}`}
                    step="0.01"
                    min="0.01"
                    required
                    disabled={isLoading || success}
                  />

                  <Input
                    label="Reference (Optional)"
                    type="text"
                    placeholder="Payment for..."
                    value={reference}
                    onChange={(e) => setReference(e.target.value)}
                    helperText="Add a note to this transfer"
                    disabled={isLoading || success}
                  />

                  <div className="flex space-x-4 pt-4">
                    <Button
                      type="button"
                      variant="secondary"
                      fullWidth
                      onClick={() => router.back()}
                      disabled={isLoading || success}
                    >
                      Cancel
                    </Button>
                    <Button
                      type="submit"
                      variant="primary"
                      fullWidth
                      leftIcon={<Send className="w-5 h-5" />}
                      isLoading={isLoading}
                      disabled={success}
                    >
                      Send Transfer
                    </Button>
                  </div>
                </>
              )}
            </form>
          </Card>
            
          <Card className="mt-6 bg-blue-50 border-blue-200" padding="md">
            <div className="text-sm text-blue-900">
              <p className="font-medium mb-2">💡 Transfer Information:</p>
              <ul className="space-y-1 text-blue-800">
                <li>• Transfers are processed instantly</li>
                <li>• All transfers are secured and encrypted</li>
                <li>• You can track transfer status in real-time</li>
                <li>• Transfers are idempotent (safe to retry)</li>
              </ul>
            </div>
          </Card>
        </div>
      </Layout>
    </>
  );
}
