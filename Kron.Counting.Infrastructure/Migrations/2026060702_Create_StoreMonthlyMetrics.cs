using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026060702)]
public sealed class CreateStoreMonthlyMetrics : Migration
{
    public override void Up()
    {
        Create.Table("StoreMonthlyMetrics")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("StoreId").AsGuid().NotNullable()
            .WithColumn("MetricYear").AsInt16().NotNullable()
            .WithColumn("MetricMonth").AsByte().NotNullable()
            .WithColumn("PeopleIn").AsInt32().NotNullable()
            .WithColumn("PeopleOut").AsInt32().NotNullable()
            .WithColumn("PeakOccupancy").AsInt32().NotNullable()
            .WithColumn("AvgOccupancy").AsDecimal(18, 2).NotNullable()
            .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable()
            .WithColumn("UpdatedAtUtc").AsDateTime2().Nullable();

        Create.ForeignKey()
            .FromTable("StoreMonthlyMetrics").ForeignColumn("StoreId")
            .ToTable("Stores").PrimaryColumn("Id");

        Create.UniqueConstraint(
                "UX_StoreMonthlyMetrics_Store_Year_Month")
            .OnTable("StoreMonthlyMetrics")
            .Columns(
                "StoreId",
                "MetricYear",
                "MetricMonth");
    }

    public override void Down()
    {
        Delete.Table("StoreMonthlyMetrics");
    }
}