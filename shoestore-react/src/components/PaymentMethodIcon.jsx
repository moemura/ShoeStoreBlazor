import orderService from '../services/orderService';

const PaymentMethodIcon = ({ method, showIcon = true, className = "" }) => {
  const methodText = orderService.formatPaymentMethod(method);

  const getIcon = (method) => {
    switch (method) {
      case 0: // COD
        return '💵';
      case 1: // Bank Transfer
        return '🏧';
      case 2: // Credit Card
        return '💳';
      case 3: // MoMo
        return '📱';
      case 4: // VnPay
        return '🏦';
      case 5: // ZaloPay
        return '💰';
      case 6: // PayPal
        return '🅿️';
      default:
        return '💳';
    }
  };

  return (
    <span className={`inline-flex items-center ${className}`}>
      {showIcon && <span className="mr-2">{getIcon(method)}</span>}
      {methodText}
    </span>
  );
};

export default PaymentMethodIcon; 