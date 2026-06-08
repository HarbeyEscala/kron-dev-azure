using System;
using System.Collections.Generic;
using System.Text;

namespace Kron.Counting.Application.Interfaces;
public interface ICacheInvalidationService
{
    Task InvalidateAnalyticsAsync(
        CancellationToken cancellationToken = default);
}