import { createBrowserRouter } from 'react-router-dom';
import Layout from '../components/Layout';
import Home from '../pages/Home';
import ProductDetail from '../pages/ProductDetail';
import Products from '../pages/Products';
import Cart from '../pages/Cart';
import Checkout from '../pages/Checkout';
import OrderConfirmation from '../pages/OrderConfirmation';
import PaymentResult from '../pages/PaymentResult';
import Orders from '../pages/Orders';
import OrderDetail from '../pages/OrderDetail';
import NotFound from '../pages/NotFound';
import LoginPage from '../pages/LoginPage';
import RegisterPage from '../pages/RegisterPage';
import ForgotPasswordPage from '../pages/ForgotPasswordPage';
import ResetPasswordPage from '../pages/ResetPasswordPage';
import ProfilePage from '../pages/ProfilePage';
import ChangePasswordPage from '../pages/ChangePasswordPage';
import VoucherHistory from '../pages/VoucherHistory';
import ProtectedRoute from '../components/ProtectedRoute';

const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [
      {
        index: true,
        element: <Home />
      },
      {
        path: 'products',
        element: <Products />
      },
      {
        path: 'products/:id',
        element: <ProductDetail />
      },
      {
        path: 'cart',
        element: <Cart />
      },
      {
        path: 'checkout',
        element: <Checkout />
      },
      {
        path: 'order-confirmation/:orderId',
        element: <OrderConfirmation />
      },
      {
        path: 'payment-result',
        element: <PaymentResult />
      },
      // Protected routes
      {
        path: 'orders',
        element: (
          <ProtectedRoute>
            <Orders />
          </ProtectedRoute>
        )
      },
      {
        path: 'order-detail/:orderId',
        element: (
          <ProtectedRoute>
            <OrderDetail />
          </ProtectedRoute>
        )
      },
      {
        path: 'profile',
        element: (
          <ProtectedRoute>
            <ProfilePage />
          </ProtectedRoute>
        )
      },
      {
        path: 'change-password',
        element: (
          <ProtectedRoute>
            <ChangePasswordPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'vouchers',
        element: (
          <ProtectedRoute>
            <VoucherHistory />
          </ProtectedRoute>
        )
      },
      {
        path: '*',
        element: <NotFound />
      }
    ]
  },
  // Authentication routes (no layout)
  {
    path: 'login',
    element: <LoginPage />
  },
  {
    path: 'register',
    element: <RegisterPage />
  },
  {
    path: 'forgot-password',
    element: <ForgotPasswordPage />
  },
  {
    path: 'reset-password',
    element: <ResetPasswordPage />
  }
]);

export default router; 