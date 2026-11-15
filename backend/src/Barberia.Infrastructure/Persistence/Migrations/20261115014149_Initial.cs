using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barberia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "barbers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    service_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_barbers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    price_cents = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_services", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shops",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    address = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    welcome_message = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: false),
                    min_lead_minutes = table.Column<int>(type: "integer", nullable: false),
                    max_days_ahead = table.Column<int>(type: "integer", nullable: false),
                    cancel_limit_minutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shops", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "staff_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    barber_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "barber_schedules",
                columns: table => new
                {
                    day = table.Column<int>(type: "integer", nullable: false),
                    barber_id = table.Column<Guid>(type: "uuid", nullable: false),
                    opens = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    closes = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_barber_schedules", x => new { x.barber_id, x.day });
                    table.ForeignKey(
                        name: "fk_barber_schedules_barbers_barber_id",
                        column: x => x.barber_id,
                        principalTable: "barbers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "barber_time_off",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from = table.Column<DateOnly>(type: "date", nullable: false),
                    to = table.Column<DateOnly>(type: "date", nullable: false),
                    reason = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    barber_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_barber_time_off", x => x.id);
                    table.ForeignKey(
                        name: "fk_barber_time_off_barbers_barber_id",
                        column: x => x.barber_id,
                        principalTable: "barbers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_closed_days",
                columns: table => new
                {
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_closed_days", x => new { x.shop_id, x.date });
                    table.ForeignKey(
                        name: "fk_shop_closed_days_shops_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_opening_hours",
                columns: table => new
                {
                    day = table.Column<int>(type: "integer", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    opens = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    closes = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_opening_hours", x => new { x.shop_id, x.day });
                    table.ForeignKey(
                        name: "fk_shop_opening_hours_shops_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_barber_time_off_barber_id",
                table: "barber_time_off",
                column: "barber_id");

            migrationBuilder.CreateIndex(
                name: "ix_staff_accounts_barber_id",
                table: "staff_accounts",
                column: "barber_id");

            migrationBuilder.CreateIndex(
                name: "ix_staff_accounts_email",
                table: "staff_accounts",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "barber_schedules");

            migrationBuilder.DropTable(
                name: "barber_time_off");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "shop_closed_days");

            migrationBuilder.DropTable(
                name: "shop_opening_hours");

            migrationBuilder.DropTable(
                name: "staff_accounts");

            migrationBuilder.DropTable(
                name: "barbers");

            migrationBuilder.DropTable(
                name: "shops");
        }
    }
}
