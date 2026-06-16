using FluentMigrator;

namespace Kron.Counting.Infrastructure.Migrations;

[Migration(2026061603)]
public sealed class CreateUserDeviceTokens : Migration
{
    private const string Schema = "dbo";
    private const string TableName = "UserDeviceTokens";

    public override void Up()
    {
        Create.Table(TableName).InSchema(Schema)

            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()

            .WithColumn("TenantId").AsGuid().NotNullable()

            .WithColumn("UserId").AsGuid().NotNullable()

            .WithColumn("Token").AsString(1000).NotNullable()

            .WithColumn("Platform").AsString(50).NotNullable()

            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)

            .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable()

            .WithColumn("LastUsedAtUtc").AsDateTime2().Nullable();

        Create.ForeignKey("FK_UserDeviceTokens_Users_UserId")
            .FromTable(TableName).InSchema(Schema).ForeignColumn("UserId")
            .ToTable("Users").InSchema(Schema).PrimaryColumn("Id");

        Create.ForeignKey("FK_UserDeviceTokens_Tenants_TenantId")
            .FromTable(TableName).InSchema(Schema).ForeignColumn("TenantId")
            .ToTable("Tenants").InSchema(Schema).PrimaryColumn("Id");

        Create.Index("IX_UserDeviceTokens_TenantId")
            .OnTable(TableName).InSchema(Schema)
            .OnColumn("TenantId");

        Create.Index("IX_UserDeviceTokens_UserId")
            .OnTable(TableName).InSchema(Schema)
            .OnColumn("UserId");

        Create.Index("IX_UserDeviceTokens_IsActive")
            .OnTable(TableName).InSchema(Schema)
            .OnColumn("IsActive");

        Execute.Sql("""
            CREATE UNIQUE INDEX UX_UserDeviceTokens_Token
            ON dbo.UserDeviceTokens(Token);
        """);

        Execute.Sql("""
            CREATE INDEX IX_UserDeviceTokens_TenantId_IsActive
            ON dbo.UserDeviceTokens(TenantId, IsActive);
        """);
    }

    public override void Down()
    {
        Execute.Sql("DROP INDEX IX_UserDeviceTokens_TenantId_IsActive ON dbo.UserDeviceTokens;");
        Execute.Sql("DROP INDEX UX_UserDeviceTokens_Token ON dbo.UserDeviceTokens;");

        Delete.ForeignKey("FK_UserDeviceTokens_Users_UserId")
            .OnTable(TableName).InSchema(Schema);

        Delete.ForeignKey("FK_UserDeviceTokens_Tenants_TenantId")
            .OnTable(TableName).InSchema(Schema);

        Delete.Table(TableName).InSchema(Schema);
    }
}