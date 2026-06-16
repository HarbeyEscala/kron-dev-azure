using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026061501)]
public sealed class CreateMeasurementPointsTable : Migration
{
    private const string Schema = "dbo";
    private const string TableName = "MeasurementPoints";

    public override void Up()
    {
        Create.Table(TableName).InSchema(Schema)

            .WithColumn("Id").AsGuid().PrimaryKey()

            .WithColumn("StoreId").AsGuid().NotNullable()

            .WithColumn("Name").AsString(200).NotNullable()

            .WithColumn("Description").AsString(500).Nullable()

            .WithColumn("IsActive").AsBoolean().NotNullable()

            .WithColumn("CreatedAtUtc")
                .AsDateTime2()
                .NotNullable()

            .WithColumn("UpdatedAtUtc")
                .AsDateTime2()
                .Nullable();

        Create.ForeignKey("FK_MeasurementPoints_Stores")
            .FromTable(TableName).InSchema(Schema).ForeignColumn("StoreId")
            .ToTable("Stores").InSchema(Schema).PrimaryColumn("Id");

        Create.Index("IX_MeasurementPoints_StoreId")
            .OnTable(TableName)
            .OnColumn("StoreId");
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_MeasurementPoints_Stores")
            .OnTable(TableName);

        Delete.Table(TableName);
    }
}