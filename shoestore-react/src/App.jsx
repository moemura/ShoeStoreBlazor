import { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Header from "./components/Header";
import Footer from "./components/Footer";
import CartDrawer from "./components/CartDrawer";
import Products from "./pages/Products";
import ProductDetail from './pages/ProductDetail';
import Promotions from './pages/Promotions';

function App() {
  const [isCartOpen, setIsCartOpen] = useState(false);

  return (
    <Router>
      <div className="min-h-screen flex flex-col">
        <Header onCartClick={() => setIsCartOpen(true)} />
        <main className="flex-grow">
          <Routes>
            <Route path="/" element={<div>Home Page</div>} />
            <Route path="/products" element={<Products />} />
            <Route path="/products/:id" element={<ProductDetail />} />
            <Route path="/promotions" element={<Promotions />} />
            <Route path="/login" element={<div>Login Page</div>} />
            <Route path="/profile" element={<div>Profile Page</div>} />
            <Route path="/orders" element={<div>Orders Page</div>} />
            <Route path="/checkout" element={<div>Checkout Page</div>} />
            <Route path="/about" element={<div>About Page</div>} />
            <Route path="/contact" element={<div>Contact Page</div>} />
            <Route path="/faq" element={<div>FAQ Page</div>} />
            <Route path="/shipping" element={<div>Shipping Page</div>} />
            <Route path="/returns" element={<div>Returns Page</div>} />
            <Route path="/privacy" element={<div>Privacy Page</div>} />
          </Routes>
        </main>
        <Footer />
        <CartDrawer isOpen={isCartOpen} onClose={() => setIsCartOpen(false)} />
      </div>
    </Router>
  );
}

export default App;
