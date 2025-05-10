import { useEffect, useState, useCallback } from "react";
import { ProductCard } from "../components/product-card";
import { Button } from "../components/ui/button";
import { Sidebar } from "../components/ui/sidebar";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../components/ui/select";
import { Input } from "../components/ui/input";
import { Slider } from "../components/ui/slider";

export function ProductsPage() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [filters, setFilters] = useState({
    categoryId: "all",
    brandId: "all",
    minPrice: 0,
    maxPrice: 10000000,
    search: "",
  });
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 12;

  const fetchProducts = useCallback(async () => {
    try {
      setLoading(true);
      const queryParams = new URLSearchParams({
        pageIndex: page,
        pageSize,
        ...(filters.categoryId !== "all" && { categoryId: filters.categoryId }),
        ...(filters.brandId !== "all" && { brandId: filters.brandId }),
        minPrice: filters.minPrice,
        maxPrice: filters.maxPrice,
        search: filters.search,
      });

      const productsResponse = await fetch(`https://localhost:5001/api/products/filter?${queryParams}`);
      if (!productsResponse.ok) {
        throw new Error('Failed to fetch products');
      }
      const productsData = await productsResponse.json();
      setProducts(productsData.data || []);
      setTotalPages(productsData.pageCount || 1);
    } catch (error) {
      console.error("Error fetching products:", error);
      setError(error.message);
      setProducts([]);
    } finally {
      setLoading(false);
    }
  }, [page, filters, pageSize]);

  const fetchCategoriesAndBrands = useCallback(async () => {
    try {
      // Fetch categories
      const categoriesResponse = await fetch("https://localhost:5001/api/categories");
      if (!categoriesResponse.ok) {
        throw new Error('Failed to fetch categories');
      }
      const categoriesData = await categoriesResponse.json();
      setCategories(categoriesData);

      // Fetch brands
      const brandsResponse = await fetch("https://localhost:5001/api/brands");
      if (!brandsResponse.ok) {
        throw new Error('Failed to fetch brands');
      }
      const brandsData = await brandsResponse.json();
      setBrands(brandsData);
    } catch (error) {
      console.error("Error fetching categories and brands:", error);
      setError(error.message);
      setCategories([]);
      setBrands([]);
    }
  }, []);

  useEffect(() => {
    fetchCategoriesAndBrands();
  }, [fetchCategoriesAndBrands]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const handleFilterChange = (key, value) => {
    setFilters(prev => ({ ...prev, [key]: value }));
    setPage(1); // Reset to first page when filters change
  };

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">
          <h2 className="text-2xl font-bold text-red-600 mb-4">Đã xảy ra lỗi</h2>
          <p className="text-gray-600">{error}</p>
          <Button
            className="mt-4"
            onClick={() => {
              setError(null);
              fetchCategoriesAndBrands();
              fetchProducts();
            }}
          >
            Thử lại
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
        {/* Sidebar */}
        <div className="lg:col-span-1">
          <Sidebar categories={categories} brands={brands} />
        </div>

        {/* Main Content */}
        <div className="lg:col-span-3">
          {/* Filters */}
          <div className="mb-8 space-y-4">
            <div>
              <h3 className="text-lg font-semibold mb-4">Tìm kiếm</h3>
              <Input
                placeholder="Tên sản phẩm..."
                value={filters.search}
                onChange={(e) => handleFilterChange("search", e.target.value)}
              />
            </div>

            <div>
              <h3 className="text-lg font-semibold mb-4">Danh mục</h3>
              <Select
                value={filters.categoryId}
                onValueChange={(value) => handleFilterChange("categoryId", value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Chọn danh mục" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả</SelectItem>
                  {categories.map((category) => (
                    <SelectItem key={category.id} value={category.id.toString()}>
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <h3 className="text-lg font-semibold mb-4">Thương hiệu</h3>
              <Select
                value={filters.brandId}
                onValueChange={(value) => handleFilterChange("brandId", value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Chọn thương hiệu" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả</SelectItem>
                  {brands.map((brand) => (
                    <SelectItem key={brand.id} value={brand.id.toString()}>
                      {brand.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <h3 className="text-lg font-semibold mb-4">Khoảng giá</h3>
              <div className="space-y-4">
                <Slider
                  defaultValue={[filters.minPrice, filters.maxPrice]}
                  max={10000000}
                  step={100000}
                  onValueChange={([min, max]) => {
                    handleFilterChange("minPrice", min);
                    handleFilterChange("maxPrice", max);
                  }}
                />
                <div className="flex justify-between text-sm text-gray-500">
                  <span>{filters.minPrice.toLocaleString()}đ</span>
                  <span>{filters.maxPrice.toLocaleString()}đ</span>
                </div>
              </div>
            </div>
          </div>

          {/* Products Grid */}
          {loading ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
              {[...Array(6)].map((_, i) => (
                <div key={i} className="animate-pulse">
                  <div className="aspect-square bg-gray-200 rounded-lg" />
                  <div className="mt-4 space-y-2">
                    <div className="h-4 bg-gray-200 rounded w-3/4" />
                    <div className="h-4 bg-gray-200 rounded w-1/2" />
                  </div>
                </div>
              ))}
            </div>
          ) : products.length === 0 ? (
            <div className="text-center py-12">
              <h3 className="text-lg font-semibold text-gray-900">Không tìm thấy sản phẩm</h3>
              <p className="mt-2 text-gray-500">Vui lòng thử lại với bộ lọc khác</p>
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                {products.map((product) => (
                  <ProductCard key={product.id} product={product} />
                ))}
              </div>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="mt-8 flex justify-center space-x-2">
                  <Button
                    variant="outline"
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    disabled={page === 1}
                  >
                    Trước
                  </Button>
                  {[...Array(totalPages)].map((_, i) => (
                    <Button
                      key={i + 1}
                      variant={page === i + 1 ? "default" : "outline"}
                      onClick={() => setPage(i + 1)}
                    >
                      {i + 1}
                    </Button>
                  ))}
                  <Button
                    variant="outline"
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    disabled={page === totalPages}
                  >
                    Sau
                  </Button>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
} 