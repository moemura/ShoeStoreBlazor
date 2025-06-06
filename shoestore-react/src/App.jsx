import { RouterProvider } from 'react-router-dom';
import router from './routes';
import { CartProvider } from './context/CartContext';
import { ToastProvider } from './components/Toast';

const App = () => {
  return (
    <ToastProvider>
      <CartProvider>
        <RouterProvider router={router} />
      </CartProvider>
    </ToastProvider>
  );
};

export default App;
