using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelPro.Metrics.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace TravelPro.Metrics
{
    public class MetricAppService : ApplicationService, IMetricAppService
    {
        private readonly IRepository<ApiMetric, Guid> _metricRepository;

        public MetricAppService(IRepository<ApiMetric, Guid> metricRepository)
        {
            _metricRepository = metricRepository;
        }

        public async Task<List<ApiMetricSummaryDto>> GetDashboardAsync()
        {
            var query = await _metricRepository.GetQueryableAsync();

            var summaryQuery = query
                .GroupBy(m => m.ApiName)
                .Select(g => new ApiMetricSummaryDto
                {
                    ApiName = g.Key,
                    TotalRequests = g.Count(),
                    SuccessRequests = g.Count(x => x.StatusCode >= 200 && x.StatusCode < 300),
                    FailedRequests = g.Count(x => x.StatusCode >= 400),
                    AverageDurationMs = Math.Round(g.Average(x => x.DurationMs), 2)
                });

            return await AsyncExecuter.ToListAsync(summaryQuery);
        }
    }
}