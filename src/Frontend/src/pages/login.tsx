import React, { useState, FormEvent } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { useRouter } from 'next/router';
import { Button } from '@/components/Button';
import { Input } from '@/components/Input';
import { Card } from '@/components/Card';
import { Mail, Lock, AlertCircle } from 'lucide-react';
import Head from 'next/head';

export default function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const { login, isAuthenticated } = useAuth();
  const router = useRouter();

  React.useEffect(() => {
    if (isAuthenticated) {
      router.push('/dashboard');
    }
  }, [isAuthenticated, router]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      await login({ username, password });
    } catch (err: any) {
      setError(err.response?.data?.message || 'Invalid credentials. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <>
      <Head>
        <title>Login - TransferHub</title>
        <meta name="description" content="Login to TransferHub" />
      </Head>

      <div className="min-h-screen bg-gradient-to-br from-primary-50 via-white to-primary-50 flex items-center justify-center px-4 py-12">
        <div className="w-full max-w-md">
          <div className="text-center mb-8">
            <h1 className="text-4xl font-bold bg-gradient-to-r from-primary-600 to-primary-800 bg-clip-text text-transparent mb-2">
              TransferHub
            </h1>
            <p className="text-gray-600">Sign in to your account</p>
          </div>

          <Card className="backdrop-blur-sm bg-white/80">
            <form onSubmit={handleSubmit} className="space-y-6">
              {error && (
                <div className="flex items-start space-x-3 p-4 bg-red-50 border border-red-200 rounded-xl">
                  <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                  <p className="text-sm text-red-800">{error}</p>
                </div>
              )}

              <Input
                label="Username"
                type="text"
                placeholder="Enter your username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                leftIcon={<Mail className="w-5 h-5" />}
                required
                autoComplete="username"
                disabled={isLoading}
              />

              <Input
                label="Password"
                type="password"
                placeholder="Enter your password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                leftIcon={<Lock className="w-5 h-5" />}
                required
                autoComplete="current-password"
                disabled={isLoading}
              />

              <Button
                type="submit"
                variant="primary"
                size="lg"
                fullWidth
                isLoading={isLoading}
              >
                Sign In
              </Button>
            </form>
          </Card>

          <Card className="mt-4 backdrop-blur-sm bg-white/80" padding="sm">
            <div className="text-sm text-gray-600">
              <p className="font-medium mb-2">Demo Users:</p>
              <ul className="space-y-1 text-xs">
                <li>• <span className="font-mono">alice</span> / <span className="font-mono">password123</span></li>
                <li>• <span className="font-mono">bob</span> / <span className="font-mono">password123</span></li>
                <li>• <span className="font-mono">charlie</span> / <span className="font-mono">password123</span></li>
              </ul>
            </div>
          </Card>

        </div>
      </div>
    </>
  );
}
