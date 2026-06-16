using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026061601)]
public sealed class CreateAlerts : Migration
{
    public override void Up()
    {
        Create.Table("Alerts")

            .WithColumn("Id")
            .AsGuid()
            .PrimaryKey()

            .WithColumn("TenantId")
            .AsGuid()
            .NotNullable()

            .WithColumn("DeviceId")
            .AsGuid()
            .Nullable()

            .WithColumn("StoreId")
            .AsGuid()
            .Nullable()

            .WithColumn("MeasurementPointId")
            .AsGuid()
            .Nullable()

            .WithColumn("Source")
            .AsString(50)
            .NotNullable()

            .WithColumn("Type")
            .AsString(100)
            .NotNullable()

            .WithColumn("Severity")
            .AsString(50)
            .NotNullable()

            .WithColumn("Message")
            .AsString(500)
            .NotNullable()

            .WithColumn("CreatedAtUtc")
            .AsDateTime2()
            .NotNullable()

            .WithColumn("LastTriggeredAtUtc")
            .AsDateTime2()
            .Nullable()

            .WithColumn("ResolvedAtUtc")
            .AsDateTime2()
            .Nullable()

            .WithColumn("IsResolved")
            .AsBoolean()
            .NotNullable()
            .WithDefaultValue(false);

        Create.Index("IX_Alerts_TenantId")
            .OnTable("Alerts")
            .OnColumn("TenantId");

        Create.Index("IX_Alerts_DeviceId")
            .OnTable("Alerts")
            .OnColumn("DeviceId");

        Create.Index("IX_Alerts_Type")
            .OnTable("Alerts")
            .OnColumn("Type");

        Create.Index("IX_Alerts_TenantId_IsResolved")
            .OnTable("Alerts")
            .OnColumn("TenantId").Ascending()
            .OnColumn("IsResolved").Ascending();

        Create.Index("IX_Alerts_CreatedAtUtc")
            .OnTable("Alerts")
            .OnColumn("CreatedAtUtc");

        Create.Index("IX_Alerts_Deduplication")
            .OnTable("Alerts")
            .OnColumn("TenantId").Ascending()
            .OnColumn("DeviceId").Ascending()
            .OnColumn("Type").Ascending()
            .OnColumn("IsResolved").Ascending();
    }

    public override void Down()
    {
        Delete.Table("Alerts");
    }
}