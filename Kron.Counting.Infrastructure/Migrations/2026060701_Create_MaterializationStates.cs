using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026060701)]
public sealed class CreateMaterializationStates : Migration
{
    public override void Up()
    {
        Create.Table("MaterializationStates")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("ProcessName").AsString(100).NotNullable()
            .WithColumn("LastProcessedUtc").AsDateTime2().NotNullable()
            .WithColumn("UpdatedAtUtc").AsDateTime2().NotNullable();

        Create.UniqueConstraint("UX_MaterializationStates_ProcessName")
            .OnTable("MaterializationStates")
            .Column("ProcessName");
    }

    public override void Down()
    {
        Delete.Table("MaterializationStates");
    }
}