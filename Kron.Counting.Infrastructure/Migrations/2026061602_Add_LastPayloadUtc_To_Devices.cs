using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026061602)]
public sealed class AddLastPayloadUtcToDevices : Migration
{
    public override void Up()
    {
        Alter.Table("Devices")
            .AddColumn("LastPayloadUtc")
            .AsDateTime2()
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("LastPayloadUtc")
            .FromTable("Devices");
    }
}