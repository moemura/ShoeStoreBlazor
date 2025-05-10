import { Link } from "react-router-dom";
import { cn } from "../../lib/utils";

export function Sidebar({ categories, brands, className }) {
  return (
    <div className={cn("space-y-6", className)}>
      {/* Categories Section */}
      <div>
        <h3 className="text-lg font-semibold mb-4">Danh mục</h3>
        <nav className="space-y-2">
          {categories.map((category) => (
            <Link
              key={category.id}
              to={`/categories/${category.id}`}
              className="block px-3 py-2 text-sm rounded-md hover:bg-accent hover:text-accent-foreground"
            >
              {category.name}
            </Link>
          ))}
        </nav>
      </div>

      {/* Brands Section */}
      <div>
        <h3 className="text-lg font-semibold mb-4">Thương hiệu</h3>
        <nav className="space-y-2">
          {brands.map((brand) => (
            <Link
              key={brand.id}
              to={`/brands/${brand.id}`}
              className="block px-3 py-2 text-sm rounded-md hover:bg-accent hover:text-accent-foreground"
            >
              {brand.name}
            </Link>
          ))}
        </nav>
      </div>
    </div>
  );
} 