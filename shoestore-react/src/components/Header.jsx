import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { cn } from "../lib/utils";
import { categoryService } from '../services/categoryService';
import { brandService } from '../services/brandService';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { ShoppingCart, Menu, X, User, Search, ChevronDown, Heart, Bell } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import ShoeLogo from './ShoeLogo';

const Header = ({ onCartClick }) => {
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isScrolled, setIsScrolled] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [isSearchFocused, setIsSearchFocused] = useState(false);
  const [hoveredNav, setHoveredNav] = useState(null);
  const { cartCount } = useCart();
  const { isAuthenticated, user, logout } = useAuth();
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

  // Handle scroll effect
  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20);
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const handleCategoryClick = (categoryId) => {
    navigate(`/products?categoryId=${categoryId}`);
    setIsMenuOpen(false);
    setHoveredNav(null);
  };

  const handleBrandClick = (brandId) => {
    navigate(`/products?brandId=${brandId}`);
    setIsMenuOpen(false);
    setHoveredNav(null);
  };

  const handleSearch = (e) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`/products?search=${encodeURIComponent(searchQuery.trim())}`);
      setIsMenuOpen(false);
      setSearchQuery('');
    }
  };

  const handleLogout = async () => {
    await logout();
    setIsMenuOpen(false);
    navigate('/');
  };

  const isActive = (path) => {
    return location.pathname === path;
  };

  const getUserDisplayName = () => {
    return user?.fullName || user?.email || 'Người dùng';
  };

  return (
    <motion.header 
      className={cn(
        "sticky top-0 z-50 w-full transition-all duration-300",
        isScrolled 
          ? "bg-white/95 backdrop-blur-md shadow-lg border-b border-gray-200/50" 
          : "bg-white border-b border-gray-100"
      )}
      initial={{ y: -100 }}
      animate={{ y: 0 }}
      transition={{ duration: 0.6, ease: "easeOut" }}
    >
      {/* Top Banner */}
      <div className="bg-gradient-to-r from-purple-600 to-pink-600 text-white text-center py-2 text-sm">
        <div className="container mx-auto px-4">
          <span className="flex items-center justify-center gap-2">
            🎉 Miễn phí vận chuyển cho đơn hàng trên 500K
            <Link to="/promotions" className="underline hover:no-underline font-medium">
              Xem chi tiết
            </Link>
          </span>
        </div>
      </div>

      {/* Main Header */}
      <div className="container mx-auto px-4">
        <div className="flex h-16 items-center justify-between">
          {/* Logo */}
          <Link to="/" className="flex items-center space-x-3 group flex-shrink-0">
            <div className="relative">
              <div className="absolute inset-0 bg-gradient-to-r from-purple-600 to-pink-600 rounded-lg blur opacity-20 group-hover:opacity-40 transition-opacity"></div>
              <div className="relative">
                <ShoeLogo className="h-12 w-12 group-hover:scale-110 transition-transform duration-300" />
              </div>
            </div>
            <div className="flex flex-col">
              <span className="text-xl font-bold bg-gradient-to-r from-purple-600 to-pink-600 bg-clip-text text-transparent">
                ShoeStore
              </span>
              <span className="text-xs text-gray-500 -mt-1">Fashion & Style</span>
            </div>
          </Link>

          {/* Desktop Navigation - Centered */}
          <nav className="hidden lg:flex items-center justify-center space-x-8 flex-1">
            <Link 
              to="/products" 
              className={cn(
                "relative text-gray-700 hover:text-purple-600 font-medium transition-colors duration-200",
                isActive('/products') && "text-purple-600"
              )}
            >
              Sản phẩm
              {isActive('/products') && (
                <motion.div
                  className="absolute -bottom-1 left-0 right-0 h-0.5 bg-gradient-to-r from-purple-600 to-pink-600"
                  layoutId="activeTab"
                />
              )}
            </Link>

            {/* Categories Dropdown - Hover based */}
            <div 
              className="relative"
              onMouseEnter={() => setHoveredNav('categories')}
              onMouseLeave={() => setHoveredNav(null)}
            >
              <button className="flex items-center space-x-1 text-gray-700 hover:text-purple-600 font-medium transition-colors duration-200 group">
                <span>Danh mục</span>
                <ChevronDown className="h-4 w-4 transition-transform group-hover:rotate-180" />
              </button>
              
              <AnimatePresence>
                {hoveredNav === 'categories' && (
                  <motion.div
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, y: 10 }}
                    transition={{ duration: 0.2 }}
                    className="absolute top-full left-0 mt-2 w-[280px] bg-white rounded-lg shadow-xl border border-gray-100 overflow-hidden z-50"
                  >
                    <div className="p-4 bg-gradient-to-r from-purple-50 to-pink-50 border-b">
                      <h3 className="font-semibold text-gray-900">Danh mục sản phẩm</h3>
                    </div>
                    <div className="max-h-80 overflow-y-auto">
                      {categories.map((category, index) => (
                        <motion.button
                          key={category.id}
                          initial={{ opacity: 0, x: -20 }}
                          animate={{ opacity: 1, x: 0 }}
                          transition={{ delay: index * 0.05 }}
                          onClick={() => handleCategoryClick(category.id)}
                          className="w-full flex items-center px-4 py-3 text-sm hover:bg-purple-50 transition-colors group"
                        >
                          <div className="w-8 h-8 bg-gradient-to-br from-purple-100 to-pink-100 rounded-lg flex items-center justify-center mr-3 group-hover:scale-110 transition-transform">
                            <span className="text-purple-600 font-bold text-xs">
                              {category.name.charAt(0)}
                            </span>
                          </div>
                          <span className="text-gray-700 group-hover:text-purple-600 font-medium">
                            {category.name}
                          </span>
                        </motion.button>
                      ))}
                    </div>
                  </motion.div>
                )}
              </AnimatePresence>
            </div>

            {/* Brands Dropdown - Hover based */}
            <div 
              className="relative"
              onMouseEnter={() => setHoveredNav('brands')}
              onMouseLeave={() => setHoveredNav(null)}
            >
              <button className="flex items-center space-x-1 text-gray-700 hover:text-purple-600 font-medium transition-colors duration-200 group">
                <span>Thương hiệu</span>
                <ChevronDown className="h-4 w-4 transition-transform group-hover:rotate-180" />
              </button>
              
              <AnimatePresence>
                {hoveredNav === 'brands' && (
                  <motion.div
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, y: 10 }}
                    transition={{ duration: 0.2 }}
                    className="absolute top-full left-0 mt-2 w-[280px] bg-white rounded-lg shadow-xl border border-gray-100 overflow-hidden z-50"
                  >
                    <div className="p-4 bg-gradient-to-r from-purple-50 to-pink-50 border-b">
                      <h3 className="font-semibold text-gray-900">Thương hiệu nổi bật</h3>
                    </div>
                    <div className="max-h-80 overflow-y-auto">
                      {brands.map((brand, index) => (
                        <motion.button
                          key={brand.id}
                          initial={{ opacity: 0, x: -20 }}
                          animate={{ opacity: 1, x: 0 }}
                          transition={{ delay: index * 0.05 }}
                          onClick={() => handleBrandClick(brand.id)}
                          className="w-full flex items-center px-4 py-3 text-sm hover:bg-purple-50 transition-colors group"
                        >
                          <div className="w-8 h-8 bg-gradient-to-br from-purple-100 to-pink-100 rounded-lg flex items-center justify-center mr-3 group-hover:scale-110 transition-transform">
                            <span className="text-purple-600 font-bold text-xs">
                              {brand.name.charAt(0)}
                            </span>
                          </div>
                          <span className="text-gray-700 group-hover:text-purple-600 font-medium">
                            {brand.name}
                          </span>
                        </motion.button>
                      ))}
                    </div>
                  </motion.div>
                )}
              </AnimatePresence>
            </div>

            <Link 
              to="/promotions" 
              className={cn(
                "relative text-gray-700 hover:text-purple-600 font-medium transition-colors duration-200 flex items-center gap-1",
                isActive('/promotions') && "text-purple-600"
              )}
            >
              <span>Khuyến mãi</span>
              <span className="bg-red-500 text-white text-xs px-1.5 py-0.5 rounded-full animate-pulse">Hot</span>
              {isActive('/promotions') && (
                <motion.div
                  className="absolute -bottom-1 left-0 right-0 h-0.5 bg-gradient-to-r from-purple-600 to-pink-600"
                  layoutId="activeTab"
                />
              )}
            </Link>
          </nav>

          {/* Search Bar - Reduced width */}
          <div className="hidden md:flex items-center justify-center flex-shrink-0">
            <form onSubmit={handleSearch} className="relative group">
              <div className={cn(
                "relative flex items-center transition-all duration-300",
                isSearchFocused ? "scale-105" : "scale-100"
              )}>
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  onFocus={() => setIsSearchFocused(true)}
                  onBlur={() => setIsSearchFocused(false)}
                  placeholder="Tìm kiếm sản phẩm..."
                  className={cn(
                    "w-80 px-4 py-3 pl-12 pr-4 rounded-full border-2 transition-all duration-300 bg-gray-50",
                    isSearchFocused 
                      ? "border-purple-300 bg-white shadow-lg focus:outline-none focus:ring-4 focus:ring-purple-100" 
                      : "border-gray-200 focus:outline-none focus:border-purple-300"
                  )}
                />
                <Search className={cn(
                  "absolute left-4 h-5 w-5 transition-colors duration-300",
                  isSearchFocused ? "text-purple-500" : "text-gray-400"
                )} />
                <AnimatePresence>
                  {searchQuery && (
                    <motion.button
                      initial={{ opacity: 0, scale: 0 }}
                      animate={{ opacity: 1, scale: 1 }}
                      exit={{ opacity: 0, scale: 0 }}
                      type="button"
                      onClick={() => setSearchQuery('')}
                      className="absolute right-4 p-1 rounded-full hover:bg-gray-200 transition-colors"
                    >
                      <X className="h-4 w-4 text-gray-400" />
                    </motion.button>
                  )}
                </AnimatePresence>
              </div>
            </form>
          </div>

          {/* Right Side Actions */}
          <div className="flex items-center space-x-4 flex-shrink-0">
            {/* Cart */}
            <button 
              onClick={onCartClick}
              className="relative p-2 text-gray-700 hover:text-purple-600 transition-colors group"
            >
              <ShoppingCart className="h-6 w-6" />
              <AnimatePresence>
                {cartCount > 0 && (
                  <motion.span
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    exit={{ scale: 0 }}
                    className="absolute -top-2 -right-2 bg-gradient-to-r from-purple-500 to-pink-500 text-white text-xs rounded-full h-6 w-6 flex items-center justify-center font-bold group-hover:scale-110 transition-transform"
                  >
                    {cartCount > 99 ? '99+' : cartCount}
                  </motion.span>
                )}
              </AnimatePresence>
            </button>

            {/* User Menu */}
            {isAuthenticated ? (
              <div 
                className="relative"
                onMouseEnter={() => setHoveredNav('user')}
                onMouseLeave={() => setHoveredNav(null)}
              >
                <button className="flex items-center space-x-2 p-2 rounded-full hover:bg-gray-100 transition-colors group">
                  <div className="w-8 h-8 bg-gradient-to-r from-purple-500 to-pink-500 rounded-full flex items-center justify-center text-white font-bold text-sm">
                    {getUserDisplayName().charAt(0).toUpperCase()}
                  </div>
                  <ChevronDown className="h-4 w-4 text-gray-500 group-hover:text-gray-700" />
                </button>
                
                <AnimatePresence>
                  {hoveredNav === 'user' && (
                    <motion.div
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: 10 }}
                      transition={{ duration: 0.2 }}
                      className="absolute top-full right-0 mt-2 w-[220px] bg-white rounded-lg shadow-xl border border-gray-100 overflow-hidden z-50"
                    >
                      <div className="p-4 bg-gradient-to-r from-purple-50 to-pink-50 border-b">
                        <div className="flex items-center space-x-3">
                          <div className="w-10 h-10 bg-gradient-to-r from-purple-500 to-pink-500 rounded-full flex items-center justify-center text-white font-bold">
                            {getUserDisplayName().charAt(0).toUpperCase()}
                          </div>
                          <div>
                            <p className="font-semibold text-gray-900 text-sm">{getUserDisplayName()}</p>
                            <p className="text-xs text-gray-500">{user?.email}</p>
                          </div>
                        </div>
                      </div>
                      <div className="py-2">
                        <Link
                          to="/profile"
                          onClick={() => setHoveredNav(null)}
                          className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-purple-50 hover:text-purple-600 transition-colors"
                        >
                          <User className="h-4 w-4 mr-3" />
                          Hồ sơ cá nhân
                        </Link>
                        <Link
                          to="/orders"
                          onClick={() => setHoveredNav(null)}
                          className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-purple-50 hover:text-purple-600 transition-colors"
                        >
                          <ShoppingCart className="h-4 w-4 mr-3" />
                          Đơn hàng của tôi
                        </Link>
                        <Link
                          to="/voucher-history"
                          onClick={() => setHoveredNav(null)}
                          className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-purple-50 hover:text-purple-600 transition-colors"
                        >
                          <Heart className="h-4 w-4 mr-3" />
                          Voucher của tôi
                        </Link>
                        <hr className="my-2" />
                        <button
                          onClick={handleLogout}
                          className="flex items-center w-full px-4 py-2 text-sm text-red-600 hover:bg-red-50 transition-colors"
                        >
                          <X className="h-4 w-4 mr-3" />
                          Đăng xuất
                        </button>
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>
              </div>
            ) : (
              <div className="flex items-center space-x-2">
                <Link
                  to="/login"
                  className="px-4 py-2 text-sm font-medium text-gray-700 hover:text-purple-600 transition-colors"
                >
                  Đăng nhập
                </Link>
                <Link
                  to="/register"
                  className="px-4 py-2 text-sm font-medium bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-full hover:from-purple-700 hover:to-pink-700 transition-all hover:scale-105"
                >
                  Đăng ký
                </Link>
              </div>
            )}

            {/* Mobile Menu Button */}
            <button
              onClick={() => setIsMenuOpen(!isMenuOpen)}
              className="lg:hidden p-2 text-gray-700 hover:text-purple-600 transition-colors"
            >
              {isMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
            </button>
          </div>
        </div>
      </div>

      {/* Mobile Menu */}
      <AnimatePresence>
        {isMenuOpen && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: "auto" }}
            exit={{ opacity: 0, height: 0 }}
            className="lg:hidden bg-white border-t border-gray-100 shadow-lg"
          >
            <div className="container mx-auto px-4 py-4 space-y-4">
              {/* Mobile Search */}
              <form onSubmit={handleSearch} className="relative">
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Tìm kiếm..."
                  className="w-full px-4 py-3 pl-12 rounded-full border border-gray-200 focus:outline-none focus:border-purple-300"
                />
                <Search className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
              </form>

              {/* Mobile Navigation */}
              <div className="space-y-2">
                <Link
                  to="/products"
                  onClick={() => setIsMenuOpen(false)}
                  className={cn(
                    "block px-4 py-3 rounded-lg font-medium transition-colors",
                    isActive('/products') ? "bg-purple-50 text-purple-600" : "text-gray-700 hover:bg-gray-50"
                  )}
                >
                  Sản phẩm
                </Link>
                <Link
                  to="/promotions"
                  onClick={() => setIsMenuOpen(false)}
                  className={cn(
                    "block px-4 py-3 rounded-lg font-medium transition-colors",
                    isActive('/promotions') ? "bg-purple-50 text-purple-600" : "text-gray-700 hover:bg-gray-50"
                  )}
                >
                  Khuyến mãi
                </Link>

                {/* Mobile Categories */}
                <div className="space-y-1">
                  <h3 className="px-4 py-2 font-semibold text-gray-900">Danh mục</h3>
                  {categories.slice(0, 5).map((category) => (
                    <button
                      key={category.id}
                      onClick={() => handleCategoryClick(category.id)}
                      className="block w-full text-left px-6 py-2 text-sm text-gray-600 hover:bg-gray-50 hover:text-purple-600 transition-colors"
                    >
                      {category.name}
                    </button>
                  ))}
                </div>

                {/* Mobile User Actions */}
                {!isAuthenticated && (
                  <div className="pt-4 border-t border-gray-100 space-y-2">
                    <Link
                      to="/login"
                      onClick={() => setIsMenuOpen(false)}
                      className="block w-full px-4 py-3 text-center text-gray-700 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
                    >
                      Đăng nhập
                    </Link>
                    <Link
                      to="/register"
                      onClick={() => setIsMenuOpen(false)}
                      className="block w-full px-4 py-3 text-center text-white bg-gradient-to-r from-purple-600 to-pink-600 rounded-lg hover:from-purple-700 hover:to-pink-700 transition-all"
                    >
                      Đăng ký
                    </Link>
                  </div>
                )}
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </motion.header>
  );
};

export default Header; 