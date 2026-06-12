using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026061203)]
public sealed class MakeDeviceStoreNullable : Migration
{
    public override void Up()
    {
        Alter.Column("StoreId")
            .OnTable("Devices")
            .AsGuid()
            .Nullable();
    }

    public override void Down()
    {
        Alter.Column("StoreId")
            .OnTable("Devices")
            .AsGuid()
            .NotNullable();
    }
}