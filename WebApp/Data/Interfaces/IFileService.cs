using Microsoft.AspNetCore.Components.Forms;

namespace WebApp.Data.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IBrowserFile file, string folder);
        Task DeleteFileAsync(string filePath);
        string GetFileUrl(string fileName, string folder);
    }
} 