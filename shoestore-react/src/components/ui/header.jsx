import { Link } from "react-router-dom";
import { Button } from "./button";
import { ShoppingCart, Search } from "lucide-react";

export function Header() {
  return (
    <header className="border-b">
      <div className="container mx-auto px-4 py-4">
        <div className="flex items-center justify-between">
          <Link to="/" className="text-2xl font-bold">
            ShoeStore
          </Link>
          
          <nav className="hidden md:flex items-center space-x-6">
            <Link to="/" className="text-sm font-medium hover:text-primary">
              Trang chủ
            </Link>
            <Link to="/products" className="text-sm font-medium hover:text-primary">
              Sản phẩm
            </Link>
            <Link to="/categories" className="text-sm font-medium hover:text-primary">
              Danh mục
            </Link>
            <Link to="/brands" className="text-sm font-medium hover:text-primary">
              Thương hiệu
            </Link>
          </nav>

          <div className="flex items-center space-x-4">
            <Button variant="ghost" size="icon">
              <Search className="h-5 w-5" />
            </Button>
            <Button variant="ghost" size="icon">
              <ShoppingCart className="h-5 w-5" />
            </Button>
          </div>
        </div>
      </div>
    </header>
  );
} 