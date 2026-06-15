using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026061205)]
public sealed class Add_TenantId_To_Devices : Migration
{
    public override void Up()
    {
        Alter.Table("Devices")
            .AddColumn("TenantId")
            .AsGuid()
            .Nullable();

        Create.Index("IX_Devices_TenantId")
            .OnTable("Devices")
            .OnColumn("TenantId");

        Create.ForeignKey("FK_Devices_Tenants")
            .FromTable("Devices")
            .ForeignColumn("TenantId")
            .ToTable("Tenants")
            .PrimaryColumn("Id");
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Devices_Tenants")
            .OnTable("Devices");

        Delete.Index("IX_Devices_TenantId")
            .OnTable("Devices");

        Delete.Column("TenantId")
            .FromTable("Devices");
    }
}