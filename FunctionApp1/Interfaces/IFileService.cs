using Microsoft.AspNetCore.Http;

namespace FCGuiaProducaoNotificacao.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFile(string containerName, IFormFile file);

        Task DeleteFile(string containerName, string FileRoute);

        Task<string> EditFile(string containerName, IFormFile file, string path);
    }
}
