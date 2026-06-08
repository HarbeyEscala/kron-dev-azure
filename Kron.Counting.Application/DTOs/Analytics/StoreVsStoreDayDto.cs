using System;
using System.Collections.Generic;
using System.Text;

namespace Kron.Counting.Application.DTOs.Analytics;
public sealed class StoreVsStoreDayDto
{
    public string DayName { get; set; } = string.Empty;

    public int PrimaryVisitors { get; set; }

    public int ComparisonVisitors { get; set; }
}