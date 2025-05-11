import { useState, useEffect, useCallback, useRef } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { productService } from '../services/productService';
import { categoryService } from '../services/categoryService';
import { brandService } from '../services/brandService';
import { debounce } from 'lodash';

const Products = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
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
        // Chuyển đổi giá trị giá thành số và thêm vào params
        if (filters.minPrice !== '') {
          filterParams.minPrice = Number(filters.minPrice);
        }
        if (filters.maxPrice !== '') {
          filterParams.maxPrice = Number(filters.maxPrice);
        }
        if (filters.search) filterParams.search = filters.search;
        if (filters.sortBy) filterParams.sortBy = filters.sortBy;

        console.log('Filter params:', filterParams); // Debug log

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
    setSearchParams({});
  };

  const handlePageChange = (newPage) => {
    handleFilterChange('pageIndex', newPage);
  };

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
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
    <div className="container mx-auto px-4 py-8">
      <div className="flex flex-col md:flex-row gap-8">
        {/* Sidebar Filters */}
        <div className="w-full md:w-72 space-y-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold">Bộ lọc</h2>
            <button
              onClick={clearFilters}
              className="text-sm text-gray-500 hover:text-gray-700"
            >
              Xóa bộ lọc
            </button>
          </div>

          {/* Categories Filter */}
          <div className="bg-white rounded-lg shadow-sm p-4">
            <h3 className="font-semibold text-lg mb-3">Danh mục</h3>
            <div className="space-y-2 max-h-48 overflow-y-auto">
              {categories.map((category) => (
                <label key={category.id} className="flex items-center space-x-2 hover:bg-gray-50 p-2 rounded-md cursor-pointer">
                  <input
                    type="radio"
                    name="category"
                    checked={filters.categoryId === category.id}
                    onChange={() => handleFilterChange('categoryId', category.id)}
                    className="rounded border-gray-300"
                  />
                  <span className="flex-1">{category.name}</span>
                </label>
              ))}
            </div>
          </div>

          {/* Brands Filter */}
          <div className="bg-white rounded-lg shadow-sm p-4">
            <h3 className="font-semibold text-lg mb-3">Thương hiệu</h3>
            <div className="space-y-2 max-h-48 overflow-y-auto">
              {brands.map((brand) => (
                <label key={brand.id} className="flex items-center space-x-2 hover:bg-gray-50 p-2 rounded-md cursor-pointer">
                  <input
                    type="radio"
                    name="brand"
                    checked={filters.brandId === brand.id}
                    onChange={() => handleFilterChange('brandId', brand.id)}
                    className="rounded border-gray-300"
                  />
                  <span className="flex-1">{brand.name}</span>
                </label>
              ))}
            </div>
          </div>

          {/* Price Range Filter */}
          <div className="bg-white rounded-lg shadow-sm p-4">
            <h3 className="font-semibold text-lg mb-3">Khoảng giá</h3>
            <div className="space-y-4">
              <div>
                <label className="block text-sm text-gray-600 mb-1">Từ</label>
                <div className="relative">
                  <input
                    type="number"
                    value={priceInputs.minPrice}
                    onChange={(e) => handlePriceChange('min', e.target.value)}
                    placeholder="0"
                    className="w-full px-3 py-2 border rounded-md pl-8"
                  />
                  <span className="absolute left-3 top-2 text-gray-500">₫</span>
                </div>
              </div>
              <div>
                <label className="block text-sm text-gray-600 mb-1">Đến</label>
                <div className="relative">
                  <input
                    type="number"
                    value={priceInputs.maxPrice}
                    onChange={(e) => handlePriceChange('max', e.target.value)}
                    placeholder="1000000"
                    className="w-full px-3 py-2 border rounded-md pl-8"
                  />
                  <span className="absolute left-3 top-2 text-gray-500">₫</span>
                </div>
              </div>
            </div>
          </div>

          {/* Sort Filter */}
          <div className="bg-white rounded-lg shadow-sm p-4">
            <h3 className="font-semibold text-lg mb-3">Sắp xếp</h3>
            <select
              value={filters.sortBy}
              onChange={(e) => handleFilterChange('sortBy', e.target.value)}
              className="w-full px-3 py-2 border rounded-md bg-white"
            >
              <option value="newest">Mới nhất</option>
              <option value="price_asc">Giá tăng dần</option>
              <option value="price_desc">Giá giảm dần</option>
              <option value="name_asc">Tên A-Z</option>
              <option value="name_desc">Tên Z-A</option>
            </select>
          </div>
        </div>

        {/* Products Grid */}
        <div className="flex-1">
          {/* Active Filters */}
          {(filters.categoryId || filters.brandId || filters.minPrice || filters.maxPrice || filters.sortBy !== 'newest') && (
            <div className="mb-6 flex flex-wrap gap-2">
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

          {loading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {[...Array(6)].map((_, index) => (
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
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                  {products.map((product) => (
                    <Link
                      key={product.id}
                      to={`/products/${product.id}`}
                      className="group block"
                    >
                      <div className="relative aspect-square overflow-hidden rounded-lg bg-gray-100 mb-4">
                        <img
                          src={product.mainImage || product.images?.[0] || 'https://via.placeholder.com/400'}
                          alt={product.name}
                          className="object-cover w-full h-full group-hover:scale-105 transition-transform duration-300"
                        />
                        {product.salePrice && (
                          <div className="absolute top-2 right-2 bg-red-500 text-white px-2 py-1 rounded-md">
                            {Math.round((1 - product.salePrice / product.price) * 100)}% OFF
                          </div>
                        )}
                      </div>
                      <h3 className="font-medium text-gray-900 mb-1">{product.name}</h3>
                      <p className="text-sm text-gray-500 mb-2">{product.brandName}</p>
                      <div className="flex items-center gap-2">
                        {product.salePrice ? (
                          <>
                            <p className="font-medium text-red-500">{formatPrice(product.salePrice)}</p>
                            <p className="text-sm text-gray-500 line-through">{formatPrice(product.price)}</p>
                          </>
                        ) : (
                          <p className="font-medium text-gray-900">{formatPrice(product.price)}</p>
                        )}
                      </div>
                      <div className="mt-2 text-sm text-gray-500">
                        Còn lại: {product.totalQuantity} sản phẩm
                      </div>
                    </Link>
                  ))}
                </div>
              )}

              {/* Pagination */}
              {pagination.pageCount > 1 && (
                <div className="flex justify-center mt-8 space-x-2">
                  <button
                    onClick={() => handlePageChange(pagination.pageIndex - 1)}
                    disabled={!pagination.hasPrevious}
                    className="px-4 py-2 border rounded-md disabled:opacity-50"
                  >
                    Trước
                  </button>
                  {[...Array(pagination.pageCount)].map((_, index) => (
                    <button
                      key={index}
                      onClick={() => handlePageChange(index + 1)}
                      className={`px-4 py-2 border rounded-md ${
                        pagination.pageIndex === index + 1
                          ? 'bg-black text-white'
                          : 'hover:bg-gray-100'
                      }`}
                    >
                      {index + 1}
                    </button>
                  ))}
                  <button
                    onClick={() => handlePageChange(pagination.pageIndex + 1)}
                    disabled={!pagination.hasNext}
                    className="px-4 py-2 border rounded-md disabled:opacity-50"
                  >
                    Sau
                  </button>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default Products; 