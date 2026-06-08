using System;
using System.Collections.Generic;
using System.Text;

namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class StoreVsStoreHourDto
{
    public int Hour { get; set; }

    public int PrimaryVisitors { get; set; }

    public int ComparisonVisitors { get; set; }
}