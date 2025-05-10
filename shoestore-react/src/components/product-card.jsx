import { Link } from "react-router-dom";
import { Button } from "./ui/button";
import { ShoppingCart } from "lucide-react";

export function ProductCard({ product }) {
  return (
    <div className="group relative">
      <div className="aspect-square w-full overflow-hidden rounded-lg bg-gray-100">
        <img
          src={product.mainImage}
          alt={product.name}
          className="h-full w-full object-cover object-center group-hover:opacity-75"
        />
      </div>
      <div className="mt-4 flex justify-between">
        <div>
          <h3 className="text-sm font-medium text-gray-900">
            <Link to={`/products/${product.id}`}>
              <span aria-hidden="true" className="absolute inset-0" />
              {product.name}
            </Link>
          </h3>
          <p className="mt-1 text-sm text-gray-500">{product.brandName}</p>
        </div>
        <div className="text-right">
          {product.salePrice ? (
            <>
              <p className="text-sm font-medium text-gray-900">{product.salePrice.toLocaleString()}đ</p>
              <p className="text-sm text-gray-500 line-through">{product.price.toLocaleString()}đ</p>
            </>
          ) : (
            <p className="text-sm font-medium text-gray-900">{product.price.toLocaleString()}đ</p>
          )}
        </div>
      </div>
      <div className="mt-4">
        <Button className="w-full" size="sm">
          <ShoppingCart className="mr-2 h-4 w-4" />
          Thêm vào giỏ
        </Button>
      </div>
    </div>
  );
} 