import React, { useState, FormEvent } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '@/contexts/AuthContext';
import { Layout } from '@/components/Layout';
import { Card } from '@/components/Card';
import { Button } from '@/components/Button';
import { Input } from '@/components/Input';
import { Select } from '@/components/Select';
import { accountService } from '@/services/api.service';
import { ArrowLeft, AlertCircle, CheckCircle } from 'lucide-react';
import Head from 'next/head';

export default function NewAccountPage() {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();
  const [initialBalance, setInitialBalance] = useState('0');
  const [currency, setCurrency] = useState('USD');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  React.useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push('/login');
    }
  }, [isAuthenticated, authLoading, router]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess(false);
    setIsLoading(true);

    try {
      const balance = parseFloat(initialBalance);
      if (isNaN(balance) || balance < 0) {
        setError('Please enter a valid balance amount');
        setIsLoading(false);
        return;
      }

      const result = await accountService.createAccount({
        initialBalance: balance,
        currency,
      });

      setSuccess(true);
      setTimeout(() => {
        router.push(`/accounts/${result.iban}`);
      }, 1500);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create account');
    } finally {
      setIsLoading(false);
    }
  };

  if (authLoading) {
    return <Layout><div>Loading...</div></Layout>;
  }

  return (
    <>
      <Head>
        <title>Create Account - TransferHub</title>
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
            <h1 className="text-3xl font-bold text-gray-900">Create New Account</h1>
            <p className="mt-2 text-gray-600">
              Open a new account with your preferred currency
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
                  <p className="text-sm text-green-800">
                    Account created successfully! Redirecting...
                  </p>
                </div>
              )}

              <Input
                label="Initial Balance"
                type="number"
                placeholder="0.00"
                value={initialBalance}
                onChange={(e) => setInitialBalance(e.target.value)}
                helperText="Enter the starting balance for this account"
                step="0.01"
                min="0"
                required
                disabled={isLoading || success}
              />

              <Select
                label="Currency"
                value={currency}
                onChange={(e) => setCurrency(e.target.value)}
                options={[
                  { value: 'USD', label: 'USD - US Dollar' },
                  { value: 'EUR', label: 'EUR - Euro' },
                  { value: 'GBP', label: 'GBP - British Pound' },
                  { value: 'TRY', label: 'TRY - Turkish Lira' },
                ]}
                helperText="Select the currency for this account"
                required
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
                  isLoading={isLoading}
                  disabled={success}
                >
                  Create Account
                </Button>
              </div>
            </form>
          </Card>

          <Card className="mt-6 bg-blue-50 border-blue-200" padding="md">
            <div className="text-sm text-blue-900">
              <p className="font-medium mb-2">💡 Account Features:</p>
              <ul className="space-y-1 text-blue-800">
                <li>• Automatic IBAN generation</li>
                <li>• Instant account activation</li>
                <li>• Send and receive transfers</li>
                <li>• Real-time balance updates</li>
              </ul>
            </div>
          </Card>
        </div>
      </Layout>
    </>
  );
}
