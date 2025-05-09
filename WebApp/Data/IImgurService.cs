using Microsoft.AspNetCore.Components.Forms;

namespace WebApp.Data
{
    public interface IImgurService
    {
        Task<string> UploadImageAsync(IBrowserFile file);
        Task<List<string>> UploadImagesAsync(List<IBrowserFile> files);
    }
} 