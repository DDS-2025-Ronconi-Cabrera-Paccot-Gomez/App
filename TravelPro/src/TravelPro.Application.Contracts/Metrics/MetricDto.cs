using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelPro.Metrics.Dtos
{
    public class ApiMetricSummaryDto
    {
        public string ApiName { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessRequests { get; set; }
        public int FailedRequests { get; set; }
        public double AverageDurationMs { get; set; }
    }
}