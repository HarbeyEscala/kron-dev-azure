using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026061604)]
public sealed class ExpandStores : Migration
{
    public override void Up()
    {
        Alter.Table("Stores")

            .AddColumn("Country")
            .AsString(100)
            .Nullable()

            .AddColumn("State")
            .AsString(100)
            .Nullable()

            .AddColumn("City")
            .AsString(100)
            .Nullable()

            .AddColumn("PostalCode")
            .AsString(50)
            .Nullable()

            .AddColumn("AddressLine1")
            .AsString(250)
            .Nullable()

            .AddColumn("AddressLine2")
            .AsString(250)
            .Nullable()

            .AddColumn("Latitude")
            .AsDecimal(18, 8)
            .Nullable()

            .AddColumn("Longitude")
            .AsDecimal(18, 8)
            .Nullable()

            .AddColumn("TimeZone")
            .AsString(100)
            .Nullable()

            .AddColumn("ContactName")
            .AsString(150)
            .Nullable()

            .AddColumn("ContactEmail")
            .AsString(250)
            .Nullable()

            .AddColumn("ContactPhone")
            .AsString(50)
            .Nullable()

            .AddColumn("Capacity")
            .AsInt32()
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("Country").FromTable("Stores");
        Delete.Column("State").FromTable("Stores");
        Delete.Column("City").FromTable("Stores");
        Delete.Column("PostalCode").FromTable("Stores");
        Delete.Column("AddressLine1").FromTable("Stores");
        Delete.Column("AddressLine2").FromTable("Stores");
        Delete.Column("Latitude").FromTable("Stores");
        Delete.Column("Longitude").FromTable("Stores");
        Delete.Column("TimeZone").FromTable("Stores");
        Delete.Column("ContactName").FromTable("Stores");
        Delete.Column("ContactEmail").FromTable("Stores");
        Delete.Column("ContactPhone").FromTable("Stores");
        Delete.Column("Capacity").FromTable("Stores");
    }
}