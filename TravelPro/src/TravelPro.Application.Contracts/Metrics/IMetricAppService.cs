using System.Collections.Generic;
using System.Threading.Tasks;
using TravelPro.Metrics.Dtos;
using Volo.Abp.Application.Services;

namespace TravelPro.Metrics
{
    public interface IMetricAppService : IApplicationService
    {
        Task<List<ApiMetricSummaryDto>> GetDashboardAsync();
    }
}