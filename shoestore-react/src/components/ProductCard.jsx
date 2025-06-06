import { Link } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useToast } from './Toast';
import { useState } from 'react';

const ProductCard = ({ product, isHovered, onMouseEnter, onMouseLeave }) => {
  const { addToCart, loading: cartLoading } = useCart();
  const { addToast } = useToast();
  const [isAddingToCart, setIsAddingToCart] = useState(false);

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  const handleQuickAddToCart = async (e) => {
    e.preventDefault(); // Prevent navigation to product detail
    e.stopPropagation();

    if (product.totalQuantity === 0) {
      addToast('Sản phẩm đã hết hàng', 'warning');
      return;
    }

    // Get the first available inventory (smallest size)
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

  return (
    <Link
      to={`/products/${product.id}`}
      className="group block"
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
    >
      <div className="relative aspect-square overflow-hidden rounded-lg bg-gray-100 mb-4 group">
        <img
          src={isHovered && product.images?.length > 1 
            ? product.images[1] 
            : (product.mainImage || product.images?.[0] || 'https://via.placeholder.com/400')}
          alt={product.name}
          className="object-cover w-full h-full group-hover:scale-105 transition-all duration-300"
        />
        {product.salePrice && (
          <div className="absolute top-2 right-2 bg-red-500 text-white px-2 py-1 rounded-md">
            -{Math.round((1 - product.salePrice / product.price) * 100)}%
          </div>
        )}
        {/* Quick Add to Cart Button */}
        <button
          onClick={handleQuickAddToCart}
          disabled={product.totalQuantity === 0 || isAddingToCart}
          className={`absolute bottom-2 left-2 right-2 py-2 px-4 rounded-md text-sm font-medium transition-all duration-300 transform translate-y-full group-hover:translate-y-0 opacity-0 group-hover:opacity-100 ${
            product.totalQuantity === 0 
              ? 'bg-gray-400 text-gray-600 cursor-not-allowed' 
              : 'bg-black text-white hover:bg-gray-800'
          } disabled:opacity-50`}
        >
          {isAddingToCart ? (
            <div className="flex items-center justify-center gap-2">
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
              Đang thêm...
            </div>
          ) : product.totalQuantity === 0 ? (
            'Hết hàng'
          ) : (
            'Thêm vào giỏ'
          )}
        </button>
      </div>
      <h3 className="font-medium text-gray-900 mb-1 line-clamp-2">{product.name}</h3>
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
        {product.totalQuantity === 0 ? 'Hết hàng' : `Hiện có: ${product.totalQuantity}`}
      </div>
    </Link>
  );
};

export default ProductCard; 