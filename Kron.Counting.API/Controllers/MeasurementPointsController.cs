using Kron.Counting.Application.DTOs;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Route("api/v1/measurement-points")]
public sealed class MeasurementPointsController : ControllerBase
{
    private readonly IMeasurementPointRepository _measurementPointRepository;

    public MeasurementPointsController(
        IMeasurementPointRepository measurementPointRepository)
    {
        _measurementPointRepository = measurementPointRepository;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMeasurementPointRequest request,
        CancellationToken cancellationToken)
    {
        var measurementPoint = new MeasurementPoint
        {
            Id = Guid.NewGuid(),
            StoreId = request.StoreId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _measurementPointRepository.CreateAsync(measurementPoint);

        var response = new MeasurementPointResponse
        {
            Id = measurementPoint.Id,
            StoreId = measurementPoint.StoreId,
            Name = measurementPoint.Name,
            Description = measurementPoint.Description,
            IsActive = measurementPoint.IsActive,
            CreatedAtUtc = measurementPoint.CreatedAtUtc
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = measurementPoint.Id },
            response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var measurementPoint =
            await _measurementPointRepository.GetByIdAsync(id);

        if (measurementPoint is null)
        {
            return NotFound();
        }

        return Ok(measurementPoint);
    }

    [HttpGet("by-store/{storeId:guid}")]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var measurementPoints =
            await _measurementPointRepository.GetByStoreAsync(storeId);

        return Ok(measurementPoints);
    }
}