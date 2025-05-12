import { useState } from 'react';

const FilterSection = ({ filters, onFilterChange, priceInputs, onPriceChange, onClearFilters }) => {
  const [isExpanded, setIsExpanded] = useState(false);

  const hasActiveFilters = filters.categoryId || filters.brandId || filters.minPrice || filters.maxPrice || filters.sortBy !== 'newest';

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  return (
    <div className="bg-white border rounded-lg shadow-sm">
      <div className="p-4">
        <div className="flex flex-col md:flex-row md:items-center gap-4">
          {/* Price Range Filter */}
          <div className="flex-1">
            <div className="flex items-center gap-4">
              <div className="flex-1">
                <div className="flex items-center gap-2">
                  <label className="text-sm text-gray-600 whitespace-nowrap">Giá từ:</label>
                  <div className="relative flex-1">
                    <input
                      type="number"
                      value={priceInputs.minPrice}
                      onChange={(e) => onPriceChange('min', e.target.value)}
                      placeholder="0"
                      className="w-full px-3 py-2 border rounded-md pl-8"
                    />
                    <span className="absolute left-3 top-2 text-gray-500">₫</span>
                  </div>
                </div>
              </div>
              <div className="flex-1">
                <div className="flex items-center gap-2">
                  <label className="text-sm text-gray-600 whitespace-nowrap">đến:</label>
                  <div className="relative flex-1">
                    <input
                      type="number"
                      value={priceInputs.maxPrice}
                      onChange={(e) => onPriceChange('max', e.target.value)}
                      placeholder="1000000"
                      className="w-full px-3 py-2 border rounded-md pl-8"
                    />
                    <span className="absolute left-3 top-2 text-gray-500">₫</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Sort Filter */}
          <div className="flex items-center gap-2">
            <label className="text-sm text-gray-600 whitespace-nowrap">Sắp xếp:</label>
            <select
              value={filters.sortBy}
              onChange={(e) => onFilterChange('sortBy', e.target.value)}
              className="px-3 py-2 border rounded-md bg-white min-w-[160px]"
            >
              <option value="newest">Mới nhất</option>
              <option value="price_asc">Giá tăng dần</option>
              <option value="price_desc">Giá giảm dần</option>
              <option value="name_asc">Tên A-Z</option>
              <option value="name_desc">Tên Z-A</option>
            </select>
          </div>

          {/* Clear Filters Button */}
          <button
            onClick={onClearFilters}
            className={`px-4 py-2 text-sm border rounded-md transition-colors whitespace-nowrap ${
              hasActiveFilters 
                ? 'text-red-600 border-red-200 hover:bg-red-50 hover:text-red-700' 
                : 'text-gray-600 border-gray-200 hover:bg-gray-50 hover:text-gray-900'
            }`}
          >
            Xóa bộ lọc
          </button>

          {/* Mobile Expand Button */}
          <button
            onClick={() => setIsExpanded(!isExpanded)}
            className="p-2 hover:bg-gray-100 rounded-full md:hidden"
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
              className={`w-5 h-5 transition-transform ${isExpanded ? 'rotate-180' : ''}`}
            >
              <path d="m18 15-6-6-6 6"/>
            </svg>
          </button>
        </div>
      </div>
    </div>
  );
};

export default FilterSection; 