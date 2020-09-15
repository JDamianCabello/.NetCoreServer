using Microsoft.EntityFrameworkCore.Migrations;

namespace CallCenterServer.Migrations
{
    /// <summary>
    /// Migration to set up our database and tables. 
    /// To create this migration use: Update-Database in the VS console
    /// To remove this migration use: remove-migration in VS console
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <summary>
        /// The Up method contains C# code that applies any changes made to the model to the schema of the database since the last migration was generated.
        /// </summary>
        /// <param name="migrationBuilder">A builder providing a fluentish API for building MigrationOperations</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    IsAdmin = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <summary>
        /// The Down method reverses those changes, restoring the database to the state of the previous migration.
        /// </summary>
        /// <param name="migrationBuilder">A builder providing a fluentish API for building MigrationOperations</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
