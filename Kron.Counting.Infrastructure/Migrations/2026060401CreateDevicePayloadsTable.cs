using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026060401)]
public class CreateDevicePayloadsTable : Migration
{
    public override void Up()
    {
        Create.Table("DevicePayloads")

            .WithColumn("Id").AsGuid().PrimaryKey()

            .WithColumn("DeviceId").AsGuid().NotNullable()

            .WithColumn("PayloadType").AsString(50).NotNullable()

            .WithColumn("RawHex").AsString(int.MaxValue).NotNullable()

            .WithColumn("Status").AsString(30).NotNullable()

            .WithColumn("ErrorMessage").AsString(int.MaxValue).Nullable()

            .WithColumn("ReceivedAtUtc").AsDateTime2().NotNullable()

            .WithColumn("ProcessedAtUtc").AsDateTime2().Nullable();

        Create.Index("IX_DevicePayloads_Status")
            .OnTable("DevicePayloads")
            .OnColumn("Status");
    }

    public override void Down()
    {
        Delete.Table("DevicePayloads");
    }
}