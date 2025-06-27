import { useEffect } from 'react';

const MenuSidebar = ({ isOpen, onClose, filters, onFilterChange, categories, brands }) => {
  useEffect(() => {
    const handleEscKey = (e) => {
      if (e.key === 'Escape') onClose();
    };

    if (isOpen) {
      document.addEventListener('keydown', handleEscKey);
    }

    return () => {
      document.removeEventListener('keydown', handleEscKey);
    };
  }, [isOpen, onClose]);

  return (
    <>
      <div 
        className={`fixed md:absolute left-0 top-0 h-full bg-white shadow-sm transition-all duration-300 ease-in-out z-30 
          ${isOpen ? 'w-[260px] rounded-lg border' : 'w-[70px] rounded-lg border'} mt-28 md:mt-0`}
      >
        <div className="flex flex-col h-full">
          {/* Header */}
          <div className="flex items-center justify-between p-4 border-b">
            <h2 className={`text-lg font-semibold ${!isOpen && 'hidden'}`}>Danh mục sản phẩm</h2>
            <button
              onClick={() => onClose(!isOpen)}
              className="p-2 hover:bg-gray-100 rounded-full ml-auto"
              title={isOpen ? "Thu gọn menu" : "Mở rộng menu"}
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                className={`h-5 w-5 transition-transform ${isOpen ? 'rotate-0' : 'rotate-180'}`}
              >
                <path d="m15 18-6-6 6-6"/>
              </svg>
            </button>
          </div>

          {/* Menu Content */}
          <div className="flex-1 overflow-y-auto">
            {/* Categories Menu */}
            <div className={`${isOpen ? 'p-4' : 'p-2'} border-b`}>
              {isOpen && <h3 className="font-semibold text-lg mb-3">Danh mục</h3>}
              <div className="space-y-1">
                {categories.map((category) => (
                  <button
                    key={category.id}
                    onClick={() => onFilterChange('categoryId', category.id === filters.categoryId ? '' : category.id)}
                    className={`w-full flex items-center gap-3 ${isOpen ? 'p-3' : 'p-2'} rounded-lg transition-colors ${
                      filters.categoryId === category.id 
                        ? 'bg-gradient-to-r from-blue-600 to-purple-600 text-white' 
                        : 'hover:bg-gray-100'
                    }`}
                    title={!isOpen ? category.name : undefined}
                  >
                    <div className={`${isOpen ? 'w-8 h-8' : 'w-10 h-10'} flex items-center justify-center rounded-full bg-gray-100 overflow-hidden`}>
                      <img
                        src={`https://localhost:5001/images/categories/${category.image}`}
                        alt={category.name}
                        className={`${isOpen ? 'w-6 h-6' : 'w-8 h-8'} object-cover`}
                      />
                    </div>
                    {isOpen && (
                      <>
                        <span className="flex-1 text-left">{category.name}</span>
                        {filters.categoryId === category.id && (
                          <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="24"
                            height="24"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            className="w-5 h-5"
                          >
                            <polyline points="20 6 9 17 4 12"/>
                          </svg>
                        )}
                      </>
                    )}
                  </button>
                ))}
              </div>
            </div>

            {/* Brands Menu */}
            <div className={`${isOpen ? 'p-4' : 'p-2'}`}>
              {isOpen && <h3 className="font-semibold text-lg mb-3">Thương hiệu</h3>}
              <div className="space-y-1">
                {brands.map((brand) => (
                  <button
                    key={brand.id}
                    onClick={() => onFilterChange('brandId', brand.id === filters.brandId ? '' : brand.id)}
                    className={`w-full flex items-center gap-3 ${isOpen ? 'p-3' : 'p-2'} rounded-lg transition-colors ${
                      filters.brandId === brand.id 
                        ? 'bg-gradient-to-r from-blue-600 to-purple-600 text-white' 
                        : 'hover:bg-gray-100'
                    }`}
                    title={!isOpen ? brand.name : undefined}
                  >
                    <div className={`${isOpen ? 'w-8 h-8' : 'w-10 h-10'} flex items-center justify-center rounded-full bg-gray-100`}>
                      <img
                        src={`https://localhost:5001/images/brands/${brand.logo}` || `https://ui-avatars.com/api/?name=${brand.name}&background=random`}
                        alt={brand.name}
                        className={`${isOpen ? 'w-6 h-6' : 'w-8 h-8'} object-contain rounded-full`}
                      />
                    </div>
                    {isOpen && (
                      <>
                        <span className="flex-1 text-left">{brand.name}</span>
                        {filters.brandId === brand.id && (
                          <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="24"
                            height="24"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            className="w-5 h-5"
                          >
                            <polyline points="20 6 9 17 4 12"/>
                          </svg>
                        )}
                      </>
                    )}
                  </button>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Mobile Backdrop */}
      {isOpen && (
        <div 
          className="fixed inset-0 bg-black/50 z-20 md:hidden" 
          onClick={onClose}
        />
      )}
    </>
  );
};

export default MenuSidebar; 