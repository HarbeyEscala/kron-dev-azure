using System;
using System.Collections.Generic;
using System.Text;
using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Interfaces;

public interface IDevicePayloadProcessor
{
    Task ProcessAsync(
        DevicePayload payload,
        CancellationToken cancellationToken = default);
}

