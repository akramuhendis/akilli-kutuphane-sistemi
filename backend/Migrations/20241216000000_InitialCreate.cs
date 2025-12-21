using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLibrary.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IslemKayitlari",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IslemTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IslemKayitlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kaynaklar",
                columns: table => new
                {
                    ISBN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Yazar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    YayinTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OduncDurumu = table.Column<bool>(type: "bit", nullable: false),
                    OduncTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OkunmaSayisi = table.Column<int>(type: "int", nullable: false),
                    Kategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KaynakTuru = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SayfaSayisi = table.Column<int>(type: "int", nullable: true),
                    YayinEvi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SayiNo = table.Column<int>(type: "int", nullable: true),
                    YayinPeriyodu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISSN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Universite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bolum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DanismanAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TezTuru = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kaynaklar", x => x.ISBN);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Yas = table.Column<int>(type: "int", nullable: false),
                    IlgiAlanlari = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FavoriKategoriler = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OduncKayitlari",
                columns: table => new
                {
                    ISBN = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    OduncTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KaynakBaslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Kategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IadeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TeslimSuresi = table.Column<int>(type: "int", nullable: false),
                    KullaniciId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    KullaniciIdAktif = table.Column<string>(type: "nvarchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OduncKayitlari", x => new { x.ISBN, x.OduncTarihi });
                    table.ForeignKey(
                        name: "FK_OduncKayitlari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OduncKayitlari_Kullanicilar_KullaniciIdAktif",
                        column: x => x.KullaniciIdAktif,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OduncKayitlari_KullaniciId",
                table: "OduncKayitlari",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_OduncKayitlari_KullaniciIdAktif",
                table: "OduncKayitlari",
                column: "KullaniciIdAktif");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IslemKayitlari");

            migrationBuilder.DropTable(
                name: "OduncKayitlari");

            migrationBuilder.DropTable(
                name: "Kaynaklar");

            migrationBuilder.DropTable(
                name: "Kullanicilar");
        }
    }
}

