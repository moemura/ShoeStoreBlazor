import { useState, useEffect, useCallback, useRef } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { productService } from '../services/productService';
import { categoryService } from '../services/categoryService';
import { brandService } from '../services/brandService';
import { debounce } from 'lodash';
import MenuSidebar from '../components/MenuSidebar';
import FilterSection from '../components/FilterSection';
import ProductCard from '../components/ProductCard';
import { motion, AnimatePresence } from 'framer-motion';

const Products = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isFilterOpen, setIsFilterOpen] = useState(true);
  const [hoveredProduct, setHoveredProduct] = useState(null);
  const [pagination, setPagination] = useState({
    pageIndex: 1,
    pageSize: 12,
    itemCount: 0,
    pageCount: 1,
    hasNext: false,
    hasPrevious: false
  });

  // Separate display state for price inputs
  const [priceInputs, setPriceInputs] = useState({
    minPrice: searchParams.get('minPrice') || '',
    maxPrice: searchParams.get('maxPrice') || ''
  });

  // Actual filter state
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
        staggerChildren: 0.1,
        delayChildren: 0.2
      }
    }
  };

  const getWavePosition = (index) => {
    const row = Math.floor(index / 4);
    const col = index % 4;
    
    // Tính toán độ trễ dựa trên vị trí trong grid
    const delay = (row * 0.15) + (col * 0.05);
    
    // Tính toán độ lệch ngang dựa trên cột
    const xOffset = (col - 1.5) * 50;
    
    return { delay, xOffset };
  };

  const itemVariants = {
    hidden: (index) => {
      const { xOffset } = getWavePosition(index);
      return {
        opacity: 0,
        y: 100,
        x: xOffset,
        scale: 0.8,
        filter: "blur(8px)"
      };
    },
    visible: (index) => {
      const { delay } = getWavePosition(index);
      return {
        opacity: 1,
        y: 0,
        x: 0,
        scale: 1,
        filter: "blur(0px)",
        transition: {
          type: "spring",
          stiffness: 100,
          damping: 15,
          mass: 1,
          delay
        }
      };
    },
    hover: {
      y: -10,
      scale: 1.02,
      filter: "brightness(1.05)",
      transition: {
        type: "spring",
        stiffness: 400,
        damping: 10
      }
    }
  };

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
        // Chỉ gửi các filter có giá trị
        const filterParams = {
          pageIndex: filters.pageIndex,
          pageSize: filters.pageSize
        };

        if (filters.categoryId) filterParams.categoryId = filters.categoryId;
        if (filters.brandId) filterParams.brandId = filters.brandId;
        if (filters.minPrice !== '') {
          filterParams.minPrice = Number(filters.minPrice);
        }
        if (filters.maxPrice !== '') {
          filterParams.maxPrice = Number(filters.maxPrice);
        }
        if (filters.search) filterParams.search = filters.search;
        if (filters.sortBy) filterParams.sortBy = filters.sortBy;

        console.log('Sending request with params:', filterParams); // Debug log

        const response = await productService.getAll(filterParams.pageIndex, filterParams.pageSize, filterParams);

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

  // Create a ref for the debounced function
  const debouncedPriceFilterRef = useRef(
    debounce((minPrice, maxPrice) => {
      const newFilters = { ...filters };
      // Chỉ cập nhật khi có giá trị
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

      // Update URL params
      const params = new URLSearchParams();
      Object.entries(newFilters).forEach(([key, value]) => {
        if (value !== '') params.set(key, value);
      });
      setSearchParams(params);
    }, 1000)
  ).current;

  // Cleanup debounce on unmount
  useEffect(() => {
    return () => {
      debouncedPriceFilterRef.cancel();
    };
  }, [debouncedPriceFilterRef]);

  // Handle price input changes
  const handlePriceChange = (type, value) => {
    // Update display state immediately
    setPriceInputs(prev => ({
      ...prev,
      [type === 'min' ? 'minPrice' : 'maxPrice']: value
    }));

    // Debounce the filter update
    debouncedPriceFilterRef(
      type === 'min' ? value : priceInputs.minPrice,
      type === 'max' ? value : priceInputs.maxPrice
    );
  };

  const handleFilterChange = (key, value) => {
    // Reset page when changing filters
    const newFilters = { ...filters, [key]: value, pageIndex: 1 };

    // Remove empty filters
    Object.keys(newFilters).forEach(k => {
      if (!newFilters[k] && k !== 'pageIndex' && k !== 'pageSize') {
        delete newFilters[k];
      }
    });

    setFilters(newFilters);

    // Update URL params
    const params = new URLSearchParams();
    Object.entries(newFilters).forEach(([key, value]) => {
      if (value) params.set(key, value);
    });
    setSearchParams(params);
  };

  const clearFilters = () => {
    setFilters({
      pageIndex: 1,
      pageSize: 12,
      sortBy: 'newest'
    });
    setPriceInputs({
      minPrice: '',
      maxPrice: ''
    });
    setSearchParams({});
  };

  const handlePageChange = (newPage) => {
    // Cập nhật filters với pageIndex mới
    const newFilters = {
      ...filters,
      pageIndex: newPage
    };
    setFilters(newFilters);

    // Cập nhật URL params
    const params = new URLSearchParams();
    Object.entries(newFilters).forEach(([key, value]) => {
      if (value !== '' && value !== null && value !== undefined) {
        params.set(key, value);
      }
    });
    setSearchParams(params);
  };

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  const renderPagination = () => {
    const pages = [];
    const maxVisiblePages = 5;
    let startPage = Math.max(1, pagination.pageIndex - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(pagination.pageCount, startPage + maxVisiblePages - 1);

    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    return (
      <div className="flex justify-center items-center gap-2 mt-8">
        {/* First Page Button */}
        <button
          onClick={() => handlePageChange(1)}
          disabled={pagination.pageIndex === 1}
          className="px-3 py-2 border rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          title="Trang đầu"
        >
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="w-5 h-5">
            <polyline points="11 17 6 12 11 7"/>
            <polyline points="17 17 12 12 17 7"/>
          </svg>
        </button>

        {/* Previous Page Button */}
        <button
          onClick={() => handlePageChange(pagination.pageIndex - 1)}
          disabled={!pagination.hasPrevious}
          className="px-3 py-2 border rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          title="Trang trước"
        >
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="w-5 h-5">
            <polyline points="15 18 9 12 15 6"/>
          </svg>
        </button>

        {/* First Page */}
        {startPage > 1 && (
          <>
            <button
              onClick={() => handlePageChange(1)}
              className="px-3 py-2 border rounded-md hover:bg-gray-50"
            >
              1
            </button>
            {startPage > 2 && (
              <span className="px-2">...</span>
            )}
          </>
        )}

        {/* Page Numbers */}
        {Array.from({ length: endPage - startPage + 1 }, (_, i) => startPage + i).map((page) => (
          <button
            key={page}
            onClick={() => handlePageChange(page)}
            className={`px-3 py-2 border rounded-md ${
              pagination.pageIndex === page
                ? 'bg-black text-white'
                : 'hover:bg-gray-50'
            }`}
          >
            {page}
          </button>
        ))}

        {/* Last Page */}
        {endPage < pagination.pageCount && (
          <>
            {endPage < pagination.pageCount - 1 && (
              <span className="px-2">...</span>
            )}
            <button
              onClick={() => handlePageChange(pagination.pageCount)}
              className="px-3 py-2 border rounded-md hover:bg-gray-50"
            >
              {pagination.pageCount}
            </button>
          </>
        )}

        {/* Next Page Button */}
        <button
          onClick={() => handlePageChange(pagination.pageIndex + 1)}
          disabled={!pagination.hasNext}
          className="px-3 py-2 border rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          title="Trang sau"
        >
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="w-5 h-5">
            <polyline points="9 18 15 12 9 6"/>
          </svg>
        </button>

        {/* Last Page Button */}
        <button
          onClick={() => handlePageChange(pagination.pageCount)}
          disabled={pagination.pageIndex === pagination.pageCount}
          className="px-3 py-2 border rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          title="Trang cuối"
        >
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="w-5 h-5">
            <polyline points="13 17 18 12 13 7"/>
            <polyline points="6 17 11 12 6 7"/>
          </svg>
        </button>
      </div>
    );
  };

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center py-12">
          <p className="text-red-500 text-lg">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8">
      {/* Main Content */}
      <div className="relative flex">
        {/* Menu Sidebar */}
        <MenuSidebar
          isOpen={isFilterOpen}
          onClose={setIsFilterOpen}
          filters={filters}
          onFilterChange={handleFilterChange}
          categories={categories}
          brands={brands}
        />

        {/* Products Section */}
        <div className={`flex-1 min-w-0 transition-all duration-260 ease-in-out ${isFilterOpen ? 'md:ml-[260px]' : 'md:ml-[70px]'}`}>
          <div className="px-4">
            {/* Header with Active Filters */}
            <div className="mb-6">


              {/* Filter Section */}
              <FilterSection
                filters={filters}
                onFilterChange={handleFilterChange}
                priceInputs={priceInputs}
                onPriceChange={handlePriceChange}
                onClearFilters={clearFilters}
              />

              {/* Active Filters */}
              <div className="flex items-center justify-between mb-4 mt-4">
                <div className="flex items-center gap-2">
                  {(filters.categoryId || filters.brandId || filters.minPrice || filters.maxPrice || filters.sortBy !== 'newest') && (
                    <div className="flex flex-wrap gap-2">
                      {filters.categoryId && (
                        <span className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-gray-100">
                          Danh mục: {categories.find(c => c.id === filters.categoryId)?.name}
                          <button
                            onClick={() => handleFilterChange('categoryId', '')}
                            className="ml-2 text-gray-500 hover:text-gray-700"
                          >
                            ×
                          </button>
                        </span>
                      )}
                      {filters.brandId && (
                        <span className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-gray-100">
                          Thương hiệu: {brands.find(b => b.id === filters.brandId)?.name}
                          <button
                            onClick={() => handleFilterChange('brandId', '')}
                            className="ml-2 text-gray-500 hover:text-gray-700"
                          >
                            ×
                          </button>
                        </span>
                      )}
                      {filters.minPrice && (
                        <span className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-gray-100">
                          Giá từ: {formatPrice(filters.minPrice)}
                          <button
                            onClick={() => handleFilterChange('minPrice', '')}
                            className="ml-2 text-gray-500 hover:text-gray-700"
                          >
                            ×
                          </button>
                        </span>
                      )}
                      {filters.maxPrice && (
                        <span className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-gray-100">
                          Giá đến: {formatPrice(filters.maxPrice)}
                          <button
                            onClick={() => handleFilterChange('maxPrice', '')}
                            className="ml-2 text-gray-500 hover:text-gray-700"
                          >
                            ×
                          </button>
                        </span>
                      )}
                      {filters.sortBy !== 'newest' && (
                        <span className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-gray-100">
                          Sắp xếp: {
                            {
                              'price_asc': 'Giá tăng dần',
                              'price_desc': 'Giá giảm dần',
                              'name_asc': 'Tên A-Z',
                              'name_desc': 'Tên Z-A'
                            }[filters.sortBy]
                          }
                          <button
                            onClick={() => handleFilterChange('sortBy', 'newest')}
                            className="ml-2 text-gray-500 hover:text-gray-700"
                          >
                            ×
                          </button>
                        </span>
                      )}
                    </div>
                  )}
                </div>
                <span className="text-gray-500">
                  {pagination.itemCount} sản phẩm
                </span>
              </div>
            </div>

            {/* Products Grid */}
            {loading ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                {[...Array(8)].map((_, index) => (
                  <div key={index} className="animate-pulse">
                    <div className="bg-gray-200 h-64 rounded-lg mb-4"></div>
                    <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                    <div className="h-4 bg-gray-200 rounded w-1/2"></div>
                  </div>
                ))}
              </div>
            ) : (
              <>
                {products.length === 0 ? (
                  <div className="text-center py-12">
                    <p className="text-gray-500 text-lg">Không tìm thấy sản phẩm nào</p>
                  </div>
                ) : (
                  <>
                    {/* Top Pagination */}
                    {pagination.pageCount > 1 && (
                      <div className="mb-4">
                        {renderPagination()}
                      </div>
                    )}

                    <motion.div 
                      className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6"
                      variants={containerVariants}
                      initial="hidden"
                      animate="visible"
                      key={filters.pageIndex}
                    >
                      <AnimatePresence mode="wait">
                        {products.map((product, index) => (
                          <motion.div
                            key={product.id}
                            custom={index}
                            variants={itemVariants}
                            whileHover="hover"
                            whileTap={{ scale: 0.98 }}
                            layout
                            initial="hidden"
                            animate="visible"
                            exit="hidden"
                          >
                            <ProductCard
                              product={product}
                              isHovered={hoveredProduct === product.id}
                              onMouseEnter={() => setHoveredProduct(product.id)}
                              onMouseLeave={() => setHoveredProduct(null)}
                            />
                          </motion.div>
                        ))}
                      </AnimatePresence>
                    </motion.div>

                    {/* Bottom Pagination */}
                    {pagination.pageCount > 1 && (
                      <div className="mt-8">
                        {renderPagination()}
                      </div>
                    )}
                  </>
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