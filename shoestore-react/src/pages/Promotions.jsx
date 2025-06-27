import { useState, useEffect } from 'react';
import { productService } from '../services/productService';

const Promotions = () => {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchPromotions = async () => {
      try {
        // Use a reasonable order total to show all available promotions
        const estimatedOrderTotal = 1000000; // 1 million VND to show available promotions
        
        const response = await productService.getAll(1, 12, {}, estimatedOrderTotal);
        
        // Filter products that have active promotions
        const promotionProducts = (response.data || response || []).filter(product => 
          product.hasActivePromotion || product.salePrice
        );
        
        setProducts(promotionProducts);
      } catch (error) {
        console.error('Error fetching promotions:', error);
        setError('Không thể tải danh sách khuyến mãi');
      } finally {
        setLoading(false);
      }
    };

    fetchPromotions();
  }, []);

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded w-1/4 mb-8"></div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {[...Array(6)].map((_, index) => (
              <div key={index}>
                <div className="bg-gray-200 h-64 rounded-lg mb-4"></div>
                <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                <div className="h-4 bg-gray-200 rounded w-1/2"></div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

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
      <h1 className="text-3xl font-bold mb-8">Khuyến mãi</h1>
      
      {products.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">Hiện không có sản phẩm khuyến mãi</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {products.map((product) => (
            <div key={product.id} className="group">
              <div className="relative aspect-square overflow-hidden rounded-lg bg-gray-100 mb-4">
                <img
                  src={product.mainImage || product.images?.[0] || 'https://via.placeholder.com/400'}
                  alt={product.name}
                  className="object-cover w-full h-full group-hover:scale-105 transition-transform duration-300"
                />
                {/* Promotion Badge */}
                {product.hasActivePromotion && product.promotionDiscount && (
                  <div className="absolute top-2 right-2 bg-gradient-to-r from-purple-500 to-pink-500 text-white px-2 py-1 rounded-md text-xs font-bold">
                    {Math.round((product.promotionDiscount / product.price) * 100)}% OFF
                  </div>
                )}
                {/* Sale Badge (fallback if no promotion) */}
                {!product.hasActivePromotion && product.salePrice && (
                  <div className="absolute top-2 right-2 bg-red-500 text-white px-2 py-1 rounded-md">
                    {Math.round((1 - product.salePrice / product.price) * 100)}% OFF
                  </div>
                )}
              </div>
              <h3 className="font-medium text-gray-900 mb-1">{product.name}</h3>
              <p className="text-sm text-gray-500 mb-2">{product.brandName}</p>
              
              {/* Promotion Name */}
              {product.hasActivePromotion && product.promotionName && (
                <div className="text-xs text-purple-600 font-medium mb-1 bg-purple-50 px-2 py-1 rounded">
                  🎉 {product.promotionName}
                </div>
              )}
              
              <div className="flex items-center gap-2">
                {product.hasActivePromotion && product.promotionPrice ? (
                  <>
                    <p className="font-medium text-purple-600">{formatPrice(product.promotionPrice)}</p>
                    <p className="text-sm text-gray-500 line-through">{formatPrice(product.price)}</p>
                  </>
                ) : product.salePrice ? (
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
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Promotions; 