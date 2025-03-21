namespace HaasConnectorAPIs.Migrations;

/// <inheritdoc />

/// <summary>
/// Defines the <see cref="InitialCreate" />
/// </summary>
internal partial class InitialCreate : Migration
{
    /// <inheritdoc />

    /// <summary>
    /// The Up
    /// </summary>
    /// <param name="migrationBuilder">The migrationBuilder<see cref="MigrationBuilder"/></param>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "HaasData",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ParamaterName = table.Column<string>(type: "TEXT", nullable: false),
                Value = table.Column<string>(type: "TEXT", nullable: false),
                time = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HaasData", x => x.Id);
            });
    }

    /// <inheritdoc />

    /// <summary>
    /// The Down
    /// </summary>
    /// <param name="migrationBuilder">The migrationBuilder<see cref="MigrationBuilder"/></param>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "HaasData");
    }
}
