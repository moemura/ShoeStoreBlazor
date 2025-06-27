import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  ShoppingCart, 
  Plus, 
  Minus, 
  X, 
  Trash2, 
  Heart,
  Gift,
  Tag,
  Truck,
  Shield,
  ArrowRight,
  ShoppingBag,
  Percent
} from 'lucide-react';
import { useCart } from '../context/CartContext';
import { useToast } from '../components/Toast';

const Cart = () => {
  const { cart, loading, error, updateQuantity, removeFromCart, clearCart } = useCart();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const [removingItem, setRemovingItem] = useState(null);

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  const cartItems = cart?.items || [];
  const subtotal = cartItems.reduce(
    (total, item) => {
      const effectivePrice = item.promotionPrice || item.salePrice || item.price || 0;
      return total + effectivePrice * item.quantity;
    },
    0
  );

  const totalSavings = cartItems.reduce(
    (total, item) => {
      if (item.hasActivePromotion && item.promotionPrice) {
        return total + (item.price - item.promotionPrice) * item.quantity;
      } else if (item.salePrice) {
        return total + (item.price - item.salePrice) * item.quantity;
      }
      return total;
    },
    0
  );

  const handleQuantityChange = async (inventoryId, newQuantity) => {
    if (newQuantity < 1) return;
    
    const success = await updateQuantity(inventoryId, newQuantity);
    if (!success) {
      addToast('Có lỗi xảy ra khi cập nhật số lượng', 'error');
    }
  };

  const handleRemoveItem = async (inventoryId, productName) => {
    setRemovingItem(inventoryId);
    const success = await removeFromCart(inventoryId);
    if (success) {
      addToast(`Đã xóa ${productName} khỏi giỏ hàng`, 'success');
    } else {
      addToast('Có lỗi xảy ra khi xóa sản phẩm', 'error');
    }
    setRemovingItem(null);
  };

  const handleClearCart = async () => {
    if (cartItems.length === 0) return;
    
    if (window.confirm('Bạn có chắc muốn xóa toàn bộ giỏ hàng?')) {
      const success = await clearCart();
      if (success) {
        addToast('Đã xóa toàn bộ giỏ hàng', 'success');
      } else {
        addToast('Có lỗi xảy ra khi xóa giỏ hàng', 'error');
      }
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="container mx-auto px-4 py-8">
          <motion.div 
            className="flex justify-center items-center py-12"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
          >
            <div className="flex flex-col items-center gap-4">
              <div className="animate-spin rounded-full h-12 w-12 border-4 border-black border-t-transparent"></div>
              <p className="text-gray-600">Đang tải giỏ hàng...</p>
            </div>
          </motion.div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <motion.div 
          className="flex items-center justify-between mb-8"
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
        >
          <div className="flex items-center gap-3">
                          <div className="p-3 bg-gradient-to-r from-blue-600 to-purple-600 rounded-xl">
              <ShoppingCart className="h-6 w-6 text-white" />
            </div>
            <div>
              <h1 className="text-3xl font-bold text-gray-900">Giỏ hàng</h1>
              <p className="text-gray-600">
                {cartItems.length} sản phẩm trong giỏ hàng
              </p>
            </div>
          </div>
          
          {cartItems.length > 0 && (
            <motion.button
              onClick={handleClearCart}
              className="flex items-center gap-2 text-red-500 hover:text-red-700 font-medium px-4 py-2 rounded-lg hover:bg-red-50 transition-all duration-300"
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
            >
              <Trash2 className="h-4 w-4" />
              Xóa tất cả
            </motion.button>
          )}
        </motion.div>

        {/* Error Display */}
        <AnimatePresence>
          {error && (
            <motion.div
              className="mb-6 p-4 bg-red-100 border border-red-400 text-red-700 rounded-xl"
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: 'auto' }}
              exit={{ opacity: 0, height: 0 }}
            >
              <p>{error}</p>
            </motion.div>
          )}
        </AnimatePresence>

        {cartItems.length === 0 ? (
          /* Empty Cart State */
          <motion.div 
            className="text-center py-16"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <div className="max-w-md mx-auto">
              <motion.div
                className="relative mb-8"
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                transition={{ delay: 0.2, type: "spring", stiffness: 200 }}
              >
                <div className="w-32 h-32 mx-auto bg-gradient-to-r from-gray-100 to-gray-200 rounded-full flex items-center justify-center">
                  <ShoppingBag className="h-16 w-16 text-gray-400" />
                </div>
                <motion.div
                  className="absolute -top-2 -right-2"
                  animate={{ rotate: [0, -10, 10, -10, 0] }}
                  transition={{ delay: 1, duration: 0.5 }}
                >
                  <div className="w-8 h-8 bg-yellow-400 rounded-full flex items-center justify-center">
                    <span className="text-lg">😢</span>
                  </div>
                </motion.div>
              </motion.div>

              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.4 }}
              >
                <h2 className="text-3xl font-bold text-gray-900 mb-4">Giỏ hàng trống</h2>
                <p className="text-gray-600 mb-8 text-lg">
                  Hãy khám phá những sản phẩm tuyệt vời của chúng tôi và thêm vào giỏ hàng nhé!
                </p>
              </motion.div>

              <motion.div
                className="space-y-4"
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.6 }}
              >
                                 <Link
                   to="/products"
                   className="inline-flex items-center gap-3 bg-gradient-to-r from-blue-600 via-purple-600 to-indigo-600 hover:from-blue-700 hover:via-purple-700 hover:to-indigo-700 text-white px-8 py-4 rounded-xl transition-all duration-300 font-semibold text-lg group"
                 >
                  <ShoppingBag className="h-5 w-5" />
                  Khám phá sản phẩm
                  <ArrowRight className="h-5 w-5 group-hover:translate-x-1 transition-transform" />
                </Link>
                
                <div className="flex items-center justify-center gap-6 text-sm text-gray-500 mt-6">
                  <div className="flex items-center gap-2">
                    <Truck className="h-4 w-4" />
                    Miễn phí vận chuyển
                  </div>
                  <div className="flex items-center gap-2">
                    <Shield className="h-4 w-4" />
                    Bảo hành chính hãng
                  </div>
                </div>
              </motion.div>
            </div>
          </motion.div>
        ) : (
          /* Cart with Items */
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Cart Items */}
            <motion.div 
              className="lg:col-span-2"
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ duration: 0.6 }}
            >
              <div className="space-y-4">
                <AnimatePresence>
                  {cartItems.map((item, index) => (
                    <motion.div
                      key={item.inventoryId}
                      className="bg-white rounded-xl shadow-md overflow-hidden hover:shadow-lg transition-all duration-300"
                      initial={{ opacity: 0, y: 20 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, x: -100, height: 0 }}
                      transition={{ delay: index * 0.1 }}
                      layout
                    >
                      <div className="p-6">
                        <div className="flex gap-6">
                          {/* Product Image */}
                          <motion.div 
                            className="relative flex-shrink-0"
                            whileHover={{ scale: 1.05 }}
                          >
                            <img
                              src={item.mainImage}
                              alt={item.productName}
                              className="w-24 h-24 md:w-32 md:h-32 object-cover rounded-xl"
                            />
                            {item.hasActivePromotion && (
                              <div className="absolute -top-2 -right-2 bg-purple-500 text-white text-xs px-2 py-1 rounded-full">
                                <Percent className="h-3 w-3" />
                              </div>
                            )}
                          </motion.div>

                          {/* Product Details */}
                          <div className="flex-1 min-w-0">
                            <div className="flex justify-between items-start mb-3">
                              <div className="flex-1 min-w-0">
                                <h3 className="font-bold text-lg text-gray-900 truncate">
                                  {item.productName}
                                </h3>
                                <div className="flex items-center gap-2 mt-1">
                                  <span className="text-gray-600">Size: {item.size}</span>
                                  {item.brandName && (
                                    <>
                                      <span className="text-gray-400">•</span>
                                      <span className="text-gray-600 text-sm">{item.brandName}</span>
                                    </>
                                  )}
                                </div>
                              </div>
                              
                              <motion.button
                                onClick={() => handleRemoveItem(item.inventoryId, item.productName)}
                                disabled={removingItem === item.inventoryId}
                                className="p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-all duration-300 disabled:opacity-50"
                                whileHover={{ scale: 1.1 }}
                                whileTap={{ scale: 0.9 }}
                              >
                                {removingItem === item.inventoryId ? (
                                  <div className="animate-spin rounded-full h-5 w-5 border-2 border-red-500 border-t-transparent"></div>
                                ) : (
                                  <X className="h-5 w-5" />
                                )}
                              </motion.button>
                            </div>

                            {/* Promotion Badge */}
                            {item.hasActivePromotion && item.promotionName && (
                              <motion.div 
                                className="inline-flex items-center gap-2 bg-gradient-to-r from-purple-500 to-pink-500 text-white px-3 py-1 rounded-full text-sm font-medium mb-3"
                                initial={{ scale: 0 }}
                                animate={{ scale: 1 }}
                                transition={{ delay: 0.5 }}
                              >
                                <Gift className="h-3 w-3" />
                                {item.promotionName}
                              </motion.div>
                            )}

                            <div className="flex items-center justify-between">
                              {/* Quantity Controls */}
                              <div className="flex items-center bg-gray-100 rounded-xl overflow-hidden">
                                <motion.button
                                  onClick={() => handleQuantityChange(item.inventoryId, item.quantity - 1)}
                                  disabled={loading || item.quantity <= 1}
                                  className="p-3 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                  whileTap={{ scale: 0.9 }}
                                >
                                  <Minus className="h-4 w-4" />
                                </motion.button>
                                <span className="px-4 py-3 min-w-[3rem] text-center font-semibold">
                                  {item.quantity}
                                </span>
                                <motion.button
                                  onClick={() => handleQuantityChange(item.inventoryId, item.quantity + 1)}
                                  disabled={loading}
                                  className="p-3 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                  whileTap={{ scale: 0.9 }}
                                >
                                  <Plus className="h-4 w-4" />
                                </motion.button>
                              </div>

                              {/* Price */}
                              <div className="text-right">
                                {item.hasActivePromotion && item.promotionPrice ? (
                                  <div>
                                    <p className="font-bold text-lg text-purple-600">
                                      {formatPrice(item.promotionPrice * item.quantity)}
                                    </p>
                                    <p className="text-sm text-gray-500 line-through">
                                      {formatPrice(item.price * item.quantity)}
                                    </p>
                                    <p className="text-xs text-green-600 font-medium">
                                      Tiết kiệm {formatPrice((item.price - item.promotionPrice) * item.quantity)}
                                    </p>
                                  </div>
                                ) : item.salePrice ? (
                                  <div>
                                    <p className="font-bold text-lg text-red-500">
                                      {formatPrice(item.salePrice * item.quantity)}
                                    </p>
                                    <p className="text-sm text-gray-500 line-through">
                                      {formatPrice(item.price * item.quantity)}
                                    </p>
                                    <p className="text-xs text-green-600 font-medium">
                                      Giảm {Math.round((1 - item.salePrice / item.price) * 100)}%
                                    </p>
                                  </div>
                                ) : (
                                  <p className="font-bold text-lg text-gray-900">
                                    {formatPrice(item.price * item.quantity)}
                                  </p>
                                )}
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </motion.div>
                  ))}
                </AnimatePresence>
              </div>
            </motion.div>

            {/* Order Summary */}
            <motion.div 
              className="lg:col-span-1"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ duration: 0.6, delay: 0.2 }}
            >
              <div className="bg-white rounded-xl shadow-lg p-6 sticky top-4">
                <h2 className="text-xl font-bold mb-6 flex items-center gap-2">
                  <Tag className="h-5 w-5" />
                  Tóm tắt đơn hàng
                </h2>
                
                <div className="space-y-4 mb-6">
                  <div className="flex justify-between items-center">
                    <span className="text-gray-600">Tạm tính ({cartItems.length} sản phẩm):</span>
                    <span className="font-semibold">{formatPrice(subtotal)}</span>
                  </div>
                  
                  {totalSavings > 0 && (
                    <motion.div 
                      className="flex justify-between items-center text-green-600"
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                    >
                      <span className="flex items-center gap-1">
                        <Gift className="h-4 w-4" />
                        Tiết kiệm:
                      </span>
                      <span className="font-semibold">-{formatPrice(totalSavings)}</span>
                    </motion.div>
                  )}
                  
                  <div className="flex justify-between items-center">
                    <span className="text-gray-600">Phí vận chuyển:</span>
                    <span className="text-green-600 font-medium">Miễn phí</span>
                  </div>
                  
                  <hr className="border-gray-200" />
                  
                  <div className="flex justify-between items-center text-xl font-bold">
                    <span>Tổng cộng:</span>
                    <span className="text-black">{formatPrice(subtotal)}</span>
                  </div>
                </div>

                {/* Benefits */}
                <div className="space-y-3 mb-6 p-4 bg-gray-50 rounded-xl">
                  <div className="flex items-center gap-2 text-sm text-gray-600">
                    <Truck className="h-4 w-4 text-green-500" />
                    <span>Miễn phí vận chuyển toàn quốc</span>
                  </div>
                  <div className="flex items-center gap-2 text-sm text-gray-600">
                    <Shield className="h-4 w-4 text-blue-500" />
                    <span>Bảo hành chính hãng 12 tháng</span>
                  </div>
                  <div className="flex items-center gap-2 text-sm text-gray-600">
                    <Heart className="h-4 w-4 text-red-500" />
                    <span>Đổi trả miễn phí trong 30 ngày</span>
                  </div>
                </div>

                <div className="space-y-3">
                                     <motion.button
                     onClick={() => navigate('/checkout')}
                     className="w-full bg-gradient-to-r from-green-600 via-emerald-600 to-teal-600 hover:from-green-700 hover:via-emerald-700 hover:to-teal-700 text-white py-4 rounded-xl transition-all duration-300 font-semibold text-lg flex items-center justify-center gap-2 group shadow-lg hover:shadow-xl"
                     whileHover={{ scale: 1.02 }}
                     whileTap={{ scale: 0.98 }}
                   >
                    Thanh toán ngay
                    <ArrowRight className="h-5 w-5 group-hover:translate-x-1 transition-transform" />
                  </motion.button>
                  
                  <Link
                    to="/products"
                    className="block w-full text-center py-4 border-2 border-gray-200 rounded-xl hover:bg-gray-50 hover:border-gray-300 transition-all duration-300 font-medium text-gray-700"
                  >
                    Tiếp tục mua sắm
                  </Link>
                </div>
              </div>
            </motion.div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Cart; 