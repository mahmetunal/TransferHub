# TransferHub Frontend

A modern, responsive web application for TransferHub - built with Next.js, TypeScript, and Tailwind CSS.

## 🚀 Features

- **Authentication**: Secure JWT-based authentication
- **Account Management**: Create and manage multiple accounts with different currencies
- **Money Transfers**: Instant transfers between accounts with real-time status tracking
- **Dashboard**: Beautiful overview of all your accounts and balances
- **Responsive Design**: Works seamlessly on desktop, tablet, and mobile devices
- **Modern UI**: Revolut-inspired design with smooth animations and transitions

## 🛠 Technology Stack

- **Framework**: Next.js 14 (React)
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **HTTP Client**: Axios
- **Icons**: Lucide React
- **Date Formatting**: date-fns

## 📋 Prerequisites

- Node.js 18.x or higher
- npm or yarn
- BFF API Gateway running on `http://localhost:5031` (or configured URL)

## 🏃‍♂️ Getting Started

### Option 1: Docker (Recommended for Quick Start)

```bash
# From project root, start entire stack
docker-compose up -d

# Or start only frontend and dependencies
docker-compose up -d frontend

# View logs
docker-compose logs -f frontend
```

The application will be available at [http://localhost:3000](http://localhost:3000)

### Option 2: Local Development

#### 1. Install Dependencies

```bash
npm install
# or
yarn install
```

#### 2. Environment Configuration

Create a `.env.local` file in the root directory:

```bash
NEXT_PUBLIC_API_URL=http://localhost:5031
```

#### 3. Run Development Server

```bash
npm run dev
# or
yarn dev
```

The application will be available at [http://localhost:3000](http://localhost:3000)

#### 4. Build for Production

```bash
npm run build
npm start
# or
yarn build
yarn start
```

## 🔐 Demo Users

The application comes with pre-configured demo users:

| Username | Password     | User ID          |
|----------|--------------|------------------|
| alice    | password123  | user-alice-001   |
| bob      | password123  | user-bob-002     |
| charlie  | password123  | user-charlie-003 |


## 🎨 Key Features

### Dashboard
- Overview of all accounts
- Total balance calculation
- Quick actions for common tasks
- Recent activity summary

### Account Management
- Create new accounts with different currencies
- View account details and balance
- Top up account balance
- View transaction history

### Transfers
- Create instant transfers between accounts
- Real-time transfer status tracking
- Filter transfers by status
- Detailed transfer information
- Idempotent transfer creation

### Authentication
- Secure JWT-based authentication
- Automatic token refresh
- Protected routes
- Session persistence

## 🔌 API Integration

The frontend communicates with the Gateway, which proxies requests to:

- **Auth API**: User authentication and token generation
- **Account API**: Account management operations
- **Transfer API**: Money transfer operations

All API calls include:
- JWT token authentication
- Idempotency keys for write operations
- Proper error handling
- Loading states

## 🎯 Available Scripts

### Development
- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm start` - Start production server
- `npm run lint` - Run ESLint
- `npm run type-check` - Run TypeScript type checking

### Docker
- `docker-compose up -d frontend` - Start frontend container
- `docker-compose build frontend` - Build frontend image
- `docker-compose logs -f frontend` - View frontend logs
- `docker-compose down` - Stop all containers

## 🌐 Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_URL` | BFF Gateway URL | `http://localhost:5031` |

## 🔒 Security Features

- JWT token-based authentication
- Automatic token expiration handling
- Protected routes with authentication guards
- HTTPS support in production
- XSS protection via React
- CSRF protection via SameSite cookies

## 📱 Responsive Design

The application is fully responsive with breakpoints:

- **Mobile**: < 768px
- **Tablet**: 768px - 1024px
- **Desktop**: > 1024px

---

Built with ❤️ using Next.js and TypeScript
