using System;
using System.Collections.Generic;
using System.Text;

namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class OccupancyAnalyticsDto
{
    public int PeakOccupancy { get; set; }

    public DateTime? PeakOccupancyUtc { get; set; }

    public decimal AverageOccupancy { get; set; }
}