using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MyList.Migrations
{
    public partial class SeedproductsForDev : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO Products (Name,Category) VALUES ('Product 1',1)");
            migrationBuilder.Sql("INSERT INTO Products (Name,Category) VALUES ('Product 2',4)");
            migrationBuilder.Sql("INSERT INTO Products (Name,Category) VALUES ('Product 3',9)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Products WHERE Name IN ('Product 1','Product 2','Product 3')");
        }
    }
}
