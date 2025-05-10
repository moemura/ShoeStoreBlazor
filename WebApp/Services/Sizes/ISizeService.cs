namespace WebApp.Services.Sizes
{
    public interface ISizeService
    {
        Task<IEnumerable<Size>> GetAll();
        Task<Size> Create(Size sizeDto);
    }
} 