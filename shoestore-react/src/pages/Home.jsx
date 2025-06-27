import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { productService } from '../services/productService';
import { promotionService } from '../services/promotionService';
import { categoryService } from '../services/categoryService';
import { brandService } from '../services/brandService';
import { motion, AnimatePresence } from 'framer-motion';

const Home = () => {
  const [featuredProducts, setFeaturedProducts] = useState([]);
  const [promotions, setPromotions] = useState([]);
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentPromotion, setCurrentPromotion] = useState(0);
  const [currentBrandPage, setCurrentBrandPage] = useState(0);
  const featuredScrollRef = useRef(null);

  // Brands pagination
  const brandsPerPage = 6;
  const totalBrandPages = Math.ceil(brands.length / brandsPerPage);
  const currentBrands = brands.slice(currentBrandPage * brandsPerPage, (currentBrandPage + 1) * brandsPerPage);

  useEffect(() => {
    const fetchData = async () => {
      try {
        // Use a reasonable order total to show promotions for featured products
        const estimatedOrderTotal = 1000000; // 1 million VND to show available promotions
        
        const [products, carouselPromotions, categoriesData, brandsData] = await Promise.all([
          productService.getFeaturedProducts(12, estimatedOrderTotal), // Get 12 featured products with dynamic pricing
          promotionService.getCarouselPromotions(),
          categoryService.getAll(),
          brandService.getAll()
        ]);
        
        setFeaturedProducts(products);
        setPromotions(carouselPromotions);
        setCategories(categoriesData);
        setBrands(brandsData);
      } catch (error) {
        console.error('Error fetching home data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  // Auto-scroll carousel
  useEffect(() => {
    if (promotions.length === 0) return;

    const interval = setInterval(() => {
      setCurrentPromotion((prev) => (prev + 1) % promotions.length);
    }, 5000); // Change every 5 seconds

    return () => clearInterval(interval);
  }, [promotions.length]);

  // Auto-scroll brands every 8 seconds
  useEffect(() => {
    if (totalBrandPages <= 1) return;
    
    const interval = setInterval(() => {
      setCurrentBrandPage((prev) => (prev + 1) % totalBrandPages);
    }, 8000);

    return () => clearInterval(interval);
  }, [totalBrandPages]);

  const scrollFeaturedProducts = (direction) => {
    if (featuredScrollRef.current) {
      const scrollAmount = 320; // Width of one product card + gap
      const currentScroll = featuredScrollRef.current.scrollLeft;
      const newScroll = direction === 'left' 
        ? currentScroll - scrollAmount 
        : currentScroll + scrollAmount;
      
      featuredScrollRef.current.scrollTo({
        left: newScroll,
        behavior: 'smooth'
      });
    }
  };

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  const getPromotionTypeDisplay = (type) => {
    switch (type) {
      case 'Percentage': return '%';
      case 'FixedAmount': return 'VND';
      default: return '';
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="animate-pulse">
          <div className="h-[600px] bg-gray-200"></div>
          <div className="container mx-auto px-4 py-12">
            <div className="h-8 bg-gray-200 rounded w-1/4 mb-8"></div>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
              {[...Array(4)].map((_, i) => (
                <div key={i} className="h-80 bg-gray-200 rounded-lg"></div>
              ))}
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Hero Section with Promotion Carousel */}
      <section className="relative h-[600px] overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-r from-black/70 to-black/30 z-10"></div>
        
        {/* Promotion Carousel Background */}
        {promotions.length > 0 && (
          <div className="absolute inset-0 transition-all duration-1000">
            <img
              src="https://images.unsplash.com/photo-1441986300917-64674bd600d8?ixlib=rb-4.0.3&auto=format&fit=crop&w=2070&q=80"
              alt="Hero Banner"
              className="w-full h-full object-cover"
            />
          </div>
        )}
        
        <div className="absolute inset-0 z-20 flex items-center">
          <div className="container mx-auto px-4">
            <div className="grid md:grid-cols-2 gap-8 items-center">
              {/* Left: Main Hero Content */}
              <div className="text-white">
                <h1 className="text-5xl font-bold mb-4">Khám Phá Phong Cách Mới</h1>
                <p className="text-xl mb-8">Bộ sưu tập giày mới nhất với thiết kế độc đáo và công nghệ tiên tiến</p>
              </div>
              
              {/* Right: Promotion Carousel */}
              {promotions.length > 0 && (
                <div className="relative">
                  <div className="bg-white/10 backdrop-blur-sm rounded-2xl p-6 border border-white/20">
                    <div className="text-center text-white">
                      <div className="text-sm font-medium mb-2 text-yellow-300">🎉 KHUYẾN MÃI HOT</div>
                      <h3 className="text-2xl font-bold mb-3">{promotions[currentPromotion]?.name}</h3>
                      <p className="text-lg mb-4">{promotions[currentPromotion]?.description}</p>
                      
                      <div className="flex items-center justify-center mb-4">
                        <span className="text-3xl font-bold text-yellow-300">
                          -{promotions[currentPromotion]?.discountValue}
                          {getPromotionTypeDisplay(promotions[currentPromotion]?.type)}
                        </span>
                      </div>
                      
                      {promotions[currentPromotion]?.minOrderAmount && (
                        <p className="text-sm text-gray-300 mb-4">
                          Đơn tối thiểu: {formatPrice(promotions[currentPromotion].minOrderAmount)}
                        </p>
                      )}
                      
                      <Link
                        to="/promotions"
                        className="inline-block bg-gradient-to-r from-purple-500 to-pink-500 text-white px-6 py-3 rounded-full font-semibold hover:from-purple-600 hover:to-pink-600 transition-all"
                      >
                        Áp Dụng Ngay
                      </Link>
                    </div>
                  </div>
                  
                  {/* Carousel Indicators */}
                  <div className="flex justify-center mt-4 space-x-2">
                    {promotions.map((_, index) => (
                      <button
                        key={index}
                        onClick={() => setCurrentPromotion(index)}
                        className={`w-3 h-3 rounded-full transition-all ${
                          index === currentPromotion 
                            ? 'bg-white scale-125' 
                            : 'bg-white/50 hover:bg-white/75'
                        }`}
                      />
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </section>

      {/* Categories Showcase */}
      <section className="py-20 bg-gradient-to-br from-gray-50 via-white to-purple-50 relative overflow-hidden">
        {/* Background Decorations */}
        <div className="absolute inset-0 overflow-hidden">
          <div className="absolute -top-10 -left-10 w-40 h-40 bg-purple-100 rounded-full blur-3xl opacity-60"></div>
          <div className="absolute -bottom-10 -right-10 w-60 h-60 bg-pink-100 rounded-full blur-3xl opacity-40"></div>
        </div>
        
        <div className="container mx-auto px-4 relative z-10">
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.8 }}
            viewport={{ once: true }}
            className="text-center mb-12"
          >
            <h2 className="text-4xl font-bold bg-gradient-to-r from-purple-600 to-pink-600 bg-clip-text text-transparent mb-4">
              CÁC LOẠI GIÀY
            </h2>
            <p className="text-gray-600 text-lg max-w-2xl mx-auto">
              Khám phá bộ sưu tập đa dạng với những danh mục phổ biến nhất
            </p>
          </motion.div>
          
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
            {categories.slice(0, 4).map((category, index) => (
              <motion.div
                key={category.id}
                initial={{ opacity: 0, y: 50, scale: 0.9 }}
                whileInView={{ opacity: 1, y: 0, scale: 1 }}
                transition={{ 
                  duration: 0.6, 
                  delay: index * 0.1,
                  type: "spring",
                  stiffness: 120
                }}
                viewport={{ once: true }}
                whileHover={{ scale: 1.05 }}
                className="group relative"
              >
                <Link
                  to={`/products?categoryId=${category.id}`}
                  className="block relative aspect-square overflow-hidden rounded-2xl shadow-xl group-hover:shadow-2xl transition-all duration-500"
                >
                  <div className="absolute inset-0 bg-gradient-to-br from-purple-600/20 to-pink-600/20 z-10"></div>
                  <img
                    src={`https://localhost:5001/images/categories/${category.image}`}
                    alt={category.name}
                    className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-125"
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent z-20 group-hover:from-purple-900/80 transition-all duration-500"></div>
                  
                  {/* Content */}
                  <div className="absolute bottom-0 left-0 right-0 p-6 z-30">
                    <h3 className="text-white text-xl font-bold mb-2 transform group-hover:translate-y-[-4px] transition-transform duration-300">
                      {category.name}
                    </h3>
                    <div className="flex items-center text-white/80 group-hover:text-white transition-colors">
                      <span className="text-sm">Khám phá ngay</span>
                      <svg className="w-4 h-4 ml-2 transform group-hover:translate-x-2 transition-transform duration-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
                      </svg>
                    </div>
                  </div>
                  
                  {/* Hover Glow Effect */}
                  <div className="absolute inset-0 bg-gradient-to-r from-purple-400/0 via-purple-400/30 to-purple-400/0 z-20 opacity-0 group-hover:opacity-100 transition-opacity duration-500 transform rotate-45 translate-x-full group-hover:translate-x-[-100%] transition-transform duration-1000"></div>
                </Link>
              </motion.div>
            ))}
          </div>
          
          {/* View All Categories CTA */}
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0.4 }}
            viewport={{ once: true }}
            className="text-center mt-12"
          >
            <Link
              to="/products"
              className="inline-flex items-center px-8 py-4 bg-gradient-to-r from-purple-600 to-pink-600 text-white font-semibold rounded-full hover:from-purple-700 hover:to-pink-700 transform hover:scale-105 transition-all duration-300 shadow-lg hover:shadow-xl"
            >
              Xem Tất Cả Danh Mục
              <svg className="w-5 h-5 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
              </svg>
            </Link>
          </motion.div>
        </div>
      </section>

      {/* Featured Products - Horizontal Scrollable List */}
      <section className="py-16 bg-gray-50">
        <div className="container mx-auto px-4">
          <div className="flex justify-between items-center mb-8">
            <h2 className="text-3xl font-bold">Sản Phẩm Nổi Bật</h2>
            <div className="flex gap-2">
              <button
                onClick={() => scrollFeaturedProducts('left')}
                className="p-2 rounded-full bg-white shadow-md hover:shadow-lg transition-shadow"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <button
                onClick={() => scrollFeaturedProducts('right')}
                className="p-2 rounded-full bg-white shadow-md hover:shadow-lg transition-shadow"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                </svg>
              </button>
            </div>
          </div>
          
          <div 
            ref={featuredScrollRef}
            className="flex gap-6 overflow-x-auto scroll-smooth scrollbar-hide pb-4"
            style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}
          >
            {featuredProducts.map((product) => (
              <Link
                key={product.id}
                to={`/products/${product.id}`}
                className="group bg-white rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-shadow flex-shrink-0 w-80"
              >
                <div className="relative aspect-square overflow-hidden">
                  <img
                    src={product.mainImage}
                    alt={product.name}
                    className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
                  />
                  {/* Promotion Badge */}
                  {product.hasActivePromotion && product.promotionDiscount && (
                    <div className="absolute top-3 right-3 bg-gradient-to-r from-purple-500 to-pink-500 text-white px-3 py-1 rounded-full text-sm font-bold shadow-lg">
                      -{Math.round((product.promotionDiscount / product.price) * 100)}%
                    </div>
                  )}
                  {/* Sale Badge (fallback if no promotion) */}
                  {!product.hasActivePromotion && product.salePrice && (
                    <div className="absolute top-3 right-3 bg-red-500 text-white px-3 py-1 rounded-full text-sm shadow-lg">
                      -{Math.round((1 - product.salePrice / product.price) * 100)}%
                    </div>
                  )}
                  {/* Like Count Badge */}
                  <div className="absolute top-3 left-3 bg-black/50 text-white px-2 py-1 rounded-full text-xs flex items-center gap-1">
                    <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M3.172 5.172a4 4 0 015.656 0L10 6.343l1.172-1.171a4 4 0 115.656 5.656L10 17.657l-6.828-6.829a4 4 0 010-5.656z" clipRule="evenodd" />
                    </svg>
                    {product.likeCount}
                  </div>
                </div>
                <div className="p-6">
                  <h3 className="font-semibold mb-3 line-clamp-2 text-lg">{product.name}</h3>
                  
                  {/* Promotion Name */}
                  {product.hasActivePromotion && product.promotionName && (
                    <div className="text-xs text-purple-600 font-medium mb-2 bg-purple-50 px-2 py-1 rounded">
                      🎉 {product.promotionName}
                    </div>
                  )}
                  
                  <div className="flex items-center gap-2">
                    {product.hasActivePromotion && product.promotionPrice ? (
                      <>
                        <span className="text-purple-600 font-bold text-lg">
                          {formatPrice(product.promotionPrice)}
                        </span>
                        <span className="text-gray-500 line-through text-sm">
                          {formatPrice(product.price)}
                        </span>
                      </>
                    ) : product.salePrice ? (
                      <>
                        <span className="text-red-500 font-bold text-lg">
                          {formatPrice(product.salePrice)}
                        </span>
                        <span className="text-gray-500 line-through text-sm">
                          {formatPrice(product.price)}
                        </span>
                      </>
                    ) : (
                      <span className="font-bold text-lg">{formatPrice(product.price)}</span>
                    )}
                  </div>
                  
                  {/* Category & Brand */}
                  <div className="text-xs text-gray-500 mt-2">
                    {product.categoryName} • {product.brandName}
                  </div>
                </div>
              </Link>
            ))}
            
            {/* View All Card */}
            <Link
              to="/products"
              className="group bg-gradient-to-br from-gray-100 to-gray-200 rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-all flex-shrink-0 w-80 flex items-center justify-center"
            >
              <div className="text-center p-8">
                <div className="w-16 h-16 mx-auto mb-4 bg-gray-300 rounded-full flex items-center justify-center group-hover:bg-gray-400 transition-colors">
                  <svg className="w-8 h-8 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14 5l7 7m0 0l-7 7m7-7H3" />
                  </svg>
                </div>
                <h3 className="font-semibold text-lg mb-2">Xem Tất Cả</h3>
                <p className="text-gray-600 text-sm">Khám phá toàn bộ bộ sưu tập</p>
              </div>
            </Link>
          </div>
        </div>
      </section>

      {/* Brands Showcase */}
      <section className="py-20 bg-white relative overflow-hidden">
        {/* Background Pattern */}
        <div className="absolute inset-0 opacity-5">
          <div className="absolute inset-0" style={{
            backgroundImage: `url("data:image/svg+xml,%3Csvg width='40' height='40' viewBox='0 0 40 40' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='%23000' fill-opacity='1' fill-rule='evenodd'%3E%3Ccircle cx='20' cy='20' r='1'/%3E%3C/g%3E%3C/svg%3E")`,
            backgroundSize: '40px 40px'
          }}></div>
        </div>
        
        <div className="container mx-auto px-4 relative z-10">
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.8 }}
            viewport={{ once: true }}
            className="text-center mb-12"
          >
            <h2 className="text-4xl font-bold text-gray-900 mb-4">Thương Hiệu Nổi Bật</h2>
            <p className="text-gray-600 text-lg max-w-2xl mx-auto">
              Hợp tác cùng những thương hiệu uy tín và chất lượng hàng đầu
            </p>
          </motion.div>

          {/* Brands Carousel */}
          <div className="relative">
            <AnimatePresence mode="wait">
              <motion.div
                key={currentBrandPage}
                initial={{ opacity: 0, x: 100 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -100 }}
                transition={{ duration: 0.5 }}
                className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-8"
              >
                {currentBrands.map((brand, index) => (
                  <motion.div
                    key={brand.id}
                    initial={{ opacity: 0, y: 30, scale: 0.8 }}
                    animate={{ opacity: 1, y: 0, scale: 1 }}
                    transition={{ 
                      duration: 0.5, 
                      delay: index * 0.1,
                      type: "spring",
                      stiffness: 120
                    }}
                    whileHover={{ 
                      scale: 1.1, 
                      rotateY: 5,
                      z: 50
                    }}
                    className="group relative"
                  >
                    <Link
                      to={`/products?brandId=${brand.id}`}
                      className="block relative"
                    >
                      {/* Card Background */}
                      <div className="relative bg-gradient-to-br from-white to-gray-50 rounded-2xl p-8 shadow-lg group-hover:shadow-2xl transition-all duration-500 border border-gray-100 group-hover:border-purple-200">
                        {/* Glow Effect */}
                        <div className="absolute inset-0 bg-gradient-to-r from-purple-600/0 via-purple-600/5 to-purple-600/0 rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-500"></div>
                        
                        {/* Brand Logo */}
                        <div className="relative z-10 flex items-center justify-center h-20">
                          <img
                            src={`https://localhost:5001/images/brands/${brand.logo}`}
                            alt={brand.name}
                            className="max-h-12 max-w-full object-contain filter brightness-75 contrast-125 group-hover:brightness-100 group-hover:contrast-100 transition-all duration-500"
                          />
                        </div>
                        
                        {/* Brand Name */}
                        <div className="mt-4 text-center">
                          <h3 className="text-sm font-semibold text-gray-700 group-hover:text-purple-600 transition-colors duration-300">
                            {brand.name}
                          </h3>
                        </div>
                        
                        {/* Hover Indicator */}
                        <div className="absolute bottom-2 left-1/2 transform -translate-x-1/2 w-0 h-1 bg-gradient-to-r from-purple-600 to-pink-600 rounded-full group-hover:w-8 transition-all duration-300"></div>
                      </div>
                    </Link>
                  </motion.div>
                ))}
              </motion.div>
            </AnimatePresence>

            {/* Navigation Arrows */}
            {totalBrandPages > 1 && (
              <>
                <button
                  onClick={() => setCurrentBrandPage(currentBrandPage === 0 ? totalBrandPages - 1 : currentBrandPage - 1)}
                  className="absolute left-0 top-1/2 transform -translate-y-1/2 -translate-x-4 w-12 h-12 bg-white rounded-full shadow-lg hover:shadow-xl flex items-center justify-center text-gray-600 hover:text-purple-600 transition-all duration-300 group disabled:opacity-50"
                  disabled={brands.length <= brandsPerPage}
                >
                  <svg className="w-5 h-5 transform group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                  </svg>
                </button>
                <button
                  onClick={() => setCurrentBrandPage(currentBrandPage === totalBrandPages - 1 ? 0 : currentBrandPage + 1)}
                  className="absolute right-0 top-1/2 transform -translate-y-1/2 translate-x-4 w-12 h-12 bg-white rounded-full shadow-lg hover:shadow-xl flex items-center justify-center text-gray-600 hover:text-purple-600 transition-all duration-300 group disabled:opacity-50"
                  disabled={brands.length <= brandsPerPage}
                >
                  <svg className="w-5 h-5 transform group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                  </svg>
                </button>
              </>
            )}
          </div>

          {/* Dots Indicator */}
          {totalBrandPages > 1 && (
            <div className="flex justify-center mt-8 space-x-2">
              {Array.from({ length: totalBrandPages }, (_, index) => (
                <button
                  key={index}
                  onClick={() => setCurrentBrandPage(index)}
                  className={`w-3 h-3 rounded-full transition-all duration-300 ${
                    currentBrandPage === index 
                      ? 'bg-gradient-to-r from-purple-600 to-pink-600 scale-125' 
                      : 'bg-gray-300 hover:bg-gray-400'
                  }`}
                />
              ))}
            </div>
          )}

          {/* Auto-rotation indicator */}
          {totalBrandPages > 1 && (
            <div className="text-center mt-6">
              <div className="text-xs text-gray-500 flex items-center justify-center gap-2">
                <div className="w-2 h-2 bg-purple-600 rounded-full animate-pulse"></div>
                Tự động chuyển đổi
              </div>
            </div>
          )}
        </div>
      </section>

      {/* Promotional Banner */}
      <section className="py-16 bg-gray-900 text-white">
        <div className="container mx-auto px-4">
          <div className="grid md:grid-cols-2 gap-8 items-center">
            <div>
              <h2 className="text-4xl font-bold mb-4">Ưu Đãi Đặc Biệt</h2>
              <p className="text-xl mb-6">Giảm giá lên đến 50% cho các sản phẩm được chọn</p>
              <Link
                to="/products?sale=true"
                className="inline-block bg-white text-black px-8 py-3 rounded-full font-semibold hover:bg-gray-100 transition-colors"
              >
                Xem Ngay
              </Link>
            </div>
            <div className="relative aspect-square rounded-lg overflow-hidden">
              <img
                src="https://localhost:5001/images/banners/promo-banner.jpg"
                alt="Promotional Banner"
                className="w-full h-full object-cover"
              />
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-16 bg-white">
        <div className="container mx-auto px-4">
          <div className="grid md:grid-cols-4 gap-8">
            <div className="text-center">
              <div className="w-16 h-16 mx-auto mb-4 bg-gray-100 rounded-full flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4" />
                </svg>
              </div>
              <h3 className="font-semibold mb-2">Miễn Phí Vận Chuyển</h3>
              <p className="text-gray-600">Cho đơn hàng từ 500K</p>
            </div>
            <div className="text-center">
              <div className="w-16 h-16 mx-auto mb-4 bg-gray-100 rounded-full flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <h3 className="font-semibold mb-2">Giao Hàng Nhanh</h3>
              <p className="text-gray-600">Trong vòng 24 giờ</p>
            </div>
            <div className="text-center">
              <div className="w-16 h-16 mx-auto mb-4 bg-gray-100 rounded-full flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                </svg>
              </div>
              <h3 className="font-semibold mb-2">Bảo Hành Chính Hãng</h3>
              <p className="text-gray-600">Lên đến 12 tháng</p>
            </div>
            <div className="text-center">
              <div className="w-16 h-16 mx-auto mb-4 bg-gray-100 rounded-full flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                </svg>
              </div>
              <h3 className="font-semibold mb-2">Thanh Toán An Toàn</h3>
              <p className="text-gray-600">Nhiều phương thức thanh toán</p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default Home;
