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
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 });
  const [isHovering, setIsHovering] = useState(false);
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  const [slideDirection, setSlideDirection] = useState('');

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

  const handleNextImage = () => {
    if (product.images && product.images.length > 0) {
      setSlideDirection('right');
      setCurrentImageIndex((prev) => (prev + 1) % (product.images.length + 1));
    }
  };

  const handlePrevImage = () => {
    if (product.images && product.images.length > 0) {
      setSlideDirection('left');
      setCurrentImageIndex((prev) => (prev - 1 + product.images.length + 1) % (product.images.length + 1));
    }
  };

  const handleThumbnailClick = (index) => {
    setSlideDirection(index > currentImageIndex ? 'right' : 'left');
    setCurrentImageIndex(index);
  };

  const getCurrentImage = () => {
    if (currentImageIndex === 0) {
      return product.mainImage;
    }
    return product.images[currentImageIndex - 1];
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
          <div className="flex gap-4">
            {/* Thumbnails Column */}
            {product.images && product.images.length > 0 && (
              <div className="relative">
                <button
                  onClick={() => {
                    const newIndex = currentImageIndex === 0 
                      ? product.images.length 
                      : currentImageIndex - 1;
                    handleThumbnailClick(newIndex);
                  }}
                  className="absolute left-1/2 -translate-x-1/2 -top-2 bg-white/80 hover:bg-white p-1 rounded-full shadow-md transition-all z-10"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
                  </svg>
                </button>
                <div className="flex flex-col gap-2 h-[400px] overflow-y-auto scrollbar-hide">
                  <div 
                    className={`w-20 aspect-square overflow-hidden rounded-lg bg-gray-100 cursor-pointer border-2 ${
                      currentImageIndex === 0 ? 'border-black' : 'border-transparent'
                    }`}
                    onClick={() => handleThumbnailClick(0)}
                  >
                    <img
                      src={product.mainImage}
                      alt={`${product.name} - main`}
                      className="w-full h-full object-cover"
                    />
                  </div>
                  {product.images.map((image, index) => (
                    <div 
                      key={index} 
                      className={`w-20 aspect-square overflow-hidden rounded-lg bg-gray-100 cursor-pointer border-2 ${
                        currentImageIndex === index + 1 ? 'border-black' : 'border-transparent'
                      }`}
                      onClick={() => handleThumbnailClick(index + 1)}
                    >
                      <img
                        src={image}
                        alt={`${product.name} - ${index + 1}`}
                        className="w-full h-full object-cover"
                      />
                    </div>
                  ))}
                </div>
                <button
                  onClick={() => {
                    const newIndex = currentImageIndex === product.images.length 
                      ? 0 
                      : currentImageIndex + 1;
                    handleThumbnailClick(newIndex);
                  }}
                  className="absolute left-1/2 -translate-x-1/2 -bottom-2 bg-white/80 hover:bg-white p-1 rounded-full shadow-md transition-all z-10"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </button>
              </div>
            )}

            {/* Main Image */}
            <div className="flex-1 relative">
              <div 
                className="aspect-square overflow-hidden rounded-lg bg-gray-100 relative"
                onMouseMove={(e) => {
                  const rect = e.currentTarget.getBoundingClientRect();
                  const x = ((e.clientX - rect.left) / rect.width) * 100;
                  const y = ((e.clientY - rect.top) / rect.height) * 100;
                  setMousePosition({ x, y });
                }}
                onMouseEnter={() => setIsHovering(true)}
                onMouseLeave={() => setIsHovering(false)}
              >
                <div className="relative w-full h-full">
                  <img
                    key={currentImageIndex}
                    src={getCurrentImage()}
                    alt={product.name}
                    className={`w-full h-full object-cover transition-all duration-500 ease-in-out ${
                      slideDirection === 'right' ? 'slide-in-right' : 'slide-in-left'
                    }`}
                  />
                  {isHovering && (
                    <div 
                      className="absolute inset-0 pointer-events-none"
                      style={{
                        backgroundImage: `url(${getCurrentImage()})`,
                        backgroundPosition: `${mousePosition.x}% ${mousePosition.y}%`,
                        backgroundSize: '300%',
                        backgroundRepeat: 'no-repeat',
                        clipPath: 'circle(200px at var(--x) var(--y))',
                        '--x': `${mousePosition.x}%`,
                        '--y': `${mousePosition.y}%`,
                      }}
                    />
                  )}
                </div>
              </div>
              {product.images && product.images.length > 0 && (
                <>
                  <button
                    onClick={handlePrevImage}
                    className="absolute left-2 top-1/2 -translate-y-1/2 bg-white/80 hover:bg-white p-2 rounded-full shadow-md transition-all duration-300 ease-in-out hover:scale-110"
                  >
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                    </svg>
                  </button>
                  <button
                    onClick={handleNextImage}
                    className="absolute right-2 top-1/2 -translate-y-1/2 bg-white/80 hover:bg-white p-2 rounded-full shadow-md transition-all duration-300 ease-in-out hover:scale-110"
                  >
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                  </button>
                </>
              )}
            </div>
          </div>
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

      <style jsx>{`
        @keyframes slideInRight {
          from {
            transform: translateX(100%);
            opacity: 0;
          }
          to {
            transform: translateX(0);
            opacity: 1;
          }
        }

        @keyframes slideInLeft {
          from {
            transform: translateX(-100%);
            opacity: 0;
          }
          to {
            transform: translateX(0);
            opacity: 1;
          }
        }

        .slide-in-right {
          animation: slideInRight 0.5s ease-in-out;
        }

        .slide-in-left {
          animation: slideInLeft 0.5s ease-in-out;
        }
      `}</style>
    </div>
  );
};

export default ProductDetail; 