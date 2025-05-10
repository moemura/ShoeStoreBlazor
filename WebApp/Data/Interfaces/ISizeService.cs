namespace WebApp.Data.Interfaces
{
    public interface ISizeService
    {
        Task<IEnumerable<Size>> GetAll();
        Task<Size> Create(Size sizeDto);
    }
} 