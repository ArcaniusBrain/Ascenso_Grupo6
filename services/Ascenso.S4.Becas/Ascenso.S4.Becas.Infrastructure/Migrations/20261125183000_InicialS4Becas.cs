using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ascenso.S4.Becas.Infrastructure.Persistencia;

namespace Ascenso.S4.Becas.Infrastructure.Migrations;

[DbContext(typeof(BecaDbContext))]
[Migration("20261125183000_InicialS4Becas")]
public sealed class InicialS4Becas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "s4");

        migrationBuilder.CreateTable(
            name: "becas",
            schema: "s4",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                postulante_id = table.Column<Guid>(nullable: false),
                programa_id = table.Column<Guid>(nullable: false),
                oferta_id = table.Column<Guid>(nullable: false),
                estado = table.Column<string>(maxLength: 30, nullable: false),
                alcance = table.Column<string>(maxLength: 20, nullable: false),
                modalidad = table.Column<string>(maxLength: 30, nullable: false),
                version = table.Column<int>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_becas", x => x.id));

        migrationBuilder.CreateTable(
            name: "tramos_beca",
            schema: "s4",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                beca_id = table.Column<Guid>(nullable: false),
                periodo = table.Column<string>(maxLength: 20, nullable: false),
                monto_cantidad = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                monto_moneda = table.Column<string>(maxLength: 3, nullable: false),
                origen = table.Column<string>(maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tramos_beca", x => x.id);
                table.ForeignKey(
                    name: "FK_tramos_beca_becas_beca_id",
                    column: x => x.beca_id,
                    principalSchema: "s4",
                    principalTable: "becas",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "referencia_personas",
            schema: "s4",
            columns: table => new
            {
                persona_id = table.Column<Guid>(nullable: false),
                cedula = table.Column<string>(maxLength: 20, nullable: false),
                nombre_completo = table.Column<string>(maxLength: 200, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_referencia_personas", x => x.persona_id));

        migrationBuilder.CreateTable(
            name: "referencia_programas",
            schema: "s4",
            columns: table => new
            {
                programa_id = table.Column<Guid>(nullable: false),
                nombre = table.Column<string>(maxLength: 200, nullable: false),
                alcance = table.Column<string>(maxLength: 20, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_referencia_programas", x => x.programa_id));

        migrationBuilder.CreateTable(
            name: "outbox_messages",
            schema: "s4",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                EventType = table.Column<string>(maxLength: 200, nullable: false),
                SchemaVersion = table.Column<int>(nullable: false),
                PayloadJson = table.Column<string>(nullable: false),
                CorrelationId = table.Column<Guid>(nullable: false),
                CausationId = table.Column<Guid>(nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(nullable: false),
                ProcessedAtUtc = table.Column<DateTimeOffset>(nullable: true),
                Attempts = table.Column<int>(nullable: false),
                LastError = table.Column<string>(maxLength: 2000, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_outbox_messages", x => x.Id));

        migrationBuilder.CreateTable(
            name: "inbox_messages",
            schema: "s4",
            columns: table => new
            {
                ConsumerName = table.Column<string>(maxLength: 200, nullable: false),
                EventId = table.Column<Guid>(nullable: false),
                EventType = table.Column<string>(maxLength: 200, nullable: false),
                ProcessedAtUtc = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_inbox_messages", x => new { x.ConsumerName, x.EventId }));

        migrationBuilder.CreateTable(
            name: "registro_decisiones",
            schema: "s4",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                tipo_decision = table.Column<string>(maxLength: 80, nullable: false),
                tipo_sujeto = table.Column<string>(maxLength: 40, nullable: false),
                sujeto_id = table.Column<string>(maxLength: 120, nullable: false),
                actor_usuario_id = table.Column<Guid>(nullable: true),
                actor_rol = table.Column<string>(maxLength: 80, nullable: false),
                actor_es_sistema = table.Column<bool>(nullable: false),
                programa_id = table.Column<Guid>(nullable: true),
                propietario_usuario_id = table.Column<Guid>(nullable: true),
                ocurrido_en_utc = table.Column<DateTimeOffset>(nullable: false),
                justificacion = table.Column<string>(maxLength: 2000, nullable: false),
                estado_anterior = table.Column<string>(maxLength: 60, nullable: true),
                estado_nuevo = table.Column<string>(maxLength: 60, nullable: true),
                datos_json = table.Column<string>(nullable: true),
                servicio_origen = table.Column<string>(maxLength: 40, nullable: false),
                correlation_id = table.Column<Guid>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_registro_decisiones", x => x.id));

        migrationBuilder.CreateIndex(name: "ix_becas_postulante", schema: "s4", table: "becas", column: "postulante_id");
        migrationBuilder.CreateIndex(name: "ix_becas_programa", schema: "s4", table: "becas", column: "programa_id");
        migrationBuilder.CreateIndex(name: "ix_tramos_beca_periodo", schema: "s4", table: "tramos_beca", column: "periodo");
        migrationBuilder.CreateIndex(name: "ux_referencia_personas_cedula", schema: "s4", table: "referencia_personas", column: "cedula", unique: true);
        migrationBuilder.CreateIndex(name: "ix_outbox_pendientes", schema: "s4", table: "outbox_messages", columns: ["ProcessedAtUtc", "OccurredAtUtc"]);
        migrationBuilder.CreateIndex(name: "ix_registro_decisiones_sujeto", schema: "s4", table: "registro_decisiones", columns: ["tipo_sujeto", "sujeto_id"]);
        migrationBuilder.CreateIndex(name: "ix_registro_decisiones_fecha", schema: "s4", table: "registro_decisiones", column: "ocurrido_en_utc");
        migrationBuilder.CreateIndex(name: "ix_registro_decisiones_programa", schema: "s4", table: "registro_decisiones", column: "programa_id");
        migrationBuilder.CreateIndex(name: "ix_registro_decisiones_propietario", schema: "s4", table: "registro_decisiones", column: "propietario_usuario_id");
        migrationBuilder.CreateIndex(name: "ix_registro_decisiones_correlacion", schema: "s4", table: "registro_decisiones", column: "correlation_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "tramos_beca", schema: "s4");
        migrationBuilder.DropTable(name: "becas", schema: "s4");
        migrationBuilder.DropTable(name: "referencia_personas", schema: "s4");
        migrationBuilder.DropTable(name: "referencia_programas", schema: "s4");
        migrationBuilder.DropTable(name: "outbox_messages", schema: "s4");
        migrationBuilder.DropTable(name: "inbox_messages", schema: "s4");
        migrationBuilder.DropTable(name: "registro_decisiones", schema: "s4");
    }
}
