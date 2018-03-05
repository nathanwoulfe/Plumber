using System.Threading.Tasks;
using Workflow.Models;

namespace Workflow.Services.Interfaces
{
    public interface IImportExportService
    {
        Task<ImportExportModel> Export();
        Task<bool> Import(ImportExportModel model);
    }
}