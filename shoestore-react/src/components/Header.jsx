import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { Popover, PopoverContent, PopoverTrigger } from "./ui/popover";
import { cn } from "../lib/utils";
import { categoryService } from '../services/categoryService';
import { brandService } from '../services/brandService';
import { useCart } from '../context/CartContext';
import { ShoppingCart, Menu, X } from 'lucide-react';

const Header = ({ onCartClick }) => {
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const { cartCount } = useCart();
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [categoriesData, brandsData] = await Promise.all([
          categoryService.getAll(),
          brandService.getAll()
        ]);
        setCategories(categoriesData);
        setBrands(brandsData);
      } catch (error) {
        console.error('Error fetching data:', error);
      }
    };

    fetchData();
  }, []);

  const handleCategoryClick = (categoryId) => {
    navigate(`/products?categoryId=${categoryId}`);
    setIsMenuOpen(false);
  };

  const handleBrandClick = (brandId) => {
    navigate(`/products?brandId=${brandId}`);
    setIsMenuOpen(false);
  };

  const handleSearch = (e) => {
    e.preventDefault();
    const searchTerm = e.target.search.value;
    if (searchTerm.trim()) {
      navigate(`/products?search=${encodeURIComponent(searchTerm.trim())}`);
      setIsMenuOpen(false);
    }
  };

  const isActive = (path) => {
    return location.pathname === path;
  };

  return (
    <header className="sticky top-0 z-50 w-full border-b bg-white">
      {/* Main Header */}
      <div className="container mx-auto px-4">
        <div className="flex h-16 items-center justify-between">
          {/* Logo and Shop Name */}
          <Link to="/" className="flex items-center space-x-2">
            <img src="/logo.png" alt="Logo" className="h-8 w-8" />
            <span className="text-xl font-bold">ShoeStore</span>
          </Link>

          {/* Desktop Navigation */}
          <nav className="hidden md:flex items-center space-x-6">
            <Link 
              to="/products" 
              className={cn(
                "text-gray-700 hover:text-gray-900",
                isActive('/products') && "font-semibold text-black"
              )}
            >
              Sản phẩm
            </Link>

            {/* Categories Dropdown */}
            <Popover>
              <PopoverTrigger asChild>
                <button className="flex items-center space-x-1 text-gray-700 hover:text-gray-900">
                  <span>Danh mục</span>
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="24"
                    height="24"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    className="h-4 w-4"
                  >
                    <path d="m6 9 6 6 6-6"/>
                  </svg>
                </button>
              </PopoverTrigger>
              <PopoverContent className="w-[200px] p-0" align="start">
                <div className="grid gap-1 p-2">
                  {categories.map((category) => (
                    <button
                      key={category.id}
                      onClick={() => handleCategoryClick(category.id)}
                      className="flex items-center rounded-md px-2 py-1.5 text-sm hover:bg-gray-100"
                    >
                      {category.name}
                    </button>
                  ))}
                </div>
              </PopoverContent>
            </Popover>

            {/* Brands Dropdown */}
            <Popover>
              <PopoverTrigger asChild>
                <button className="flex items-center space-x-1 text-gray-700 hover:text-gray-900">
                  <span>Thương hiệu</span>
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="24"
                    height="24"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    className="h-4 w-4"
                  >
                    <path d="m6 9 6 6 6-6"/>
                  </svg>
                </button>
              </PopoverTrigger>
              <PopoverContent className="w-[200px] p-0" align="start">
                <div className="grid gap-1 p-2">
                  {brands.map((brand) => (
                    <button
                      key={brand.id}
                      onClick={() => handleBrandClick(brand.id)}
                      className="flex items-center rounded-md px-2 py-1.5 text-sm hover:bg-gray-100"
                    >
                      {brand.name}
                    </button>
                  ))}
                </div>
              </PopoverContent>
            </Popover>

            <Link 
              to="/promotions" 
              className={cn(
                "text-gray-700 hover:text-gray-900",
                isActive('/promotions') && "font-semibold text-black"
              )}
            >
              Khuyến mãi
            </Link>
          </nav>

          {/* Search Bar */}
          <div className="hidden md:flex flex-1 max-w-md mx-4">
            <form onSubmit={handleSearch} className="relative w-full">
              <input
                type="text"
                name="search"
                placeholder="Tìm kiếm sản phẩm..."
                className="w-full px-4 py-2 border rounded-full focus:outline-none focus:ring-2 focus:ring-gray-200"
              />
              <button type="submit" className="absolute right-3 top-1/2 -translate-y-1/2">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="24"
                  height="24"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  className="h-5 w-5 text-gray-400"
                >
                  <circle cx="11" cy="11" r="8"/>
                  <path d="m21 21-4.3-4.3"/>
                </svg>
              </button>
            </form>
          </div>

          {/* Right Section */}
          <div className="flex items-center space-x-4">
            {/* Cart Icon */}
            <button
              onClick={onCartClick}
              className="p-2 text-gray-700 hover:text-gray-900 relative"
            >
              <ShoppingCart className="w-6 h-6" />
              {cartCount > 0 && (
                <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center">
                  {cartCount > 99 ? '99+' : cartCount}
                </span>
              )}
            </button>

            {/* Mobile Menu Button */}
            <button
              onClick={() => setIsMenuOpen(!isMenuOpen)}
              className="md:hidden p-2 text-gray-700 hover:text-gray-900"
            >
              {isMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
            </button>

            {/* Login/Avatar - Desktop */}
            <div className="hidden md:block">
              {isLoggedIn ? (
                <Popover>
                  <PopoverTrigger asChild>
                    <button className="flex items-center space-x-2">
                      <img
                        src="/avatar.png"
                        alt="Avatar"
                        className="h-8 w-8 rounded-full"
                      />
                    </button>
                  </PopoverTrigger>
                  <PopoverContent className="w-[200px] p-0" align="end">
                    <div className="grid gap-1 p-2">
                      <Link
                        to="/profile"
                        className="flex items-center rounded-md px-2 py-1.5 text-sm hover:bg-gray-100"
                      >
                        Tài khoản
                      </Link>
                      <Link
                        to="/orders"
                        className="flex items-center rounded-md px-2 py-1.5 text-sm hover:bg-gray-100"
                      >
                        Đơn hàng
                      </Link>
                      <button
                        onClick={() => setIsLoggedIn(false)}
                        className="flex items-center rounded-md px-2 py-1.5 text-sm text-red-600 hover:bg-gray-100"
                      >
                        Đăng xuất
                      </button>
                    </div>
                  </PopoverContent>
                </Popover>
              ) : (
                <Link
                  to="/login"
                  className="text-gray-700 hover:text-gray-900"
                >
                  Đăng nhập
                </Link>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Mobile Menu */}
      <div
        className={cn(
          "md:hidden fixed inset-0 z-40 bg-white transform transition-transform duration-300 ease-in-out",
          isMenuOpen ? "translate-x-0" : "translate-x-full"
        )}
      >
        <div className="flex flex-col h-full">
          {/* Mobile Menu Header */}
          <div className="flex items-center justify-between p-4 border-b">
            <h2 className="text-lg font-semibold">Menu</h2>
            <button
              onClick={() => setIsMenuOpen(false)}
              className="p-2 hover:bg-gray-100 rounded-full"
            >
              <X className="w-6 h-6" />
            </button>
          </div>

          {/* Mobile Search */}
          <div className="p-4 border-b">
            <form onSubmit={handleSearch} className="relative">
              <input
                type="text"
                name="search"
                placeholder="Tìm kiếm sản phẩm..."
                className="w-full px-4 py-2 border rounded-full focus:outline-none focus:ring-2 focus:ring-gray-200"
              />
              <button type="submit" className="absolute right-3 top-1/2 -translate-y-1/2">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="24"
                  height="24"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  className="h-5 w-5 text-gray-400"
                >
                  <circle cx="11" cy="11" r="8"/>
                  <path d="m21 21-4.3-4.3"/>
                </svg>
              </button>
            </form>
          </div>

          {/* Mobile Navigation */}
          <nav className="flex-1 overflow-y-auto p-4">
            <div className="space-y-4">
              <Link
                to="/products"
                className={cn(
                  "block text-lg font-medium text-gray-900",
                  isActive('/products') && "font-semibold text-black"
                )}
                onClick={() => setIsMenuOpen(false)}
              >
                Sản phẩm
              </Link>

              {/* Categories */}
              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-2">Danh mục</h3>
                <div className="space-y-2">
                  {categories.map((category) => (
                    <button
                      key={category.id}
                      onClick={() => handleCategoryClick(category.id)}
                      className="block w-full text-left px-2 py-1 text-gray-600 hover:bg-gray-100 rounded-md"
                    >
                      {category.name}
                    </button>
                  ))}
                </div>
              </div>

              {/* Brands */}
              <div>
                <h3 className="text-lg font-medium text-gray-900 mb-2">Thương hiệu</h3>
                <div className="space-y-2">
                  {brands.map((brand) => (
                    <button
                      key={brand.id}
                      onClick={() => handleBrandClick(brand.id)}
                      className="block w-full text-left px-2 py-1 text-gray-600 hover:bg-gray-100 rounded-md"
                    >
                      {brand.name}
                    </button>
                  ))}
                </div>
              </div>

              <Link
                to="/promotions"
                className={cn(
                  "block text-lg font-medium text-gray-900",
                  isActive('/promotions') && "font-semibold text-black"
                )}
                onClick={() => setIsMenuOpen(false)}
              >
                Khuyến mãi
              </Link>

              {/* Mobile Login/Avatar */}
              {isLoggedIn ? (
                <div className="space-y-2">
                  <Link
                    to="/profile"
                    className={cn(
                      "block text-lg font-medium text-gray-900",
                      isActive('/profile') && "font-semibold text-black"
                    )}
                    onClick={() => setIsMenuOpen(false)}
                  >
                    Tài khoản
                  </Link>
                  <Link
                    to="/orders"
                    className={cn(
                      "block text-lg font-medium text-gray-900",
                      isActive('/orders') && "font-semibold text-black"
                    )}
                    onClick={() => setIsMenuOpen(false)}
                  >
                    Đơn hàng
                  </Link>
                  <button
                    onClick={() => {
                      setIsLoggedIn(false);
                      setIsMenuOpen(false);
                    }}
                    className="block w-full text-left text-lg font-medium text-red-600"
                  >
                    Đăng xuất
                  </button>
                </div>
              ) : (
                <Link
                  to="/login"
                  className={cn(
                    "block text-lg font-medium text-gray-900",
                    isActive('/login') && "font-semibold text-black"
                  )}
                  onClick={() => setIsMenuOpen(false)}
                >
                  Đăng nhập
                </Link>
              )}
            </div>
          </nav>
        </div>
      </div>

      {/* SubHeader - Marquee */}
      <div className="bg-gray-100 py-2">
        <div className="container mx-auto px-4">
          <div className="overflow-hidden whitespace-nowrap">
            <div className="animate-marquee inline-block">
              <span className="text-sm text-gray-600">
                🎉 Khuyến mãi đặc biệt: Giảm giá lên đến 50% cho tất cả sản phẩm! 🎉
              </span>
            </div>
          </div>
        </div>
      </div>
    </header>
  );
};

export default Header; 