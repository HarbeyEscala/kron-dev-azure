using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026061204)]
public sealed class AddProvisioningStatusToDevices : Migration
{
    public override void Up()
    {
        Alter.Table("Devices")
            .AddColumn("ProvisioningStatus")
            .AsString(50)
            .WithDefaultValue("Pending");
    }

    public override void Down()
    {
        Delete.Column("ProvisioningStatus")
            .FromTable("Devices");
    }
}