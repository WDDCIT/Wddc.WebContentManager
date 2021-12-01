using System.Threading.Tasks;

namespace Wddc.WebApp.Services.Finance
{
    public interface IReturnsService
    {
        Task DistributeDailyReturns(string salesBatch);
        Task ExcelImportReturns();
    }
}