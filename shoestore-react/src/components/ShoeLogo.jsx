import React from 'react';

const ShoeLogo = ({ className = "h-10 w-10", color = "currentColor" }) => {
  return (
    <svg 
      viewBox="0 0 100 100" 
      className={className}
      fill="none" 
      xmlns="http://www.w3.org/2000/svg"
    >
      {/* Gradient definitions */}
      <defs>
        <linearGradient id="shoeGradient" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#8B5CF6" />
          <stop offset="50%" stopColor="#EC4899" />
          <stop offset="100%" stopColor="#F59E0B" />
        </linearGradient>
        <linearGradient id="soleGradient" x1="0%" y1="0%" x2="100%" y2="0%">
          <stop offset="0%" stopColor="#374151" />
          <stop offset="100%" stopColor="#6B7280" />
        </linearGradient>
      </defs>
      
      {/* Shoe sole */}
      <path 
        d="M15 70 Q15 75 20 75 L75 75 Q85 75 85 65 Q85 60 80 60 L25 60 Q15 60 15 70 Z" 
        fill="url(#soleGradient)"
      />
      
      {/* Main shoe body */}
      <path 
        d="M20 60 Q20 45 35 35 L70 35 Q80 35 80 45 L80 55 Q80 60 75 60 L25 60 Q20 60 20 55 Z" 
        fill="url(#shoeGradient)"
      />
      
      {/* Shoe laces area */}
      <path 
        d="M30 45 Q30 40 35 40 L65 40 Q70 40 70 45 L70 50 Q70 55 65 55 L35 55 Q30 55 30 50 Z" 
        fill="white" 
        fillOpacity="0.3"
      />
      
      {/* Lace holes */}
      <circle cx="38" cy="47" r="2" fill="#374151" />
      <circle cx="48" cy="47" r="2" fill="#374151" />
      <circle cx="58" cy="47" r="2" fill="#374151" />
      
      {/* Nike-style swoosh */}
      <path 
        d="M25 50 Q40 45 55 50 Q45 52 25 50 Z" 
        fill="white" 
        fillOpacity="0.8"
      />
      
      {/* Heel counter */}
      <path 
        d="M70 35 Q75 35 75 40 L75 50 Q75 55 70 55 L70 45 Q70 35 70 35 Z" 
        fill="white" 
        fillOpacity="0.2"
      />
      
      {/* Toe cap highlight */}
      <ellipse 
        cx="40" 
        cy="45" 
        rx="8" 
        ry="5" 
        fill="white" 
        fillOpacity="0.3"
      />
    </svg>
  );
};

export default ShoeLogo; 