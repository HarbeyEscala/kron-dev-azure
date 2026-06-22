using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026062201)]
public sealed class AddHourlyMaterializationIndex : Migration
{
    private const string TableName = "DeviceReadings";

    public override void Up()
    {
        Create.Index(
                "IX_DeviceReadings_StoreId_ReadingTimestampUtc")
            .OnTable(TableName)
            .OnColumn("StoreId").Ascending()
            .OnColumn("ReadingTimestampUtc").Ascending();
    }

    public override void Down()
    {
        Delete.Index(
                "IX_DeviceReadings_StoreId_ReadingTimestampUtc")
            .OnTable(TableName);
    }
}