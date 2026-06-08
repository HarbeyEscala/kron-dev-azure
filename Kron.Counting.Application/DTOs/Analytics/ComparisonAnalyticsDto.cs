using System;
using System.Collections.Generic;
using System.Text;


namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class ComparisonAnalyticsDto
{
    public int CurrentPeriodVisitors { get; set; }

    public int PreviousPeriodVisitors { get; set; }

    public int CurrentPeriodExits { get; set; }

    public int PreviousPeriodExits { get; set; }

    public decimal VisitorGrowthPercentage { get; set; }

    public decimal ExitGrowthPercentage { get; set; }
}