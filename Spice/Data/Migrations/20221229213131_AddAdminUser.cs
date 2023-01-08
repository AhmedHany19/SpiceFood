using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spice.Data.Migrations
{
    public partial class AddAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [City], [Name], [PostalCode], [State], [StreetAddress]) VALUES (N'83c597f4-85f2-4192-b1aa-91a6cfcfcfc7', N'Manger@test.com', N'MANGER@TEST.COM', N'Manger@test.com', N'MANGER@TEST.COM', 1, N'AQAAAAEAACcQAAAAEIJ4PSFwuOzewNByIwnkMdKa1I2zRf5MrK1ogI7y1dXJtU+srs6ArFHYJYer9Cps+w==', N'AJCLLF4MLIVWLBDZU6THY2V2FIPEQNGK', N'4c3cb08b-2cf6-4ff1-ae92-8bf2b86ef974', N'0111111111', 0, 0, NULL, 1, 0, N'cairo', N'Manager', N'00200', N'banha', N'egypt')\r\n");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [dbo].[AspNetUsers] WHERE Id ='83c597f4-85f2-4192-b1aa-91a6cfcfcfc7'");
        }
    }
}
