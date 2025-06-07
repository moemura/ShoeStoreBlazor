import { RouterProvider } from 'react-router-dom';
import router from './routes';
import { CartProvider } from './context/CartContext';
import { AuthProvider } from './context/AuthContext';
import { OrderProvider } from './context/OrderContext';
import { ToastProvider } from './components/Toast';

const App = () => {
  return (
    <ToastProvider>
      <AuthProvider>
        <CartProvider>
          <OrderProvider>
            <RouterProvider router={router} />
          </OrderProvider>
        </CartProvider>
      </AuthProvider>
    </ToastProvider>
  );
};

export default App;
