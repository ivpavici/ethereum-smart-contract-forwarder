using Microsoft.EntityFrameworkCore.Migrations;

namespace EthereumExchangeWallet.Api.Migrations
{
    public partial class UpdateDepositAddressModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsContractFactoryAddress",
                table: "DepositAddresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsContractForwarderAddress",
                table: "DepositAddresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsContractFactoryAddress",
                table: "DepositAddresses");

            migrationBuilder.DropColumn(
                name: "IsContractForwarderAddress",
                table: "DepositAddresses");
        }
    }
}
