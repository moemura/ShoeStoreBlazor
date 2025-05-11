import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { productService } from '../services/productService';

const ProductDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedSize, setSelectedSize] = useState('');
  const [quantity, setQuantity] = useState(1);

  useEffect(() => {
    const fetchProduct = async () => {
      try {
        const data = await productService.getById(id);
        setProduct(data);
        // Set default selected size if available
        if (data.inventories && data.inventories.length > 0) {
          setSelectedSize(data.inventories[0].sizeId);
        }
      } catch (error) {
        console.error('Error fetching product:', error);
        setError('Không thể tải thông tin sản phẩm');
      } finally {
        setLoading(false);
      }
    };

    fetchProduct();
  }, [id]);

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  const handleAddToCart = () => {
    if (!selectedSize) {
      alert('Vui lòng chọn size');
      return;
    }
    // TODO: Implement add to cart functionality
    console.log('Add to cart:', { productId: id, size: selectedSize, quantity });
  };

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="animate-pulse">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div className="bg-gray-200 h-96 rounded-lg"></div>
            <div className="space-y-4">
              <div className="h-8 bg-gray-200 rounded w-3/4"></div>
              <div className="h-4 bg-gray-200 rounded w-1/2"></div>
              <div className="h-4 bg-gray-200 rounded w-1/4"></div>
            </div>
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
          <button
            onClick={() => navigate(-1)}
            className="mt-4 px-4 py-2 bg-gray-100 rounded-md hover:bg-gray-200"
          >
            Quay lại
          </button>
        </div>
      </div>
    );
  }

  if (!product) {
    return null;
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {/* Product Images */}
        <div className="space-y-4">
          <div className="aspect-square overflow-hidden rounded-lg bg-gray-100">
            <img
              src={product.mainImage}
              alt={product.name}
              className="w-full h-full object-cover"
            />
          </div>
          {product.images && product.images.length > 0 && (
            <div className="grid grid-cols-4 gap-4">
              {product.images.map((image, index) => (
                <div key={index} className="aspect-square overflow-hidden rounded-lg bg-gray-100">
                  <img
                    src={image}
                    alt={`${product.name} - ${index + 1}`}
                    className="w-full h-full object-cover"
                  />
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Product Info */}
        <div className="space-y-6">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">{product.name}</h1>
            <p className="mt-2 text-gray-500">{product.brandName}</p>
          </div>

          <div className="space-y-2">
            <div className="flex items-center gap-4">
              {product.salePrice ? (
                <>
                  <p className="text-2xl font-bold text-red-500">
                    {formatPrice(product.salePrice)}
                  </p>
                  <p className="text-lg text-gray-500 line-through">
                    {formatPrice(product.price)}
                  </p>
                </>
              ) : (
                <p className="text-2xl font-bold text-gray-900">
                  {formatPrice(product.price)}
                </p>
              )}
            </div>
            {product.salePrice && (
              <p className="text-sm text-red-500">
                Giảm {Math.round((1 - product.salePrice / product.price) * 100)}%
              </p>
            )}
          </div>

          <div>
            <h2 className="text-lg font-semibold mb-2">Mô tả</h2>
            <p className="text-gray-600">{product.description}</p>
          </div>

          {/* Size Selection */}
          <div>
            <h2 className="text-lg font-semibold mb-2">Chọn size</h2>
            <div className="grid grid-cols-4 gap-2">
              {product.inventories.map((inventory) => (
                <button
                  key={inventory.sizeId}
                  onClick={() => setSelectedSize(inventory.sizeId)}
                  disabled={inventory.quantity === 0}
                  className={`px-4 py-2 border rounded-md text-center ${
                    selectedSize === inventory.sizeId
                      ? 'border-black bg-black text-white'
                      : 'border-gray-300 hover:border-gray-400'
                  } ${
                    inventory.quantity === 0
                      ? 'opacity-50 cursor-not-allowed'
                      : 'cursor-pointer'
                  }`}
                >
                  Size {inventory.sizeId}
                  <span className="block text-xs mt-1">
                    còn {inventory.quantity}
                  </span>
                </button>
              ))}
            </div>
          </div>

          {/* Quantity Selection */}
          <div>
            <h2 className="text-lg font-semibold mb-2">Số lượng</h2>
            <div className="flex items-center gap-4">
              <button
                onClick={() => setQuantity(Math.max(1, quantity - 1))}
                className="px-3 py-1 border rounded-md hover:bg-gray-100"
              >
                -
              </button>
              <span className="w-12 text-center">{quantity}</span>
              <button
                onClick={() => setQuantity(quantity + 1)}
                className="px-3 py-1 border rounded-md hover:bg-gray-100"
              >
                +
              </button>
            </div>
          </div>

          {/* Add to Cart Button */}
          <button
            onClick={handleAddToCart}
            className="w-full py-3 bg-black text-white rounded-md hover:bg-gray-800 transition-colors"
          >
            Thêm vào giỏ hàng
          </button>
        </div>
      </div>
    </div>
  );
};

export default ProductDetail; 