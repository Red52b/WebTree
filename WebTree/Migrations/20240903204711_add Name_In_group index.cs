using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTree.Migrations
{
    public partial class addName_In_groupindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NodeName",
                table: "Nodes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EventId",
                table: "JournalRecords",
                type: "text",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Nodes_TreeId_ParentNodeId_NodeName",
                table: "Nodes",
                columns: new[] { "TreeId", "ParentNodeId", "NodeName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Nodes_TreeId_ParentNodeId_NodeName",
                table: "Nodes");

            migrationBuilder.AlterColumn<string>(
                name: "NodeName",
                table: "Nodes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "EventId",
                table: "JournalRecords",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
