using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026062202)]
public sealed class CreateDeviceTelemetryStatus : Migration
{
    private const string Schema = "dbo";

    public override void Up()
    {
        Create.Table("DeviceTelemetryStatus")
            .InSchema(Schema)

            .WithColumn("DeviceId")
            .AsGuid()
            .PrimaryKey()

            .WithColumn("LastSeenAtUtc")
            .AsDateTime2()
            .Nullable()

            .WithColumn("LastPayloadUtc")
            .AsDateTime2()
            .Nullable()

            .WithColumn("IpAddress")
            .AsString(45)
            .Nullable()

            .WithColumn("IsOnline")
            .AsBoolean()
            .NotNullable()
            .WithDefaultValue(false)

            .WithColumn("UpdatedAtUtc")
            .AsDateTime2()
            .NotNullable();

        Create.ForeignKey(
                "FK_DeviceTelemetryStatus_Devices")
            .FromTable("DeviceTelemetryStatus")
            .ForeignColumn("DeviceId")
            .ToTable("Devices")
            .PrimaryColumn("Id");
    }

    public override void Down()
    {
        Delete.Table("DeviceTelemetryStatus")
            .InSchema(Schema);
    }
}