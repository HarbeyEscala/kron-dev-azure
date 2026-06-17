using Kron.Counting.Application.DTOs.Responses;
using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Mappings;

public static class DomainToDtoMappings
{
    public static BrandDto ToDto(this Brand entity)
    {
        return new BrandDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    public static TenantDto ToDto(this Tenant entity)
    {
        return new TenantDto
        {
            Id = entity.Id,
            BrandId = entity.BrandId,
            Code = entity.Code,
            Name = entity.Name,
            TimeZone = entity.TimeZone,
            Currency = entity.Currency,
            Locale = entity.Locale,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    public static UserDto ToDto(this User entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Role = entity.Role,
            IsActive = entity.IsActive,
            LastLoginUtc = entity.LastLoginUtc,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    public static StoreDto ToDto(this Store entity)
    {
        return new StoreDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            Country = entity.Country,
            State = entity.State,
            City = entity.City,
            PostalCode = entity.PostalCode,
            AddressLine1 = entity.AddressLine1,
            AddressLine2 = entity.AddressLine2,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            TimeZone = entity.TimeZone,
            ContactName = entity.ContactName,
            ContactEmail = entity.ContactEmail,
            ContactPhone = entity.ContactPhone,
            Capacity = entity.Capacity,
            StoreType = entity.StoreType,
            Region = entity.Region,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    public static DeviceDto ToDto(this Device entity)
    {
        return new DeviceDto
        {
            Id = entity.Id,
            StoreId = entity.StoreId,
            SerialNumber = entity.SerialNumber,
            Name = entity.Name,
            DeviceType = entity.DeviceType,
            FirmwareVersion = entity.FirmwareVersion,
            IpAddress = entity.IpAddress,
            ProvisioningStatus = entity.ProvisioningStatus,
            LastSeenAtUtc = entity.LastSeenAtUtc,
            IsOnline = entity.IsOnline,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    public static DashboardSnapshotDto ToDto(this LiveDashboardSnapshot entity)
    {
        return new DashboardSnapshotDto
        {
            StoreId = entity.StoreId,
            CurrentOccupancy = entity.CurrentOccupancy,
            TodayIn = entity.TodayIn,
            TodayOut = entity.TodayOut,
            LastReadingAtUtc = entity.LastReadingAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    public static StoreHourlyMetricDto ToDto(this StoreHourlyMetric entity)
    {
        return new StoreHourlyMetricDto
        {
            StoreId = entity.StoreId,
            MetricDate = entity.MetricDate,
            MetricHour = entity.MetricHour,
            PeopleIn = entity.PeopleIn,
            PeopleOut = entity.PeopleOut,
            PeakOccupancy = entity.PeakOccupancy,
            AvgOccupancy = entity.AvgOccupancy
        };
    }

    public static StoreDailyMetricDto ToDto(this StoreDailyMetric entity)
    {
        return new StoreDailyMetricDto
        {
            StoreId = entity.StoreId,
            MetricDate = entity.MetricDate,
            PeopleIn = entity.PeopleIn,
            PeopleOut = entity.PeopleOut,
            PeakOccupancy = entity.PeakOccupancy,
            AvgOccupancy = entity.AvgOccupancy
        };
    }
}