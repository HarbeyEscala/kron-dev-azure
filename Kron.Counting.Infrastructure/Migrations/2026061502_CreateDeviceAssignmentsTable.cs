using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026061502)]
public sealed class CreateDeviceAssignmentsTable : Migration
{
    private const string Schema = "dbo";
    private const string TableName = "DeviceAssignments";

    public override void Up()
    {
        Create.Table(TableName).InSchema(Schema)

            .WithColumn("Id").AsGuid().PrimaryKey()

            .WithColumn("DeviceId").AsGuid().NotNullable()

            .WithColumn("MeasurementPointId").AsGuid().NotNullable()

            .WithColumn("AssignedAtUtc").AsDateTime2().NotNullable()

            .WithColumn("UnassignedAtUtc").AsDateTime2().Nullable()

            .WithColumn("BaselineTotalIn").AsInt32().NotNullable()

            .WithColumn("BaselineTotalOut").AsInt32().NotNullable()

            .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable();

        Create.ForeignKey("FK_DeviceAssignments_Devices")
            .FromTable(TableName).InSchema(Schema).ForeignColumn("DeviceId")
            .ToTable("Devices").InSchema(Schema).PrimaryColumn("Id");

        Create.ForeignKey("FK_DeviceAssignments_MeasurementPoints")
            .FromTable(TableName).InSchema(Schema).ForeignColumn("MeasurementPointId")
            .ToTable("MeasurementPoints").InSchema(Schema).PrimaryColumn("Id");

        Create.Index("IX_DeviceAssignments_DeviceId")
            .OnTable(TableName)
            .OnColumn("DeviceId");

        Create.Index("IX_DeviceAssignments_MeasurementPointId")
            .OnTable(TableName)
            .OnColumn("MeasurementPointId");
    }

    public override void Down()
    {
        Delete.Table(TableName);
    }
}