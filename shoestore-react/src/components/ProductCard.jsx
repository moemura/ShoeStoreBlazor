import { Link } from 'react-router-dom';

const ProductCard = ({ product, isHovered, onMouseEnter, onMouseLeave }) => {
  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  return (
    <Link
      to={`/products/${product.id}`}
      className="group block"
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
    >
      <div className="relative aspect-square overflow-hidden rounded-lg bg-gray-100 mb-4">
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