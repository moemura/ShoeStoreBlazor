namespace WebApp.Models.Mapping
{
    public static class InventoryMapping
    {
        public static InventoryDto ToDto(this Inventory inventory)
        {
            if (inventory == null) return null;
            return new InventoryDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                SizeId = inventory.SizeId,
                Quantity = inventory.Quantity,
            };
        }
        public static Inventory ToEntity(this InventoryDto dto)
        {
            if (dto == null) return null;
            return new Inventory
            {
                Id = dto.Id,
                ProductId = dto.ProductId,
                SizeId = dto.SizeId,
                Quantity = dto.Quantity,
            };
        }
    }
}
