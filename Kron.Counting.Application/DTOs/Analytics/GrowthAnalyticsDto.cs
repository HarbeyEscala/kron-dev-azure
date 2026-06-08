using System;
using System.Collections.Generic;
using System.Text;

namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class GrowthAnalyticsDto
{
    public int CurrentPeriodVisitors { get; set; }

    public int PreviousPeriodVisitors { get; set; }

    public decimal GrowthPercentage { get; set; }
}