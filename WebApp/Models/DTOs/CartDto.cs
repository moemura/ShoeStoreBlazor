using System;
using System.Collections.Generic;

namespace WebApp.Models.DTOs
{
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 