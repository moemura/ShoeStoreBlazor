import { useEffect, useState } from "react";
import { ProductCard } from "../components/product-card";
import { Button } from "../components/ui/button";
import { Link } from "react-router-dom";

export function HomePage() {
  const [featuredProducts, setFeaturedProducts] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const response = await fetch("https://localhost:5001/api/products");
        const data = await response.json();
        setFeaturedProducts(data.slice(0, 8)); // Get first 8 products
      } catch (error) {
        console.error("Error fetching products:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, []);

  return (
    <div className="space-y-12">
      {/* Hero Section */}
      <section className="relative">
        <div className="absolute inset-0">
          <img
            src="/hero-bg.jpg"
            alt="Hero background"
            className="h-full w-full object-cover"
          />
          <div className="absolute inset-0 bg-black/50" />
        </div>
        <div className="relative container mx-auto px-4 py-32 text-center">
          <h1 className="text-4xl font-bold tracking-tight text-white sm:text-6xl">
            Khám phá bộ sưu tập giày mới nhất
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-300">
            Tìm kiếm phong cách của riêng bạn với những mẫu giày độc đáo và thời trang
          </p>
          <div className="mt-10 flex items-center justify-center gap-x-6">
            <Button asChild size="lg">
              <Link to="/products">Mua sắm ngay</Link>
            </Button>
            <Button variant="outline" size="lg" className="text-white border-white hover:bg-white/10">
              Tìm hiểu thêm
            </Button>
          </div>
        </div>
      </section>

      {/* Featured Products */}
      <section className="container mx-auto px-4">
        <div className="flex items-center justify-between mb-8">
          <h2 className="text-2xl font-bold">Sản phẩm nổi bật</h2>
          <Button variant="ghost" asChild>
            <Link to="/products">Xem tất cả</Link>
          </Button>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {[...Array(8)].map((_, i) => (
              <div key={i} className="animate-pulse">
                <div className="aspect-square bg-gray-200 rounded-lg" />
                <div className="mt-4 space-y-2">
                  <div className="h-4 bg-gray-200 rounded w-3/4" />
                  <div className="h-4 bg-gray-200 rounded w-1/2" />
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {featuredProducts.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        )}
      </section>

      {/* Categories Section */}
      <section className="bg-gray-50">
        <div className="container mx-auto px-4 py-16">
          <h2 className="text-2xl font-bold mb-8">Danh mục sản phẩm</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="relative aspect-[4/3] overflow-hidden rounded-lg">
              <img
                src="/category-sport.jpg"
                alt="Sport"
                className="h-full w-full object-cover"
              />
              <div className="absolute inset-0 bg-black/40" />
              <div className="absolute inset-0 flex items-center justify-center">
                <h3 className="text-2xl font-bold text-white">Giày thể thao</h3>
              </div>
            </div>
            <div className="relative aspect-[4/3] overflow-hidden rounded-lg">
              <img
                src="/category-casual.jpg"
                alt="Casual"
                className="h-full w-full object-cover"
              />
              <div className="absolute inset-0 bg-black/40" />
              <div className="absolute inset-0 flex items-center justify-center">
                <h3 className="text-2xl font-bold text-white">Giày casual</h3>
              </div>
            </div>
            <div className="relative aspect-[4/3] overflow-hidden rounded-lg">
              <img
                src="/category-formal.jpg"
                alt="Formal"
                className="h-full w-full object-cover"
              />
              <div className="absolute inset-0 bg-black/40" />
              <div className="absolute inset-0 flex items-center justify-center">
                <h3 className="text-2xl font-bold text-white">Giày công sở</h3>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
} 