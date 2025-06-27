import { useState, useEffect, useRef } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { productService } from '../services/productService';
import { categoryService } from '../services/categoryService';
import { brandService } from '../services/brandService';
import { debounce } from 'lodash';
import FilterSection from '../components/FilterSection';
import ProductCard from '../components/ProductCard';
import { motion, AnimatePresence } from 'framer-motion';
import { Grid, List, SlidersHorizontal, Search, X, ChevronDown, Package, ArrowUpDown } from 'lucide-react';

const Products = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [hoveredProduct, setHoveredProduct] = useState(null);
  const [viewMode, setViewMode] = useState('grid');
  const [searchInput, setSearchInput] = useState(searchParams.get('search') || '');
  const [showFilters, setShowFilters] = useState(false);
  const [pagination, setPagination] = useState({
    pageIndex: 1,
    pageSize: 12,
    itemCount: 0,
    pageCount: 1,
    hasNext: false,
    hasPrevious: false
  });

  const [priceInputs, setPriceInputs] = useState({
    minPrice: searchParams.get('minPrice') || '',
    maxPrice: searchParams.get('maxPrice') || ''
  });

  const [filters, setFilters] = useState({
    categoryId: searchParams.get('categoryId') || '',
    brandId: searchParams.get('brandId') || '',
    minPrice: searchParams.get('minPrice') || '',
    maxPrice: searchParams.get('maxPrice') || '',
    sortBy: searchParams.get('sortBy') || 'newest',
    pageIndex: parseInt(searchParams.get('pageIndex')) || 1,
    pageSize: 12,
    search: searchParams.get('search') || ''
  });

  const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: {
        staggerChildren: 0.05,
        delayChildren: 0.1
      }
    }
  };

  const itemVariants = {
    hidden: {
      opacity: 0,
      y: 30,
      scale: 0.95,
    },
    visible: {
      opacity: 1,
      y: 0,
      scale: 1,
      transition: {
        type: "spring",
        stiffness: 100,
        damping: 15,
        mass: 0.8,
      }
    }
  };

  const sortOptions = [
    { value: 'newest', label: 'Mới nhất', icon: '🆕' },
    { value: 'price_asc', label: 'Giá thấp đến cao', icon: '⬆️' },
    { value: 'price_desc', label: 'Giá cao đến thấp', icon: '⬇️' },
    { value: 'name_asc', label: 'Tên A-Z', icon: '🔤' },
    { value: 'popular', label: 'Phổ biến', icon: '🔥' }
  ];

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [categoriesData, brandsData] = await Promise.all([
          categoryService.getAll(),
          brandService.getAll()
        ]);
        setCategories(categoriesData || []);
        setBrands(brandsData || []);
      } catch (error) {
        console.error('Error fetching categories and brands:', error);
        setError('Không thể tải danh mục và thương hiệu');
      }
    };

    fetchData();
  }, []);

  useEffect(() => {
    const fetchProducts = async () => {
      setLoading(true);
      setError(null);
      try {
        const filterParams = {
          pageIndex: filters.pageIndex,
          pageSize: filters.pageSize
        };

        if (filters.categoryId) filterParams.categoryId = filters.categoryId;
        if (filters.brandId) filterParams.brandId = filters.brandId;
        if (filters.minPrice !== '') filterParams.minPrice = Number(filters.minPrice);
        if (filters.maxPrice !== '') filterParams.maxPrice = Number(filters.maxPrice);
        if (filters.search) filterParams.search = filters.search;
        if (filters.sortBy) filterParams.sortBy = filters.sortBy;

        const estimatedOrderTotal = 1000000;
        const response = await productService.getAll(filterParams.pageIndex, filterParams.pageSize, filterParams, estimatedOrderTotal);

        if (response) {
          setProducts(response.data || []);
          setPagination({
            pageIndex: response.pageIndex || 1,
            pageSize: response.pageSize || 12,
            itemCount: response.itemCount || 0,
            pageCount: response.pageCount || 1,
            hasNext: response.hasNext || false,
            hasPrevious: response.hasPrevious || false
          });
        } else {
          setProducts([]);
          setError('Không thể tải danh sách sản phẩm');
        }
      } catch (error) {
        console.error('Error fetching products:', error);
        setError('Có lỗi xảy ra khi tải sản phẩm');
        setProducts([]);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, [filters]);

  const debouncedPriceFilterRef = useRef(
    debounce((minPrice, maxPrice) => {
      const newFilters = { ...filters };
      if (minPrice !== '') {
        newFilters.minPrice = minPrice;
      } else {
        delete newFilters.minPrice;
      }
      if (maxPrice !== '') {
        newFilters.maxPrice = maxPrice;
      } else {
        delete newFilters.maxPrice;
      }
      setFilters(newFilters);
    }, 500)
  );

  const handlePriceChange = (type, value) => {
    const newPriceInputs = { ...priceInputs, [type]: value };
    setPriceInputs(newPriceInputs);
    debouncedPriceFilterRef.current(newPriceInputs.minPrice, newPriceInputs.maxPrice);
  };

  const handleFilterChange = (key, value) => {
    const newFilters = { ...filters, [key]: value };
    
    if (key !== 'pageIndex') {
      newFilters.pageIndex = 1;
    }
    
    setFilters(newFilters);
    
    const newSearchParams = new URLSearchParams(searchParams);
    if (value) {
      newSearchParams.set(key, value);
    } else {
      newSearchParams.delete(key);
    }
    setSearchParams(newSearchParams);
  };

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    handleFilterChange('search', searchInput);
  };

  const clearFilters = () => {
    setFilters({
      categoryId: '',
      brandId: '',
      minPrice: '',
      maxPrice: '',
      sortBy: 'newest',
      pageIndex: 1,
      pageSize: 12,
      search: ''
    });
    setPriceInputs({ minPrice: '', maxPrice: '' });
    setSearchInput('');
    setSearchParams({});
  };

  const handlePageChange = (newPage) => {
    if (newPage >= 1 && newPage <= pagination.pageCount) {
      handleFilterChange('pageIndex', newPage);
    }
  };

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  const getActiveFiltersCount = () => {
    let count = 0;
    if (filters.categoryId) count++;
    if (filters.brandId) count++;
    if (filters.minPrice) count++;
    if (filters.maxPrice) count++;
    if (filters.search) count++;
    if (filters.sortBy !== 'newest') count++;
    return count;
  };

  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center p-8 bg-white rounded-lg shadow-lg max-w-md">
          <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <X className="h-8 w-8 text-red-600" />
          </div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Có lỗi xảy ra</h3>
          <p className="text-gray-600 mb-4">{error}</p>
          <button
            onClick={() => window.location.reload()}
            className="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors"
          >
            Thử lại
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header Section */}
      <div className="bg-white border-b border-gray-200">
        <div className="container mx-auto px-4 py-6">
          <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6">
            {/* Title & Breadcrumb */}
            <div>
              <div className="flex items-center gap-2 text-sm text-gray-600 mb-2">
                <Link to="/" className="hover:text-purple-600 transition-colors">Trang chủ</Link>
                <span>/</span>
                <span className="font-medium">Sản phẩm</span>
              </div>
              <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
                <Package className="h-8 w-8 text-purple-600" />
                Tất cả sản phẩm
                <span className="text-lg font-normal text-gray-500">({pagination.itemCount})</span>
              </h1>
            </div>

            {/* Search & View Controls */}
            <div className="flex items-center gap-4">
              {/* Search */}
              <form onSubmit={handleSearchSubmit} className="relative">
                <input
                  type="text"
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                  placeholder="Tìm kiếm sản phẩm..."
                  className="w-64 pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                />
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
                {searchInput && (
                  <button
                    type="button"
                    onClick={() => {
                      setSearchInput('');
                      handleFilterChange('search', '');
                    }}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                  >
                    <X className="h-4 w-4" />
                  </button>
                )}
              </form>

              {/* View Mode Toggle */}
              <div className="flex bg-gray-100 rounded-lg p-1">
                <button
                  onClick={() => setViewMode('grid')}
                  className={`flex items-center gap-2 px-3 py-1.5 rounded-md text-sm font-medium transition-colors ${
                    viewMode === 'grid' 
                      ? 'bg-white text-gray-900 shadow-sm' 
                      : 'text-gray-600 hover:text-gray-900'
                  }`}
                >
                  <Grid className="h-4 w-4" />
                  Lưới
                </button>
                <button
                  onClick={() => setViewMode('list')}
                  className={`flex items-center gap-2 px-3 py-1.5 rounded-md text-sm font-medium transition-colors ${
                    viewMode === 'list' 
                      ? 'bg-white text-gray-900 shadow-sm' 
                      : 'text-gray-600 hover:text-gray-900'
                  }`}
                >
                  <List className="h-4 w-4" />
                  Danh sách
                </button>
              </div>
            </div>
          </div>

          {/* Filters & Sort Bar */}
          <div className="flex flex-wrap items-center justify-between gap-4 mt-6 pt-6 border-t border-gray-200">
            {/* Filter Toggle */}
            <div className="flex items-center gap-4">
              <button
                onClick={() => setShowFilters(!showFilters)}
                className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
              >
                <SlidersHorizontal className="h-4 w-4" />
                Bộ lọc
                {getActiveFiltersCount() > 0 && (
                  <span className="bg-purple-600 text-white text-xs rounded-full px-2 py-0.5 ml-1">
                    {getActiveFiltersCount()}
                  </span>
                )}
              </button>

              {/* Active Filters Display */}
              {getActiveFiltersCount() > 0 && (
                <div className="flex items-center gap-2">
                  <span className="text-sm text-gray-600">Đang lọc:</span>
                  <div className="flex flex-wrap gap-2">
                    {filters.categoryId && (
                      <span className="inline-flex items-center gap-1 px-3 py-1 bg-purple-100 text-purple-800 rounded-full text-sm">
                        {categories.find(c => c.id === filters.categoryId)?.name}
                        <button onClick={() => handleFilterChange('categoryId', '')} className="hover:text-purple-600">
                          <X className="h-3 w-3" />
                        </button>
                      </span>
                    )}
                    {filters.brandId && (
                      <span className="inline-flex items-center gap-1 px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm">
                        {brands.find(b => b.id === filters.brandId)?.name}
                        <button onClick={() => handleFilterChange('brandId', '')} className="hover:text-blue-600">
                          <X className="h-3 w-3" />
                        </button>
                      </span>
                    )}
                    {(filters.minPrice || filters.maxPrice) && (
                      <span className="inline-flex items-center gap-1 px-3 py-1 bg-green-100 text-green-800 rounded-full text-sm">
                        {filters.minPrice && filters.maxPrice 
                          ? `${formatPrice(filters.minPrice)} - ${formatPrice(filters.maxPrice)}`
                          : filters.minPrice 
                            ? `Từ ${formatPrice(filters.minPrice)}`
                            : `Đến ${formatPrice(filters.maxPrice)}`
                        }
                        <button onClick={() => {
                          handleFilterChange('minPrice', '');
                          handleFilterChange('maxPrice', '');
                          setPriceInputs({ minPrice: '', maxPrice: '' });
                        }} className="hover:text-green-600">
                          <X className="h-3 w-3" />
                        </button>
                      </span>
                    )}
                    {filters.search && (
                      <span className="inline-flex items-center gap-1 px-3 py-1 bg-yellow-100 text-yellow-800 rounded-full text-sm">
                        "{filters.search}"
                        <button onClick={() => {
                          handleFilterChange('search', '');
                          setSearchInput('');
                        }} className="hover:text-yellow-600">
                          <X className="h-3 w-3" />
                        </button>
                      </span>
                    )}
                  </div>
                  <button onClick={clearFilters} className="text-sm text-red-600 hover:text-red-700 font-medium">
                    Xóa tất cả
                  </button>
                </div>
              )}
            </div>

            {/* Sort Dropdown */}
            <div className="relative">
              <select
                value={filters.sortBy}
                onChange={(e) => handleFilterChange('sortBy', e.target.value)}
                className="appearance-none bg-white border border-gray-300 rounded-lg px-4 py-2 pr-8 text-sm font-medium focus:ring-2 focus:ring-purple-500 focus:border-transparent"
              >
                {sortOptions.map(option => (
                  <option key={option.value} value={option.value}>
                    {option.icon} {option.label}
                  </option>
                ))}
              </select>
              <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400 pointer-events-none" />
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="container mx-auto px-4 py-8">
        <div className="flex gap-8">
          {/* Sidebar Filters */}
          <AnimatePresence>
            {showFilters && (
              <motion.div
                initial={{ opacity: 0, x: -300 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -300 }}
                className="w-80 flex-shrink-0"
              >
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 sticky top-4">
                  <FilterSection
                    filters={filters}
                    onFilterChange={handleFilterChange}
                    priceInputs={priceInputs}
                    onPriceChange={handlePriceChange}
                    onClearFilters={clearFilters}
                  />
                </div>
              </motion.div>
            )}
          </AnimatePresence>

          {/* Products Grid */}
          <div className="flex-1 min-w-0">
            {loading ? (
              <div className={`grid gap-6 ${viewMode === 'grid' ? 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4' : 'grid-cols-1'}`}>
                {[...Array(8)].map((_, index) => (
                  <div key={index} className="animate-pulse">
                    <div className="bg-gray-200 h-64 rounded-lg mb-4"></div>
                    <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                    <div className="h-4 bg-gray-200 rounded w-1/2"></div>
                  </div>
                ))}
              </div>
            ) : products.length === 0 ? (
              <div className="text-center py-16">
                <div className="w-24 h-24 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-6">
                  <Package className="h-12 w-12 text-gray-400" />
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-2">Không tìm thấy sản phẩm</h3>
                <p className="text-gray-600 mb-6">Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm của bạn</p>
                <button
                  onClick={clearFilters}
                  className="px-6 py-3 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors"
                >
                  Xóa tất cả bộ lọc
                </button>
              </div>
            ) : (
              <>
                <motion.div
                  className={`grid gap-6 ${viewMode === 'grid' ? 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4' : 'grid-cols-1'}`}
                  variants={containerVariants}
                  initial="hidden"
                  animate="visible"
                  key={`${filters.pageIndex}-${viewMode}`}
                >
                  {products.map((product, index) => (
                    <motion.div
                      key={product.id}
                      variants={itemVariants}
                      whileHover={{ y: -4, scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                    >
                      <ProductCard
                        product={product}
                        viewMode={viewMode}
                        isHovered={hoveredProduct === product.id}
                        onMouseEnter={() => setHoveredProduct(product.id)}
                        onMouseLeave={() => setHoveredProduct(null)}
                      />
                    </motion.div>
                  ))}
                </motion.div>

                {/* Pagination */}
                {pagination.pageCount > 1 && (
                  <div className="flex items-center justify-center gap-2 mt-8">
                    <button
                      onClick={() => handlePageChange(pagination.pageIndex - 1)}
                      disabled={!pagination.hasPrevious}
                      className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      <ArrowUpDown className="h-4 w-4 rotate-90" />
                      Trước
                    </button>

                    <div className="flex gap-1">
                      {Array.from({ length: Math.min(5, pagination.pageCount) }, (_, i) => {
                        const page = Math.max(1, Math.min(pagination.pageCount - 4, pagination.pageIndex - 2)) + i;
                        return (
                          <button
                            key={page}
                            onClick={() => handlePageChange(page)}
                            className={`w-10 h-10 text-sm font-medium rounded-lg transition-all ${
                              pagination.pageIndex === page
                                ? 'bg-gradient-to-r from-purple-600 to-pink-600 text-white shadow-lg'
                                : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
                            }`}
                          >
                            {page}
                          </button>
                        );
                      })}
                    </div>

                    <button
                      onClick={() => handlePageChange(pagination.pageIndex + 1)}
                      disabled={!pagination.hasNext}
                      className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      Sau
                      <ArrowUpDown className="h-4 w-4 -rotate-90" />
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Products;
