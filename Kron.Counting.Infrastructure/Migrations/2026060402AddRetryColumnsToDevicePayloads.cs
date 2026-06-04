using System;
using System.Collections.Generic;
using System.Text;
using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026060402)]
public class AddRetryColumnsToDevicePayloads : Migration
{
    public override void Up()
    {
        Alter.Table("DevicePayloads")

            .AddColumn("RetryCount")
            .AsInt32()
            .NotNullable()
            .WithDefaultValue(0);

        Alter.Table("DevicePayloads")

            .AddColumn("LastRetryAtUtc")
            .AsDateTime2()
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("RetryCount")
            .FromTable("DevicePayloads");

        Delete.Column("LastRetryAtUtc")
            .FromTable("DevicePayloads");
    }
}