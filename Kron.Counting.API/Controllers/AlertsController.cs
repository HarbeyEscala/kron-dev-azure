using Kron.Counting.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Route("api/v1/alerts")]
public sealed class AlertsController : ControllerBase
{
    private readonly IAlertRepository _alertRepository;

    public AlertsController(
        IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    [HttpGet("open")]
    public async Task<IActionResult> GetOpen(
        [FromQuery] Guid tenantId)
    {
        var result =
            await _alertRepository.GetOpenAlertsAsync(
                tenantId);

        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] Guid tenantId,
        [FromQuery] int take = 100)
    {
        var result =
            await _alertRepository.GetHistoryAsync(
                tenantId,
                take);

        return Ok(result);
    }

    [HttpGet("device/{deviceId:guid}")]
    public async Task<IActionResult> GetByDevice(
        Guid deviceId,
        [FromQuery] Guid tenantId)
    {
        var result =
            await _alertRepository.GetByDeviceAsync(
                tenantId,
                deviceId);

        return Ok(result);
    }

    [HttpGet("store/{storeId:guid}")]
    public async Task<IActionResult> GetByStore(
        Guid storeId,
        [FromQuery] Guid tenantId)
    {
        var result =
            await _alertRepository.GetByStoreAsync(
                tenantId,
                storeId);

        return Ok(result);
    }

    [HttpPost("{alertId:guid}/resolve")]
    public async Task<IActionResult> Resolve(
        Guid alertId)
    {
        await _alertRepository.ResolveAsync(
            alertId);

        return NoContent();
    }
}