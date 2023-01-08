using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spice.Data.Migrations
{
    public partial class AssignAllROlesToMangerUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO [dbo].[AspNetUserRoles](UserId,RoleId) SELECT '83c597f4-85f2-4192-b1aa-91a6cfcfcfc7', Id FROM [dbo].[AspNetRoles]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [dbo].[AspNetRoles] WHERE UserId='83c597f4-85f2-4192-b1aa-91a6cfcfcfc7'");

        }
    }
}
