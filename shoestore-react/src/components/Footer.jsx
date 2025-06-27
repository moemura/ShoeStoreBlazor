import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Mail, Phone, MapPin, Facebook, Instagram, Twitter, Youtube, ArrowRight, Heart } from 'lucide-react';

const Footer = () => {
  const socialLinks = [
    { icon: Facebook, href: "https://facebook.com", label: "Facebook", color: "hover:text-blue-600" },
    { icon: Instagram, href: "https://instagram.com", label: "Instagram", color: "hover:text-pink-600" },
    { icon: Twitter, href: "https://twitter.com", label: "Twitter", color: "hover:text-blue-400" },
    { icon: Youtube, href: "https://youtube.com", label: "YouTube", color: "hover:text-red-600" }
  ];

  const quickLinks = [
    { to: "/products", label: "Sản phẩm" },
    { to: "/promotions", label: "Khuyến mãi" },
    { to: "/about", label: "Về chúng tôi" },
    { to: "/contact", label: "Liên hệ" }
  ];

  const customerServiceLinks = [
    { to: "/faq", label: "Câu hỏi thường gặp" },
    { to: "/shipping", label: "Chính sách vận chuyển" },
    { to: "/returns", label: "Đổi trả & Hoàn tiền" },
    { to: "/privacy", label: "Chính sách bảo mật" },
    { to: "/terms", label: "Điều khoản sử dụng" }
  ];

  const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: {
        staggerChildren: 0.1
      }
    }
  };

  const itemVariants = {
    hidden: { opacity: 0, y: 20 },
    visible: {
      opacity: 1,
      y: 0,
      transition: { duration: 0.6 }
    }
  };

  return (
    <footer className="relative bg-gradient-to-br from-gray-900 via-gray-800 to-black text-white overflow-hidden">
      {/* Background Elements */}
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute -top-40 -right-40 w-80 h-80 bg-purple-600/10 rounded-full blur-3xl"></div>
        <div className="absolute -bottom-40 -left-40 w-80 h-80 bg-pink-600/10 rounded-full blur-3xl"></div>
        <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-96 h-96 bg-gradient-to-r from-purple-600/5 to-pink-600/5 rounded-full blur-3xl"></div>
      </div>

      {/* Newsletter Section */}
      <div className="relative border-b border-gray-700">
        <div className="container mx-auto px-4 py-12">
          <motion.div
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            variants={containerVariants}
            className="max-w-4xl mx-auto text-center"
          >
            <motion.div variants={itemVariants}>
              <h3 className="text-3xl font-bold mb-4 bg-gradient-to-r from-purple-400 to-pink-400 bg-clip-text text-transparent">
                Đăng ký nhận tin khuyến mãi
              </h3>
              <p className="text-gray-300 text-lg mb-8">
                Nhận thông tin về sản phẩm mới và ưu đãi độc quyền trước tiên
              </p>
            </motion.div>
            
            <motion.form variants={itemVariants} className="flex flex-col sm:flex-row gap-4 max-w-md mx-auto">
              <div className="flex-1">
                <input
                  type="email"
                  placeholder="Nhập email của bạn..."
                  className="w-full px-6 py-4 rounded-full bg-gray-800/50 border border-gray-600 focus:border-purple-500 focus:outline-none focus:ring-2 focus:ring-purple-500/20 transition-all text-white placeholder-gray-400"
                />
              </div>
              <button
                type="submit"
                className="px-8 py-4 bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 rounded-full font-semibold transition-all hover:scale-105 flex items-center justify-center gap-2"
              >
                Đăng ký
                <ArrowRight className="h-4 w-4" />
              </button>
            </motion.form>
          </motion.div>
        </div>
      </div>

      {/* Main Footer Content */}
      <div className="relative container mx-auto px-4 py-16">
        <motion.div
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          variants={containerVariants}
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8"
        >
          {/* About Section */}
          <motion.div variants={itemVariants} className="lg:col-span-1">
            <div className="flex items-center space-x-3 mb-6">
              <div className="relative">
                <div className="absolute inset-0 bg-gradient-to-r from-purple-600 to-pink-600 rounded-lg blur opacity-40"></div>
                <img src="/logo.png" alt="Logo" className="relative h-12 w-12 rounded-lg" />
              </div>
              <div>
                <h3 className="text-2xl font-bold bg-gradient-to-r from-purple-400 to-pink-400 bg-clip-text text-transparent">
                  ShoeStore
                </h3>
                <p className="text-sm text-gray-400">Fashion & Style</p>
              </div>
            </div>
            
            <p className="text-gray-300 mb-6 leading-relaxed">
              Chuyên cung cấp giày dép chất lượng cao với thiết kế thời trang, 
              phù hợp với mọi phong cách và xu hướng hiện đại.
            </p>

            {/* Social Media */}
            <div>
              <h4 className="font-semibold mb-4 text-white">Kết nối với chúng tôi</h4>
              <div className="flex space-x-4">
                {socialLinks.map(({ icon: Icon, href, label, color }) => (
                  <motion.a
                    key={label}
                    href={href}
                    target="_blank"
                    rel="noopener noreferrer"
                    className={`p-3 bg-gray-800/50 rounded-full text-gray-400 ${color} transition-all hover:scale-110 hover:bg-gray-700/50`}
                    whileHover={{ y: -2 }}
                    whileTap={{ scale: 0.95 }}
                  >
                    <Icon className="h-5 w-5" />
                  </motion.a>
                ))}
              </div>
            </div>
          </motion.div>

          {/* Quick Links */}
          <motion.div variants={itemVariants}>
            <h3 className="font-bold text-xl mb-6 text-white">Liên kết nhanh</h3>
            <ul className="space-y-3">
              {quickLinks.map(({ to, label }) => (
                <li key={to}>
                  <Link
                    to={to}
                    className="text-gray-300 hover:text-white transition-colors flex items-center group"
                  >
                    <ArrowRight className="h-4 w-4 mr-2 opacity-0 -translate-x-2 group-hover:opacity-100 group-hover:translate-x-0 transition-all text-purple-400" />
                    <span className="group-hover:translate-x-1 transition-transform">{label}</span>
                  </Link>
                </li>
              ))}
            </ul>
          </motion.div>

          {/* Customer Service */}
          <motion.div variants={itemVariants}>
            <h3 className="font-bold text-xl mb-6 text-white">Hỗ trợ khách hàng</h3>
            <ul className="space-y-3">
              {customerServiceLinks.map(({ to, label }) => (
                <li key={to}>
                  <Link
                    to={to}
                    className="text-gray-300 hover:text-white transition-colors flex items-center group"
                  >
                    <ArrowRight className="h-4 w-4 mr-2 opacity-0 -translate-x-2 group-hover:opacity-100 group-hover:translate-x-0 transition-all text-purple-400" />
                    <span className="group-hover:translate-x-1 transition-transform">{label}</span>
                  </Link>
                </li>
              ))}
            </ul>
          </motion.div>

          {/* Contact Info */}
          <motion.div variants={itemVariants}>
            <h3 className="font-bold text-xl mb-6 text-white">Thông tin liên hệ</h3>
            <ul className="space-y-4">
              <li className="flex items-start space-x-3 group">
                <div className="p-2 bg-gradient-to-r from-purple-600/20 to-pink-600/20 rounded-lg group-hover:from-purple-600/30 group-hover:to-pink-600/30 transition-colors">
                  <Phone className="h-4 w-4 text-purple-400" />
                </div>
                <div>
                  <p className="text-gray-400 text-sm">Hotline</p>
                  <p className="text-white font-medium">0123 456 789</p>
                </div>
              </li>
              
              <li className="flex items-start space-x-3 group">
                <div className="p-2 bg-gradient-to-r from-purple-600/20 to-pink-600/20 rounded-lg group-hover:from-purple-600/30 group-hover:to-pink-600/30 transition-colors">
                  <Mail className="h-4 w-4 text-purple-400" />
                </div>
                <div>
                  <p className="text-gray-400 text-sm">Email</p>
                  <p className="text-white font-medium">contact@shoestore.com</p>
                </div>
              </li>
              
              <li className="flex items-start space-x-3 group">
                <div className="p-2 bg-gradient-to-r from-purple-600/20 to-pink-600/20 rounded-lg group-hover:from-purple-600/30 group-hover:to-pink-600/30 transition-colors">
                  <MapPin className="h-4 w-4 text-purple-400" />
                </div>
                <div>
                  <p className="text-gray-400 text-sm">Địa chỉ</p>
                  <p className="text-white font-medium">123 Đường ABC, Quận XYZ, TP.HCM</p>
                </div>
              </li>
            </ul>

            {/* Store Hours */}
            <div className="mt-6 p-4 bg-gradient-to-r from-purple-600/10 to-pink-600/10 rounded-lg border border-purple-600/20">
              <h4 className="font-semibold text-white mb-2">Giờ làm việc</h4>
              <p className="text-gray-300 text-sm">Thứ 2 - Chủ nhật: 8:00 - 22:00</p>
            </div>
          </motion.div>
        </motion.div>
      </div>

      {/* Bottom Bar */}
      <div className="relative border-t border-gray-700">
        <div className="container mx-auto px-4 py-6">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
            className="flex flex-col md:flex-row justify-between items-center space-y-4 md:space-y-0"
          >
            <div className="flex items-center space-x-2 text-gray-400">
              <span>&copy; {new Date().getFullYear()} ShoeStore. All rights reserved.</span>
            </div>

            <div className="flex items-center space-x-6 text-sm text-gray-400">
              <Link to="/privacy" className="hover:text-white transition-colors">
                Chính sách bảo mật
              </Link>
              <Link to="/terms" className="hover:text-white transition-colors">
                Điều khoản
              </Link>
              <Link to="/sitemap" className="hover:text-white transition-colors">
                Sơ đồ trang web
              </Link>
            </div>

            <div className="flex items-center space-x-2 text-gray-400">
              <span>Made with</span>
              <Heart className="h-4 w-4 text-red-500 animate-pulse" />
              <span>in Vietnam</span>
            </div>
          </motion.div>
        </div>
      </div>
    </footer>
  );
};

export default Footer; 