import { useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useToast } from '../components/Toast';

const Cart = () => {
  const { cart, loading, error, updateQuantity, removeFromCart, clearCart } = useCart();
  const { addToast } = useToast();
  const navigate = useNavigate();

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  const cartItems = cart?.items || [];
  const subtotal = cartItems.reduce(
    (total, item) => {
      const effectivePrice = item.salePrice || item.price || 0;
      return total + effectivePrice * item.quantity;
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
    const success = await removeFromCart(inventoryId);
    if (success) {
      addToast(`Đã xóa ${productName} khỏi giỏ hàng`, 'success');
    } else {
      addToast('Có lỗi xảy ra khi xóa sản phẩm', 'error');
    }
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
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-center items-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900"></div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold">Giỏ hàng</h1>
        {cartItems.length > 0 && (
          <button
            onClick={handleClearCart}
            className="text-red-500 hover:text-red-700 text-sm font-medium"
          >
            Xóa tất cả
          </button>
        )}
      </div>

      {/* Error Display */}
      {error && (
        <div className="mb-6 p-4 bg-red-100 border border-red-400 text-red-700 rounded-lg">
          <p>{error}</p>
        </div>
      )}

      {cartItems.length === 0 ? (
        <div className="text-center py-12">
          <div className="mb-8">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              className="h-24 w-24 mx-auto text-gray-400 mb-4"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={1}
                d="M3 3h2l.4 2M7 13h10l4-8H5.4m0 0L7 13m0 0l-2.5 5M7 13l2.5 5m0 0h8m-8 0v-2m8 2v-2"
              />
            </svg>
            <h2 className="text-2xl font-semibold text-gray-700 mb-2">Giỏ hàng trống</h2>
            <p className="text-gray-500 mb-6">Bạn chưa có sản phẩm nào trong giỏ hàng</p>
          </div>
          <Link
            to="/products"
            className="inline-block bg-black text-white px-6 py-3 rounded-lg hover:bg-gray-800 transition-colors"
          >
            Tiếp tục mua sắm
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Cart Items */}
          <div className="lg:col-span-2">
            <div className="space-y-4">
              {cartItems.map((item) => (
                <div key={item.inventoryId} className="flex gap-4 p-4 border rounded-lg">
                  <img
                    src={item.mainImage}
                    alt={item.productName}
                    className="w-24 h-24 object-cover rounded-lg"
                  />
                  <div className="flex-1">
                    <div className="flex justify-between items-start mb-2">
                      <div>
                        <h3 className="font-semibold text-lg">{item.productName}</h3>
                        <p className="text-gray-500">Size: {item.size}</p>
                        {item.brandName && (
                          <p className="text-gray-500 text-sm">{item.brandName}</p>
                        )}
                      </div>
                      <button
                        onClick={() => handleRemoveItem(item.inventoryId, item.productName)}
                        className="text-gray-400 hover:text-red-500 p-1"
                      >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12"></path>
                        </svg>
                      </button>
                    </div>
                    
                    <div className="flex items-center justify-between">
                      {/* Quantity Controls */}
                      <div className="flex items-center border rounded-lg">
                        <button
                          onClick={() => handleQuantityChange(item.inventoryId, item.quantity - 1)}
                          disabled={loading || item.quantity <= 1}
                          className="px-3 py-2 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          -
                        </button>
                        <span className="px-4 py-2 min-w-[3rem] text-center">{item.quantity}</span>
                        <button
                          onClick={() => handleQuantityChange(item.inventoryId, item.quantity + 1)}
                          disabled={loading}
                          className="px-3 py-2 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          +
                        </button>
                      </div>

                      {/* Price */}
                      <div className="text-right">
                        {item.salePrice ? (
                          <div>
                            <p className="font-semibold text-red-500">
                              {formatPrice(item.salePrice * item.quantity)}
                            </p>
                            <p className="text-sm text-gray-500 line-through">
                              {formatPrice(item.price * item.quantity)}
                            </p>
                          </div>
                        ) : (
                          <p className="font-semibold">
                            {formatPrice(item.price * item.quantity)}
                          </p>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Order Summary */}
          <div className="lg:col-span-1">
            <div className="bg-gray-50 rounded-lg p-6 sticky top-4">
              <h2 className="text-xl font-semibold mb-4">Tổng kết đơn hàng</h2>
              
              <div className="space-y-3 mb-6">
                <div className="flex justify-between">
                  <span>Tạm tính ({cartItems.length} sản phẩm):</span>
                  <span>{formatPrice(subtotal)}</span>
                </div>
                <div className="flex justify-between">
                  <span>Phí vận chuyển:</span>
                  <span className="text-green-600">Miễn phí</span>
                </div>
                <hr />
                <div className="flex justify-between text-lg font-semibold">
                  <span>Tổng cộng:</span>
                  <span>{formatPrice(subtotal)}</span>
                </div>
              </div>

              <div className="space-y-3">
                <button
                  onClick={() => navigate('/checkout')}
                  className="w-full bg-black text-white py-3 rounded-lg hover:bg-gray-800 transition-colors font-medium"
                >
                  Tiến hành thanh toán
                </button>
                <Link
                  to="/products"
                  className="block w-full text-center py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  Tiếp tục mua sắm
                </Link>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Cart; 