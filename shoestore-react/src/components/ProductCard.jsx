import { Link } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useToast } from './Toast';
import { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { ShoppingCart, Heart, Star, Zap } from 'lucide-react';

const ProductCard = ({ product, isHovered, onMouseEnter, onMouseLeave }) => {
  const { addToCart, loading: cartLoading } = useCart();
  const { addToast } = useToast();
  const [isAddingToCart, setIsAddingToCart] = useState(false);
  const [isFavorited, setIsFavorited] = useState(false);
  const [imageLoaded, setImageLoaded] = useState(false);

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  const handleQuickAddToCart = async (e) => {
    e.preventDefault();
    e.stopPropagation();

    if (product.totalQuantity === 0) {
      addToast('Sản phẩm đã hết hàng', 'warning');
      return;
    }

    const availableInventory = product.inventories?.find(inv => inv.quantity > 0);
    if (!availableInventory) {
      addToast('Sản phẩm đã hết hàng', 'warning');
      return;
    }

    setIsAddingToCart(true);
    try {
      const success = await addToCart(availableInventory.id, 1);
      if (success) {
        addToast(`Đã thêm ${product.name} (Size ${availableInventory.sizeId}) vào giỏ hàng!`, 'success');
      } else {
        addToast('Có lỗi xảy ra khi thêm vào giỏ hàng', 'error');
      }
    } catch (error) {
      console.error('Error adding to cart:', error);
      addToast('Có lỗi xảy ra khi thêm vào giỏ hàng', 'error');
    } finally {
      setIsAddingToCart(false);
    }
  };

  const handleBuyNow = async (e) => {
    e.preventDefault();
    e.stopPropagation();

    if (product.totalQuantity === 0) {
      addToast('Sản phẩm đã hết hàng', 'warning');
      return;
    }

    const availableInventory = product.inventories?.find(inv => inv.quantity > 0);
    if (!availableInventory) {
      addToast('Sản phẩm đã hết hàng', 'warning');
      return;
    }

    setIsAddingToCart(true);
    try {
      const success = await addToCart(availableInventory.id, 1);
      if (success) {
        addToast(`Đã thêm vào giỏ hàng! Chuyển đến thanh toán...`, 'success');
        // Navigate to checkout after a short delay
        setTimeout(() => {
          window.location.href = '/checkout';
        }, 1000);
      } else {
        addToast('Có lỗi xảy ra khi thêm vào giỏ hàng', 'error');
      }
    } catch (error) {
      console.error('Error adding to cart:', error);
      addToast('Có lỗi xảy ra khi thêm vào giỏ hàng', 'error');
    } finally {
      setIsAddingToCart(false);
    }
  };

  const handleFavoriteToggle = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsFavorited(!isFavorited);
    addToast(
      isFavorited ? 'Đã xóa khỏi danh sách yêu thích' : 'Đã thêm vào danh sách yêu thích',
      'success'
    );
  };

  const hasHoverImage = () => {
    if (product.mainImage && product.images && product.images.length >= 1) {
      return true;
    }
    if (!product.mainImage && product.images && product.images.length >= 2) {
      return true;
    }
    return false;
  };

  const getMainImage = () => {
    return product.mainImage || product.images?.[0] || 'https://via.placeholder.com/400';
  };

  const getHoverImage = () => {
    if (product.mainImage && product.images && product.images.length >= 1) {
      return product.images[0];
    }
    if (!product.mainImage && product.images && product.images.length >= 2) {
      return product.images[1];
    }
    return getMainImage();
  };

  const getDiscountPercentage = () => {
    if (product.hasActivePromotion && product.promotionDiscount) {
      return Math.round((product.promotionDiscount / product.price) * 100);
    }
    if (product.salePrice) {
      return Math.round((1 - product.salePrice / product.price) * 100);
    }
    return 0;
  };

  const isOnSale = product.hasActivePromotion || product.salePrice;
  const discountPercentage = getDiscountPercentage();

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
      className="group"
    >
      <div className="relative bg-white rounded-2xl overflow-hidden shadow-sm hover:shadow-2xl transition-all duration-500 transform hover:-translate-y-2">
        {/* Image Container */}
        <Link to={`/products/${product.id}`} className="block">
          <div className="relative aspect-square overflow-hidden bg-gradient-to-br from-gray-50 to-gray-100 cursor-pointer">
            {/* Loading Shimmer */}
            {!imageLoaded && (
              <div className="absolute inset-0 bg-gradient-to-r from-gray-200 via-gray-300 to-gray-200 animate-pulse" />
            )}

            {/* Main Image */}
            <motion.img
              src={getMainImage()}
              alt={product.name}
              onLoad={() => setImageLoaded(true)}
              className="object-cover w-full h-full"
              initial={{ scale: 1 }}
              whileHover={{ scale: 1.1 }}
              transition={{ duration: 0.6, ease: "easeOut" }}
            />

            {/* Hover Image */}
            {hasHoverImage() && (
              <motion.img
                src={getHoverImage()}
                alt={`${product.name} - hover`}
                className="absolute inset-0 object-cover w-full h-full"
                initial={{ opacity: 0, scale: 1.1 }}
                animate={{ 
                  opacity: isHovered ? 1 : 0,
                  scale: isHovered ? 1.1 : 1
                }}
                transition={{ duration: 0.6, ease: "easeOut" }}
              />
            )}

            {/* Gradient Overlay */}
            <div className="absolute inset-0 bg-gradient-to-t from-black/20 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

            {/* Badges */}
            <div className="absolute top-3 left-3 flex flex-col gap-2">
              {/* Sale Badge - Shortened */}
              {isOnSale && (
                <motion.div
                  initial={{ x: -50, opacity: 0 }}
                  animate={{ x: 0, opacity: 1 }}
                  className="bg-gradient-to-r from-red-500 to-pink-500 text-white px-2 py-1 rounded-full text-xs font-bold shadow-lg flex items-center gap-1"
                >
                  <Zap className="h-3 w-3" />
                  -{discountPercentage}%
                </motion.div>
              )}

              {/* Out of Stock Badge */}
              {product.totalQuantity === 0 && (
                <motion.div
                  initial={{ x: -50, opacity: 0 }}
                  animate={{ x: 0, opacity: 1 }}
                  className="bg-gray-500 text-white px-3 py-1 rounded-full text-xs font-medium shadow-lg"
                >
                  Hết hàng
                </motion.div>
              )}
            </div>

            {/* Action Buttons */}
            <div className="absolute top-3 right-3 flex flex-col gap-2">
              {/* Favorite Button */}
              <motion.button
                onClick={handleFavoriteToggle}
                className={`p-2 rounded-full shadow-lg backdrop-blur-sm border transition-all duration-300 ${
                  isFavorited 
                    ? 'bg-red-500 text-white border-red-500' 
                    : 'bg-white/80 text-gray-700 border-white/50 hover:bg-red-500 hover:text-white hover:border-red-500'
                }`}
                whileHover={{ scale: 1.1 }}
                whileTap={{ scale: 0.9 }}
                initial={{ x: 50, opacity: 0 }}
                animate={{ x: 0, opacity: 1 }}
              >
                <Heart className={`h-4 w-4 ${isFavorited ? 'fill-current' : ''}`} />
              </motion.button>

              {/* Quick Add to Cart Button */}
              <motion.button
                onClick={handleQuickAddToCart}
                disabled={product.totalQuantity === 0 || isAddingToCart}
                className={`p-2 rounded-full shadow-lg backdrop-blur-sm border transition-all duration-300 ${
                  product.totalQuantity === 0 
                    ? 'bg-gray-400/80 text-gray-600 cursor-not-allowed border-gray-400/50' 
                    : 'bg-white/80 text-gray-700 border-white/50 hover:bg-green-500 hover:text-white hover:border-green-500'
                }`}
                whileHover={{ scale: 1.1 }}
                whileTap={{ scale: 0.9 }}
                initial={{ x: 50, opacity: 0 }}
                animate={{ x: 0, opacity: 1 }}
                transition={{ delay: 0.1 }}
              >
                {isAddingToCart ? (
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-current"></div>
                ) : (
                  <ShoppingCart className="h-4 w-4" />
                )}
              </motion.button>
            </div>

            {/* Buy Now Button */}
            <AnimatePresence>
              {isHovered && (
                <motion.button
                  onClick={handleBuyNow}
                  disabled={product.totalQuantity === 0 || isAddingToCart}
                  className={`absolute bottom-4 left-4 right-4 py-3 px-4 rounded-xl text-sm font-semibold transition-all duration-300 backdrop-blur-sm border shadow-lg ${
                    product.totalQuantity === 0 
                      ? 'bg-gray-400/80 text-gray-600 cursor-not-allowed border-gray-400/50' 
                      : 'bg-gradient-to-r from-orange-500 to-red-500 hover:from-orange-600 hover:to-red-600 text-white border-white/20 hover:shadow-xl'
                  } disabled:opacity-50`}
                  initial={{ y: 50, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  exit={{ y: 50, opacity: 0 }}
                  transition={{ duration: 0.3, ease: "easeOut" }}
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                >
                  {isAddingToCart ? (
                    <div className="flex items-center justify-center gap-2">
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                      Đang xử lý...
                    </div>
                  ) : product.totalQuantity === 0 ? (
                    'Hết hàng'
                  ) : (
                    <div className="flex items-center justify-center gap-2">
                      <Zap className="h-4 w-4" />
                      Mua ngay
                    </div>
                  )}
                </motion.button>
              )}
            </AnimatePresence>
          </div>
        </Link>

        {/* Product Info */}
        <div className="p-4 space-y-2">
          {/* Product Name */}
          <Link to={`/products/${product.id}`}>
            <h3 className="font-semibold text-gray-900 line-clamp-2 hover:text-purple-600 transition-colors duration-300 leading-tight cursor-pointer">
              {product.name}
            </h3>
          </Link>

          {/* Brand/Category */}


          {/* Rating */}
          <div className="flex items-center gap-1">
            {[...Array(5)].map((_, i) => (
              <Star 
                key={i} 
                className={`h-4 w-4 ${i < 4 ? 'text-yellow-400 fill-current' : 'text-gray-300'}`} 
              />
            ))}
            <span className="text-sm text-gray-500 ml-1">(4.0)</span>
          </div>

          {/* Price */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              {product.hasActivePromotion && product.promotionPrice ? (
                <>
                  <p className="font-bold text-lg text-purple-600">{formatPrice(product.promotionPrice)}</p>
                  <p className="text-sm text-gray-500 line-through">{formatPrice(product.price)}</p>
                </>
              ) : product.salePrice ? (
                <>
                  <p className="font-bold text-lg text-red-500">{formatPrice(product.salePrice)}</p>
                  <p className="text-sm text-gray-500 line-through">{formatPrice(product.price)}</p>
                </>
              ) : (
                <p className="font-bold text-lg text-gray-900">{formatPrice(product.price)}</p>
              )}
            </div>
          </div>

          {/* Stock Status */}
          <div className={`text-sm font-medium ${
            product.totalQuantity === 0 
              ? 'text-red-500' 
              : product.totalQuantity < 10 
                ? 'text-orange-500' 
                : 'text-green-500'
          }`}>
            {product.totalQuantity === 0 
              ? 'Hết hàng' 
              : product.totalQuantity < 10 
                ? `Chỉ còn ${product.totalQuantity} sản phẩm`
                : `Còn ${product.totalQuantity} sản phẩm`
            }
          </div>
        </div>
      </div>
    </motion.div>
  );
};

export default ProductCard; 