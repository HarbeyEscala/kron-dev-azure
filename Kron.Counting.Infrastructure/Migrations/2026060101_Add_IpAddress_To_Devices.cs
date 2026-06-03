using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026060101)]
public sealed class Add_IpAddress_To_Devices : Migration
{
    public override void Up()
    {
        Alter.Table("Devices")
            .InSchema("dbo")
            .AddColumn("IpAddress")
            .AsString(50)
            .Nullable();

        Create.Index("IX_Devices_IpAddress")
            .OnTable("Devices")
            .InSchema("dbo")
            .OnColumn("IpAddress");
    }

    public override void Down()
    {
        Delete.Index("IX_Devices_IpAddress")
            .OnTable("Devices")
            .InSchema("dbo");

        Delete.Column("IpAddress")
            .FromTable("Devices")
            .InSchema("dbo");
    }
}