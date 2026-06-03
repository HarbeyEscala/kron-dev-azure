using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026060301)]
public sealed class AddUniqueIndexToDeviceReadings : Migration
{
    public override void Up()
    {
        Create.Index("UX_DeviceReadings_Unique")
            .OnTable("DeviceReadings")
            .OnColumn("DeviceId").Ascending()
            .OnColumn("ReadingTimestampUtc").Ascending()
            .OnColumn("PeopleIn").Ascending()
            .OnColumn("PeopleOut").Ascending()
            .WithOptions()
            .Unique();
    }

    public override void Down()
    {
        Delete.Index("UX_DeviceReadings_Unique")
            .OnTable("DeviceReadings");
    }
}