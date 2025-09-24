using Sinco.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinco.Server.Repositories.AttachedFile
{
    public interface IAttachedFileRepository
    {
        Task<bool> UploadFileAsync(FileAttachment file);
        Task<IEnumerable<FileAttachment>> GetFilesAsync(string controller, string sysKey);
        Task<FileAttachment> GetFileAsync(string controller, string sysKey, string fileName);
        Task<bool> DeleteFileAsync(string controller, string sysKey, string fileName);
    }
}