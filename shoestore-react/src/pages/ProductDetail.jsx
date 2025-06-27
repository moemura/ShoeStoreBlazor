import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  ChevronLeft, 
  ChevronRight, 
  ZoomIn, 
  Heart, 
  Share2, 
  Star, 
  Plus, 
  Minus,
  ShoppingCart,
  Check,
  Truck,
  Shield,
  RotateCcw,
  MessageSquare,
  ThumbsUp,
  Camera,
  ArrowLeft,
  User
} from 'lucide-react';
import { productService } from '../services/productService';
import { useCart } from '../context/CartContext';
import { useToast } from '../components/Toast';

const ProductDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { addToCart, loading: cartLoading } = useCart();
  const { addToast } = useToast();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedSize, setSelectedSize] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  const [isZoomed, setIsZoomed] = useState(false);
  const [isWishlisted, setIsWishlisted] = useState(false);
  const [activeTab, setActiveTab] = useState('description');
  const [relatedProducts, setRelatedProducts] = useState([]);

  useEffect(() => {
    const fetchProduct = async () => {
      try {
        const estimatedOrderTotal = 1000000;
        const data = await productService.getByIdWithDynamicPricing(id, estimatedOrderTotal);
        setProduct(data);
        
        if (data.inventories && data.inventories.length > 0) {
          setSelectedSize(data.inventories[0].sizeId);
        }

        // Fetch related products
        try {
          const related = await productService.getProducts({
            categoryId: data.categoryId,
            limit: 4,
            excludeId: id
          });
          setRelatedProducts(related.items || []);
        } catch (relatedError) {
          console.log('Could not fetch related products:', relatedError);
        }
      } catch (error) {
        console.error('Error fetching product:', error);
        try {
          const data = await productService.getById(id);
          setProduct(data);
          if (data.inventories && data.inventories.length > 0) {
            setSelectedSize(data.inventories[0].sizeId);
          }
        } catch (fallbackError) {
          console.error('Fallback error:', fallbackError);
          setError('Không thể tải thông tin sản phẩm');
        }
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

  const handleAddToCart = async () => {
    if (!selectedSize) {
      addToast('Vui lòng chọn size', 'warning');
      return;
    }

    const selectedInventory = product.inventories.find(inv => inv.sizeId === selectedSize);
    if (!selectedInventory) {
      addToast('Không tìm thấy size được chọn', 'error');
      return;
    }

    if (selectedInventory.quantity < quantity) {
      addToast(`Chỉ còn ${selectedInventory.quantity} sản phẩm trong kho`, 'warning');
      return;
    }

    try {
      const success = await addToCart(selectedInventory.id, quantity);
      if (success) {
        addToast(`Đã thêm ${quantity} sản phẩm vào giỏ hàng!`, 'success');
      } else {
        addToast('Có lỗi xảy ra khi thêm vào giỏ hàng', 'error');
      }
    } catch (error) {
      console.error('Error adding to cart:', error);
      addToast('Có lỗi xảy ra khi thêm vào giỏ hàng', 'error');
    }
  };

  const allImages = product ? [product.mainImage, ...(product.images || [])] : [];

  const handleImageChange = (index) => {
    setCurrentImageIndex(index);
  };

  const handlePrevImage = () => {
    setCurrentImageIndex((prev) => (prev - 1 + allImages.length) % allImages.length);
  };

  const handleNextImage = () => {
    setCurrentImageIndex((prev) => (prev + 1) % allImages.length);
  };

  const toggleWishlist = () => {
    setIsWishlisted(!isWishlisted);
    addToast(isWishlisted ? 'Đã xóa khỏi wishlist' : 'Đã thêm vào wishlist', 'success');
  };

  const handleShare = () => {
    if (navigator.share) {
      navigator.share({
        title: product.name,
        text: `Xem sản phẩm ${product.name} tại ShoeStore`,
        url: window.location.href,
      });
    } else {
      navigator.clipboard.writeText(window.location.href);
      addToast('Đã copy link sản phẩm', 'success');
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="container mx-auto px-4 py-8">
          <motion.div 
            className="animate-pulse"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
          >
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
              <div className="space-y-4">
                <div className="bg-gray-200 h-96 md:h-[500px] rounded-2xl"></div>
                <div className="flex gap-2">
                  {[...Array(4)].map((_, i) => (
                    <div key={i} className="bg-gray-200 h-20 w-20 rounded-lg"></div>
                  ))}
                </div>
              </div>
              <div className="space-y-6">
                <div className="h-8 bg-gray-200 rounded w-3/4"></div>
                <div className="h-4 bg-gray-200 rounded w-1/2"></div>
                <div className="h-6 bg-gray-200 rounded w-1/4"></div>
                <div className="space-y-2">
                  {[...Array(3)].map((_, i) => (
                    <div key={i} className="h-4 bg-gray-200 rounded"></div>
                  ))}
                </div>
              </div>
            </div>
          </motion.div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <motion.div 
          className="text-center py-12"
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
        >
          <div className="text-6xl mb-4">😞</div>
          <p className="text-red-500 text-lg mb-4">{error}</p>
          <button
            onClick={() => navigate(-1)}
            className="px-6 py-3 bg-gradient-to-r from-gray-600 to-gray-700 hover:from-gray-700 hover:to-gray-800 text-white rounded-xl transition-all duration-300 shadow-lg"
          >
            Quay lại
          </button>
        </motion.div>
      </div>
    );
  }

  if (!product) {
    return null;
  }

  const mockReviews = [
    {
      id: 1,
      user: "Minh Anh",
      rating: 5,
      comment: "Chất lượng tuyệt vời, đúng như mô tả. Giao hàng nhanh!",
      date: "2024-01-15",
      verified: true
    },
    {
      id: 2,
      user: "Hoàng Nam",
      rating: 4,
      comment: "Giày đẹp, thoải mái. Size chuẩn như bảng size.",
      date: "2024-01-10",
      verified: true
    }
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Breadcrumb */}
      <div className="bg-white border-b">
        <div className="container mx-auto px-4 py-4">
          <motion.div 
            className="flex items-center gap-2 text-sm text-gray-600"
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
          >
            <button 
              onClick={() => navigate(-1)}
              className="flex items-center gap-1 hover:text-black transition-colors"
            >
              <ArrowLeft className="h-4 w-4" />
              Quay lại
            </button>
            <span>/</span>
            <span className="text-black font-medium">{product.name}</span>
          </motion.div>
        </div>
      </div>

      <div className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
          {/* Image Gallery */}
          <motion.div 
            className="space-y-4"
            initial={{ opacity: 0, x: -50 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.6 }}
          >
            {/* Main Image */}
            <div className="relative group">
              <motion.div 
                className="aspect-square bg-white rounded-2xl shadow-lg overflow-hidden relative"
                whileHover={{ scale: 1.02 }}
                transition={{ duration: 0.3 }}
              >
                <AnimatePresence mode="wait">
                  <motion.img
                    key={currentImageIndex}
                    src={allImages[currentImageIndex]}
                    alt={product.name}
                    className="w-full h-full object-cover"
                    initial={{ opacity: 0, scale: 1.1 }}
                    animate={{ opacity: 1, scale: 1 }}
                    exit={{ opacity: 0, scale: 0.9 }}
                    transition={{ duration: 0.4 }}
                  />
                </AnimatePresence>

                {/* Navigation Arrows */}
                {allImages.length > 1 && (
                  <>
                    <button
                      onClick={handlePrevImage}
                      className="absolute left-4 top-1/2 -translate-y-1/2 bg-white/90 hover:bg-white p-3 rounded-full shadow-lg transition-all duration-300 opacity-0 group-hover:opacity-100"
                    >
                      <ChevronLeft className="h-5 w-5" />
                    </button>
                    <button
                      onClick={handleNextImage}
                      className="absolute right-4 top-1/2 -translate-y-1/2 bg-white/90 hover:bg-white p-3 rounded-full shadow-lg transition-all duration-300 opacity-0 group-hover:opacity-100"
                    >
                      <ChevronRight className="h-5 w-5" />
                    </button>
                  </>
                )}

                {/* Zoom Button */}
                <button
                  onClick={() => setIsZoomed(true)}
                  className="absolute top-4 right-4 bg-white/90 hover:bg-white p-3 rounded-full shadow-lg transition-all duration-300 opacity-0 group-hover:opacity-100"
                >
                  <ZoomIn className="h-5 w-5" />
                </button>

                {/* Image Counter */}
                {allImages.length > 1 && (
                  <div className="absolute bottom-4 left-1/2 -translate-x-1/2 bg-black/70 text-white px-3 py-1 rounded-full text-sm">
                    {currentImageIndex + 1} / {allImages.length}
                  </div>
                )}
              </motion.div>
            </div>

            {/* Thumbnail Gallery */}
            {allImages.length > 1 && (
              <motion.div 
                className="flex gap-3 overflow-x-auto pb-2"
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2 }}
              >
                {allImages.map((image, index) => (
                  <motion.button
                    key={index}
                    onClick={() => handleImageChange(index)}
                    className={`flex-shrink-0 w-20 h-20 rounded-lg overflow-hidden border-2 transition-all duration-300 ${
                      currentImageIndex === index 
                        ? 'border-transparent bg-gradient-to-r from-blue-600 to-purple-600'
                        : 'border-transparent hover:border-gray-300'
                    }`}
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                  >
                    <img
                      src={image}
                      alt={`${product.name} - ${index + 1}`}
                      className="w-full h-full object-cover"
                    />
                  </motion.button>
                ))}
              </motion.div>
            )}
          </motion.div>

          {/* Product Information */}
          <motion.div 
            className="space-y-8"
            initial={{ opacity: 0, x: 50 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.6, delay: 0.2 }}
          >
            {/* Header */}
            <div className="space-y-4">
              <div className="flex justify-between items-start">
                <div>
                  <motion.h1 
                    className="text-3xl md:text-4xl font-bold text-gray-900"
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.3 }}
                  >
                    {product.name}
                  </motion.h1>
                  <motion.p 
                    className="text-lg text-gray-600 mt-2"
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.4 }}
                  >
                    {product.brandName}
                  </motion.p>
                </div>
                
                <div className="flex gap-2">
                  <motion.button
                    onClick={toggleWishlist}
                    className={`p-3 rounded-full border transition-all duration-300 ${
                      isWishlisted 
                        ? 'bg-red-50 border-red-200 text-red-500' 
                        : 'bg-white border-gray-200 text-gray-400 hover:text-red-500'
                    }`}
                    whileHover={{ scale: 1.1 }}
                    whileTap={{ scale: 0.9 }}
                  >
                    <Heart className={`h-5 w-5 ${isWishlisted ? 'fill-current' : ''}`} />
                  </motion.button>
                  <motion.button
                    onClick={handleShare}
                    className="p-3 rounded-full border bg-white border-gray-200 text-gray-400 hover:text-black transition-colors"
                    whileHover={{ scale: 1.1 }}
                    whileTap={{ scale: 0.9 }}
                  >
                    <Share2 className="h-5 w-5" />
                  </motion.button>
                </div>
              </div>

              {/* Rating */}
              <motion.div 
                className="flex items-center gap-3"
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.5 }}
              >
                <div className="flex items-center gap-1">
                  {[...Array(5)].map((_, i) => (
                    <Star 
                      key={i} 
                      className={`h-5 w-5 ${i < 4 ? 'text-yellow-400 fill-current' : 'text-gray-300'}`} 
                    />
                  ))}
                </div>
                <span className="text-gray-600">(4.2)</span>
                <span className="text-blue-600 hover:underline cursor-pointer">
                  24 đánh giá
                </span>
              </motion.div>
            </div>

            {/* Price */}
            <motion.div 
              className="space-y-3"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.6 }}
            >
              {product.hasActivePromotion && product.promotionName && (
                <div className="inline-flex items-center gap-2 bg-gradient-to-r from-purple-500 to-pink-500 text-white px-4 py-2 rounded-full text-sm font-medium">
                  <span className="text-lg">🎉</span>
                  {product.promotionName}
                </div>
              )}
              
              <div className="flex items-center gap-4">
                {product.hasActivePromotion && product.promotionPrice ? (
                  <>
                    <span className="text-3xl md:text-4xl font-bold text-purple-600">
                      {formatPrice(product.promotionPrice)}
                    </span>
                    <span className="text-xl text-gray-500 line-through">
                      {formatPrice(product.price)}
                    </span>
                  </>
                ) : product.salePrice ? (
                  <>
                    <span className="text-3xl md:text-4xl font-bold text-red-500">
                      {formatPrice(product.salePrice)}
                    </span>
                    <span className="text-xl text-gray-500 line-through">
                      {formatPrice(product.price)}
                    </span>
                  </>
                ) : (
                  <span className="text-3xl md:text-4xl font-bold text-gray-900">
                    {formatPrice(product.price)}
                  </span>
                )}
              </div>
              
              {product.hasActivePromotion && product.promotionDiscount && (
                <div className="text-green-600 font-medium">
                  💰 Tiết kiệm {formatPrice(product.promotionDiscount)} 
                  ({Math.round((product.promotionDiscount / product.price) * 100)}% OFF)
                </div>
              )}
            </motion.div>

            {/* Size Selection */}
            <motion.div 
              className="space-y-4"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.7 }}
            >
              <h3 className="text-lg font-semibold">Chọn size</h3>
              <div className="grid grid-cols-4 sm:grid-cols-6 gap-3">
                {product.inventories.map((inventory) => (
                  <motion.button
                    key={inventory.sizeId}
                    onClick={() => setSelectedSize(inventory.sizeId)}
                    disabled={inventory.quantity === 0}
                    className={`relative p-4 border rounded-xl text-center transition-all duration-300 ${
                      selectedSize === inventory.sizeId
                        ? 'border-transparent bg-gradient-to-r from-blue-600 to-purple-600 text-white shadow-lg'
                        : inventory.quantity === 0
                        ? 'border-gray-200 bg-gray-100 text-gray-400 cursor-not-allowed'
                        : 'border-gray-300 hover:border-blue-400 hover:shadow-md'
                    }`}
                    whileHover={inventory.quantity > 0 ? { scale: 1.05 } : {}}
                    whileTap={inventory.quantity > 0 ? { scale: 0.95 } : {}}
                  >
                    <div className="font-semibold">Size {inventory.sizeId}</div>
                    <div className="text-xs mt-1">
                      {inventory.quantity === 0 ? 'Hết hàng' : `Còn ${inventory.quantity}`}
                    </div>
                    {selectedSize === inventory.sizeId && (
                      <motion.div
                        className="absolute -top-1 -right-1 bg-green-500 text-white rounded-full p-1"
                        initial={{ scale: 0 }}
                        animate={{ scale: 1 }}
                      >
                        <Check className="h-3 w-3" />
                      </motion.div>
                    )}
                  </motion.button>
                ))}
              </div>
            </motion.div>

            {/* Quantity Selection */}
            <motion.div 
              className="space-y-4"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.8 }}
            >
              <h3 className="text-lg font-semibold">Số lượng</h3>
              <div className="flex items-center gap-4">
                <div className="flex items-center border border-gray-300 rounded-xl overflow-hidden">
                  <motion.button
                    onClick={() => setQuantity(Math.max(1, quantity - 1))}
                    className="p-3 hover:bg-gray-100 transition-colors"
                    whileTap={{ scale: 0.9 }}
                  >
                    <Minus className="h-4 w-4" />
                  </motion.button>
                  <span className="w-16 text-center font-semibold">{quantity}</span>
                  <motion.button
                    onClick={() => setQuantity(quantity + 1)}
                    className="p-3 hover:bg-gray-100 transition-colors"
                    whileTap={{ scale: 0.9 }}
                  >
                    <Plus className="h-4 w-4" />
                  </motion.button>
                </div>
              </div>
            </motion.div>

            {/* Action Buttons */}
            <motion.div 
              className="space-y-4"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.9 }}
            >
              <motion.button
                onClick={handleAddToCart}
                disabled={cartLoading || !selectedSize}
                className="w-full py-4 bg-gradient-to-r from-blue-600 via-purple-600 to-indigo-600 hover:from-blue-700 hover:via-purple-700 hover:to-indigo-700 text-white rounded-xl transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-3 text-lg font-semibold shadow-lg hover:shadow-xl"
                whileHover={!cartLoading && selectedSize ? { scale: 1.02 } : {}}
                whileTap={!cartLoading && selectedSize ? { scale: 0.98 } : {}}
              >
                {cartLoading ? (
                  <>
                    <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
                    Đang thêm...
                  </>
                ) : (
                  <>
                    <ShoppingCart className="h-5 w-5" />
                    Thêm vào giỏ hàng
                  </>
                )}
              </motion.button>

              {/* Benefits */}
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 text-sm">
                <div className="flex items-center gap-2 text-gray-600">
                  <Truck className="h-5 w-5 text-green-500" />
                  <span>Miễn phí vận chuyển</span>
                </div>
                <div className="flex items-center gap-2 text-gray-600">
                  <Shield className="h-5 w-5 text-blue-500" />
                  <span>Bảo hành chính hãng</span>
                </div>
                <div className="flex items-center gap-2 text-gray-600">
                  <RotateCcw className="h-5 w-5 text-purple-500" />
                  <span>Đổi trả 30 ngày</span>
                </div>
              </div>
            </motion.div>
          </motion.div>
        </div>

        {/* Product Details Tabs */}
        <motion.div 
          className="mt-16"
          initial={{ opacity: 0, y: 50 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 1 }}
        >
          <div className="bg-white rounded-2xl shadow-lg overflow-hidden">
            {/* Tab Headers */}
            <div className="flex border-b">
              {[
                { id: 'description', label: 'Mô tả sản phẩm', icon: Camera },
                { id: 'reviews', label: 'Đánh giá', icon: MessageSquare },
              ].map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`flex-1 flex items-center justify-center gap-2 py-4 px-6 font-medium transition-all duration-300 ${
                    activeTab === tab.id
                      ? 'bg-gradient-to-r from-blue-600 to-purple-600 text-white'
                      : 'text-gray-600 hover:text-black hover:bg-gray-50'
                  }`}
                >
                  <tab.icon className="h-4 w-4" />
                  {tab.label}
                </button>
              ))}
            </div>

            {/* Tab Content */}
            <div className="p-8">
              <AnimatePresence mode="wait">
                {activeTab === 'description' && (
                  <motion.div
                    key="description"
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, y: -20 }}
                    className="space-y-4"
                  >
                    <h3 className="text-xl font-semibold mb-4">Thông tin chi tiết</h3>
                    <p className="text-gray-600 leading-relaxed text-lg">
                      {product.description}
                    </p>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
                      <div>
                        <h4 className="font-semibold mb-2">Thông số kỹ thuật</h4>
                        <ul className="space-y-2 text-gray-600">
                          <li><strong>Thương hiệu:</strong> {product.brandName}</li>
                          <li><strong>Danh mục:</strong> {product.categoryName}</li>
                          <li><strong>Chất liệu:</strong> Da thật, cao su</li>
                          <li><strong>Xuất xứ:</strong> Việt Nam</li>
                        </ul>
                      </div>
                      <div>
                        <h4 className="font-semibold mb-2">Hướng dẫn bảo quản</h4>
                        <ul className="space-y-2 text-gray-600">
                          <li>• Vệ sinh nhẹ nhàng bằng khăn ẩm</li>
                          <li>• Để nơi khô ráo, thoáng mát</li>
                          <li>• Tránh ánh nắng trực tiếp</li>
                          <li>• Sử dụng cây giày để giữ form</li>
                        </ul>
                      </div>
                    </div>
                  </motion.div>
                )}

                {activeTab === 'reviews' && (
                  <motion.div
                    key="reviews"
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, y: -20 }}
                    className="space-y-6"
                  >
                    <div className="flex items-center justify-between">
                      <h3 className="text-xl font-semibold">Đánh giá từ khách hàng</h3>
                      <button className="px-4 py-2 bg-gradient-to-r from-blue-600 to-purple-600 text-white rounded-lg hover:from-blue-700 hover:to-purple-700 transition-colors">
                        Viết đánh giá
                      </button>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                      <div className="text-center p-6 bg-gray-50 rounded-xl">
                        <div className="text-4xl font-bold text-black">4.2</div>
                        <div className="flex justify-center mt-2">
                          {[...Array(5)].map((_, i) => (
                            <Star 
                              key={i} 
                              className={`h-5 w-5 ${i < 4 ? 'text-yellow-400 fill-current' : 'text-gray-300'}`} 
                            />
                          ))}
                        </div>
                        <div className="text-gray-600 mt-2">24 đánh giá</div>
                      </div>

                      <div className="md:col-span-2 space-y-4">
                        {mockReviews.map((review) => (
                          <div key={review.id} className="border border-gray-200 rounded-xl p-6">
                            <div className="flex items-center justify-between mb-3">
                              <div className="flex items-center gap-3">
                                <div className="w-10 h-10 bg-gradient-to-r from-blue-500 to-purple-500 rounded-full flex items-center justify-center text-white font-semibold">
                                  {review.user[0]}
                                </div>
                                <div>
                                  <div className="font-semibold">{review.user}</div>
                                  {review.verified && (
                                    <div className="text-sm text-green-600">✓ Đã mua hàng</div>
                                  )}
                                </div>
                              </div>
                              <div className="text-sm text-gray-500">{review.date}</div>
                            </div>
                            <div className="flex items-center gap-2 mb-3">
                              {[...Array(5)].map((_, i) => (
                                <Star 
                                  key={i} 
                                  className={`h-4 w-4 ${i < review.rating ? 'text-yellow-400 fill-current' : 'text-gray-300'}`} 
                                />
                              ))}
                            </div>
                            <p className="text-gray-700">{review.comment}</p>
                            <div className="flex items-center gap-4 mt-4 text-sm text-gray-500">
                              <button className="flex items-center gap-1 hover:text-black">
                                <ThumbsUp className="h-4 w-4" />
                                Hữu ích (12)
                              </button>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </motion.div>
                )}
              </AnimatePresence>
            </div>
          </div>
        </motion.div>

        {/* Related Products */}
        {relatedProducts.length > 0 && (
          <motion.div 
            className="mt-16"
            initial={{ opacity: 0, y: 50 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 1.2 }}
          >
            <h2 className="text-2xl font-bold mb-8">Sản phẩm liên quan</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
              {relatedProducts.map((relatedProduct, index) => (
                <motion.div
                  key={relatedProduct.id}
                  className="bg-white rounded-xl shadow-lg overflow-hidden hover:shadow-xl transition-all duration-300 cursor-pointer"
                  initial={{ opacity: 0, y: 30 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 1.3 + index * 0.1 }}
                  whileHover={{ scale: 1.05 }}
                  onClick={() => navigate(`/products/${relatedProduct.id}`)}
                >
                  <div className="aspect-square overflow-hidden">
                    <img
                      src={relatedProduct.mainImage}
                      alt={relatedProduct.name}
                      className="w-full h-full object-cover"
                    />
                  </div>
                  <div className="p-4">
                    <h3 className="font-semibold text-gray-900 mb-2 line-clamp-2">
                      {relatedProduct.name}
                    </h3>
                    <div className="flex items-center justify-between">
                      <span className="text-lg font-bold text-black">
                        {formatPrice(relatedProduct.price)}
                      </span>
                      <div className="flex items-center gap-1">
                        <Star className="h-4 w-4 text-yellow-400 fill-current" />
                        <span className="text-sm text-gray-600">4.2</span>
                      </div>
                    </div>
                  </div>
                </motion.div>
              ))}
            </div>
          </motion.div>
        )}
      </div>

      {/* Zoom Modal */}
      <AnimatePresence>
        {isZoomed && (
          <motion.div
            className="fixed inset-0 bg-black/90 z-50 flex items-center justify-center p-4"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={() => setIsZoomed(false)}
          >
            <motion.div
              className="relative max-w-4xl max-h-full"
              initial={{ scale: 0.8 }}
              animate={{ scale: 1 }}
              exit={{ scale: 0.8 }}
              onClick={(e) => e.stopPropagation()}
            >
              <img
                src={allImages[currentImageIndex]}
                alt={product.name}
                className="max-w-full max-h-full object-contain"
              />
              <button
                onClick={() => setIsZoomed(false)}
                className="absolute top-4 right-4 text-white bg-black/50 rounded-full p-2 hover:bg-black/70"
              >
                <ChevronLeft className="h-6 w-6 rotate-45" />
              </button>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default ProductDetail; 