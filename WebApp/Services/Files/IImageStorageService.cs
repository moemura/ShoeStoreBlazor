using Microsoft.AspNetCore.Components.Forms;

namespace WebApp.Services.Files
{
    public interface IImageStorageService
    {
        Task<string> UploadImageAsync(IBrowserFile file);
        Task<List<string>> UploadImagesAsync(List<IBrowserFile> files);
    }
} 