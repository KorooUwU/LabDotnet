using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class ConvertToBookstore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Products",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Isbn",
                table: "Products",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PublicationYear",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "Products",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");
            // Only convert the original demo catalog; preserve custom products and order snapshots.
            migrationBuilder.Sql("UPDATE Products SET Name=N'Những ngày đọc chậm', Description=N'Sách mẫu · Những câu chuyện về một nhịp sống bình yên.', Category=N'VĂN HỌC', Price=125000, Author=N'An Mộc (mẫu)', Publisher=N'Nhà xuất bản mẫu', PublicationYear=2026, PageCount=208, ImageUrl='/images/books/doc-cham.svg', Emoji='' WHERE Id=1 AND Name=N'Cốc gốm Sương Mai';");
            migrationBuilder.Sql("UPDATE Products SET Name=N'Một khu vườn trong tâm trí', Description=N'Sách mẫu · Dành một khoảng lặng để hiểu bản thân.', Category=N'PHÁT TRIỂN BẢN THÂN', Price=149000, Author=N'Minh An (mẫu)', Publisher=N'Nhà xuất bản mẫu', PublicationYear=2026, PageCount=256, ImageUrl='/images/books/khu-vuon.svg', Emoji='' WHERE Id=2 AND Name=N'Nến thơm Tháng Mười';");
            migrationBuilder.Sql("UPDATE Products SET Name=N'Chuyến đi của hạt mầm', Description=N'Sách mẫu · Câu chuyện nhỏ dành cho những trí tưởng tượng lớn.', Category=N'THIẾU NHI', Price=89000, Author=N'Mai Lá (mẫu)', Publisher=N'Nhà xuất bản mẫu', PublicationYear=2026, PageCount=48, ImageUrl='/images/books/hat-mam.svg', Emoji='' WHERE Id=3 AND Name=N'Túi vải Mộc';");
            migrationBuilder.Sql("UPDATE Products SET Name=N'Bắt đầu với C#', Description=N'Sách mẫu · Làm quen lập trình qua những ví dụ gần gũi.', Category=N'CÔNG NGHỆ', Price=195000, Author=N'Quang Nam (mẫu)', Publisher=N'Nhà xuất bản mẫu', PublicationYear=2026, PageCount=320, ImageUrl='/images/books/csharp.svg', Emoji='' WHERE Id=4 AND Name=N'Bình hoa Đất Đỏ';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Isbn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PageCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PublicationYear",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "Products");
        }
    }
}


