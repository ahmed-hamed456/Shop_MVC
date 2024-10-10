using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myshop.Web.Migrations
{
    /// <inheritdoc />
    public partial class EditSomeFeilds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ShoppingCarts",
                newName: "ShoppingCartId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Products",
                newName: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShoppingCartId",
                table: "ShoppingCarts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Products",
                newName: "Id");
        }
    }
}
