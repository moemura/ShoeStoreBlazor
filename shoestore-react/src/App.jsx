import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { Header } from "./components/ui/header";
import { Footer } from "./components/ui/footer";
import { HomePage } from "./pages/Home";
import { ProductsPage } from "./pages/Products";

function App() {
  return (
    <Router>
      <div className="min-h-screen flex flex-col">
        <Header />
        <main className="flex-grow">
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/products" element={<ProductsPage />} />
          </Routes>
        </main>
        <Footer />
      </div>
    </Router>
  );
}

export default App;
