using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ascenso.S1.Identidad.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "s1_identidad");

            migrationBuilder.CreateTable(
                name: "bloqueos_otp_operacion",
                schema: "s1_identidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operacion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    recurso_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    bloqueado_hasta_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bloqueos_otp_operacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clientes_api_universidad",
                schema: "s1_identidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    universidad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id_keycloak = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes_api_universidad", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "desafios_otp_operacion",
                schema: "s1_identidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operacion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    recurso_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    canal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    codigo_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    salt = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    creado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expira_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    intentos_fallidos = table.Column<int>(type: "integer", nullable: false),
                    consumido_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    operation_id_consumidor = table.Column<Guid>(type: "uuid", nullable: true),
                    invalidado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_desafios_otp_operacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "desafios_segundo_factor",
                schema: "s1_identidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    canal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    codigo_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    salt = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    creado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expira_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    intentos_fallidos = table.Column<int>(type: "integer", nullable: false),
                    consumido_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    invalidado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    es_recuperacion_correo = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_desafios_segundo_factor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "s1_identidad",
                columns: table => new
                {
                    ConsumerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => new { x.ConsumerName, x.EventId });
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "s1_identidad",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "registro_decisiones",
                schema: "s1_identidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_decision = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    tipo_sujeto = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    sujeto_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    propietario_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actor_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actor_rol = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    actor_es_sistema = table.Column<bool>(type: "boolean", nullable: false),
                    ocurrido_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    justificacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    estado_anterior = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    estado_nuevo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    datos_json = table.Column<string>(type: "text", nullable: true),
                    servicio_origen = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registro_decisiones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_usuario",
                schema: "s1_identidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    nombre_normalizado = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    acceso_financiero_o_decision = table.Column<bool>(type: "boolean", nullable: false),
                    es_enlace_universidad = table.Column<bool>(type: "boolean", nullable: false),
                    es_predefinido = table.Column<bool>(type: "boolean", nullable: false),
                    permisos_json = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_usuario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "s1_identidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cedula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre_completo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    correo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    canal_preferido = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    es_evaluador_externo = table.Column<bool>(type: "boolean", nullable: false),
                    universidad_id = table.Column<Guid>(type: "uuid", nullable: true),
                    keycloak_user_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    sello_seguridad = table.Column<Guid>(type: "uuid", nullable: false),
                    bloqueado_2fa_hasta_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    programas_asignados_json = table.Column<string>(type: "text", nullable: false),
                    tipo_usuario_ids_json = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bloqueos_otp_terna_unico",
                schema: "s1_identidad",
                table: "bloqueos_otp_operacion",
                columns: new[] { "usuario_id", "operacion", "recurso_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_clientes_api_client_id_unico",
                schema: "s1_identidad",
                table: "clientes_api_universidad",
                column: "client_id_keycloak",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_desafios_otp_terna_activos",
                schema: "s1_identidad",
                table: "desafios_otp_operacion",
                columns: new[] { "usuario_id", "operacion", "recurso_id", "consumido_en_utc", "invalidado_en_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_desafios_2fa_usuario_activos",
                schema: "s1_identidad",
                table: "desafios_segundo_factor",
                columns: new[] { "usuario_id", "consumido_en_utc", "invalidado_en_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_pendientes",
                schema: "s1_identidad",
                table: "outbox_messages",
                columns: new[] { "ProcessedAtUtc", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_correlacion",
                schema: "s1_identidad",
                table: "registro_decisiones",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_fecha",
                schema: "s1_identidad",
                table: "registro_decisiones",
                column: "ocurrido_en_utc");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_programa",
                schema: "s1_identidad",
                table: "registro_decisiones",
                column: "programa_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_propietario",
                schema: "s1_identidad",
                table: "registro_decisiones",
                column: "propietario_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_sujeto",
                schema: "s1_identidad",
                table: "registro_decisiones",
                columns: new[] { "tipo_sujeto", "sujeto_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tipos_usuario_nombre_unico",
                schema: "s1_identidad",
                table: "tipos_usuario",
                column: "nombre_normalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_cedula_unico",
                schema: "s1_identidad",
                table: "usuarios",
                column: "cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_correo_unico",
                schema: "s1_identidad",
                table: "usuarios",
                column: "correo",
                unique: true);

            // Endurecimiento append-only de registro_decisiones en PostgreSQL (RF-35, HU-35.1 Anexo A).
            migrationBuilder.Sql(
                Ascenso.Messaging.Outbox.RegistroDecisionSqlAppendOnly.GenerarSqlPostgreSql(
                    IdentidadDbContext.Schema));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bloqueos_otp_operacion",
                schema: "s1_identidad");

            migrationBuilder.DropTable(
                name: "clientes_api_universidad",
                schema: "s1_identidad");

            migrationBuilder.DropTable(
                name: "desafios_otp_operacion",
                schema: "s1_identidad");

            migrationBuilder.DropTable(
                name: "desafios_segundo_factor",
                schema: "s1_identidad");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "s1_identidad");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "s1_identidad");

            migrationBuilder.DropTable(
                name: "registro_decisiones",
                schema: "s1_identidad");

            migrationBuilder.DropTable(
                name: "tipos_usuario",
                schema: "s1_identidad");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "s1_identidad");
        }
    }
}
