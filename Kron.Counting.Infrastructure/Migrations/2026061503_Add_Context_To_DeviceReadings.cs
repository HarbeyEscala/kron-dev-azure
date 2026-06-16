using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026061503)]
public sealed class AddContextToDeviceReadings : Migration
{
    private const string Schema = "dbo";
    private const string TableName = "DeviceReadings";

    public override void Up()
    {
        Alter.Table(TableName)
            .InSchema(Schema)

            .AddColumn("DeviceAssignmentId")
            .AsGuid()
            .Nullable();

        Alter.Table(TableName)
            .InSchema(Schema)

            .AddColumn("MeasurementPointId")
            .AsGuid()
            .Nullable();

        Alter.Table(TableName)
            .InSchema(Schema)

            .AddColumn("StoreId")
            .AsGuid()
            .Nullable();

        Create.Index("IX_DeviceReadings_DeviceAssignmentId")
            .OnTable(TableName)
            .OnColumn("DeviceAssignmentId");

        Create.Index("IX_DeviceReadings_MeasurementPointId")
            .OnTable(TableName)
            .OnColumn("MeasurementPointId");

        Create.Index("IX_DeviceReadings_StoreId")
            .OnTable(TableName)
            .OnColumn("StoreId");
    }

    public override void Down()
    {
        Delete.Index("IX_DeviceReadings_DeviceAssignmentId")
            .OnTable(TableName);

        Delete.Index("IX_DeviceReadings_MeasurementPointId")
            .OnTable(TableName);

        Delete.Index("IX_DeviceReadings_StoreId")
            .OnTable(TableName);

        Delete.Column("DeviceAssignmentId")
            .FromTable(TableName);

        Delete.Column("MeasurementPointId")
            .FromTable(TableName);

        Delete.Column("StoreId")
            .FromTable(TableName);
    }
}