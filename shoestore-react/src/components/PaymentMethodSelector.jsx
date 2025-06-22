import React from 'react';
import { paymentService } from '../services/paymentService';

const PaymentMethodSelector = ({ 
  selectedPaymentMethod, 
  onPaymentMethodChange, 
  className = "" 
}) => {
  const paymentMethods = paymentService.getPaymentMethods();

  return (
    <div className={`payment-method-selector ${className}`}>
      <h3 className="text-lg font-semibold text-gray-800 mb-4">
        Phương thức thanh toán
      </h3>
      
      <div className="space-y-3">
        {paymentMethods.map((method) => (
          <div
            key={method.id}
            className={`payment-method-option border rounded-lg p-4 cursor-pointer transition-all duration-200 ${
              selectedPaymentMethod === method.id
                ? 'border-blue-500 bg-blue-50 shadow-md'
                : 'border-gray-200 bg-white hover:border-gray-300 hover:shadow-sm'
            }`}
            onClick={() => onPaymentMethodChange(method.id)}
          >
            <div className="flex items-center">
              <input
                type="radio"
                id={`payment-${method.id}`}
                name="paymentMethod"
                value={method.id}
                checked={selectedPaymentMethod === method.id}
                onChange={() => onPaymentMethodChange(method.id)}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
              />
              
              <label
                htmlFor={`payment-${method.id}`}
                className="ml-3 flex-1 cursor-pointer"
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-3">
                    <span className="text-2xl">{method.icon}</span>
                    <div>
                      <div className="font-medium text-gray-900">
                        {method.displayName}
                      </div>
                      <div className="text-sm text-gray-500">
                        {method.description}
                      </div>
                    </div>
                  </div>
                  
                  {method.id !== 0 && (
                    <div className="text-xs bg-green-100 text-green-800 px-2 py-1 rounded-full">
                      Thanh toán ngay
                    </div>
                  )}
                </div>
              </label>
            </div>
          </div>
        ))}
      </div>
      
      {selectedPaymentMethod !== 0 && (
        <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
          <div className="flex items-start space-x-2">
            <div className="text-blue-500 text-sm">ℹ️</div>
            <div className="text-sm text-blue-800">
              <strong>Lưu ý:</strong> Bạn sẽ được chuyển đến trang thanh toán của {paymentService.formatPaymentMethod(selectedPaymentMethod)} để hoàn tất giao dịch.
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PaymentMethodSelector; 