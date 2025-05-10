export function Footer() {
  return (
    <footer className="border-t">
      <div className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          <div>
            <h3 className="font-bold text-lg mb-4">Về chúng tôi</h3>
            <p className="text-sm text-muted-foreground">
              ShoeStore - Cửa hàng giày dép uy tín, chất lượng với đa dạng mẫu mã và thương hiệu.
            </p>
          </div>
          
          <div>
            <h3 className="font-bold text-lg mb-4">Liên kết</h3>
            <ul className="space-y-2">
              <li>
                <a href="/products" className="text-sm text-muted-foreground hover:text-primary">
                  Sản phẩm
                </a>
              </li>
              <li>
                <a href="/categories" className="text-sm text-muted-foreground hover:text-primary">
                  Danh mục
                </a>
              </li>
              <li>
                <a href="/brands" className="text-sm text-muted-foreground hover:text-primary">
                  Thương hiệu
                </a>
              </li>
            </ul>
          </div>
          
          <div>
            <h3 className="font-bold text-lg mb-4">Liên hệ</h3>
            <ul className="space-y-2">
              <li className="text-sm text-muted-foreground">
                Địa chỉ: 123 Đường ABC, Quận XYZ, TP.HCM
              </li>
              <li className="text-sm text-muted-foreground">
                Điện thoại: (028) 1234 5678
              </li>
              <li className="text-sm text-muted-foreground">
                Email: contact@shoestore.com
              </li>
            </ul>
          </div>
          
          <div>
            <h3 className="font-bold text-lg mb-4">Theo dõi chúng tôi</h3>
            <div className="flex space-x-4">
              <a href="#" className="text-muted-foreground hover:text-primary">
                Facebook
              </a>
              <a href="#" className="text-muted-foreground hover:text-primary">
                Instagram
              </a>
              <a href="#" className="text-muted-foreground hover:text-primary">
                Twitter
              </a>
            </div>
          </div>
        </div>
        
        <div className="border-t mt-8 pt-8 text-center text-sm text-muted-foreground">
          © 2024 ShoeStore. Tất cả quyền được bảo lưu.
        </div>
      </div>
    </footer>
  );
} 