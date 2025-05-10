using Microsoft.AspNetCore.Components.Forms;

namespace WebApp.Data.Interfaces
{
    public interface IImageStorageService
    {
        Task<string> UploadImageAsync(IBrowserFile file);
        Task<List<string>> UploadImagesAsync(List<IBrowserFile> files);
    }
} 