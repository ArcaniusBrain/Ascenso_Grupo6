using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ascenso.S2.CatalogoFondos.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "s2_catalogo_fondos");

            migrationBuilder.CreateTable(
                name: "calendarios_academicos",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendarios_academicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "donantes",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nombre_normalizado = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    contacto_correo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    contacto_telefono = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    registrado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_donantes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "s2_catalogo_fondos",
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
                name: "inicios_inscripciones_periodo",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    universidad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_universidad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    codigo_periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    registrado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inicios_inscripciones_periodo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operaciones_idempotentes_catalogo",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resultado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    procesado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operaciones_idempotentes_catalogo", x => x.operation_id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "s2_catalogo_fondos",
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
                name: "presupuestos_programa",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    aportes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    otorgado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ofertas_pendientes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    renovaciones_pendientes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desembolsado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cupo_becarios = table.Column<int>(type: "integer", nullable: false),
                    cupos_ocupados = table.Column<int>(type: "integer", nullable: false),
                    actualizado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presupuestos_programa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "programas",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nombre_normalizado = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    periodo_inicio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cupo_becarios = table.Column<int>(type: "integer", nullable: false),
                    duracion_periodos = table.Column<int>(type: "integer", nullable: false),
                    monto_por_periodo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    modalidad_pago = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    alcance = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    permite_acumulacion = table.Column<bool>(type: "boolean", nullable: false),
                    cond_promedio_minimo = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    cond_exige_horas_servicio = table.Column<bool>(type: "boolean", nullable: true),
                    cond_horas_servicio = table.Column<int>(type: "integer", nullable: true),
                    cond_reportes_exigidos = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    cond_duracion_max_pausa_meses = table.Column<int>(type: "integer", nullable: true),
                    cond_descripcion_renovacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    creado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "registro_decisiones",
                schema: "s2_catalogo_fondos",
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
                name: "universidades_aliadas",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nombre_normalizado = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    convenio_terminos = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    convenio_via_datos_academicos = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    contacto_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    contacto_correo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    contacto_telefono = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    registrada_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_universidades_aliadas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "periodos_academicos",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    calendario_academico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    es_actual = table.Column<bool>(type: "boolean", nullable: false),
                    registrado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodos_academicos", x => x.id);
                    table.ForeignKey(
                        name: "FK_periodos_academicos_calendarios_academicos_calendario_acade~",
                        column: x => x.calendario_academico_id,
                        principalSchema: "s2_catalogo_fondos",
                        principalTable: "calendarios_academicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aportes",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    donante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    registrado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aportes", x => x.id);
                    table.ForeignKey(
                        name: "FK_aportes_donantes_donante_id",
                        column: x => x.donante_id,
                        principalSchema: "s2_catalogo_fondos",
                        principalTable: "donantes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cupos_ciclo",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    presupuesto_programa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ciclo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    limite = table.Column<int>(type: "integer", nullable: false),
                    ocupados = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cupos_ciclo", x => x.id);
                    table.ForeignKey(
                        name: "FK_cupos_ciclo_presupuestos_programa_presupuesto_programa_id",
                        column: x => x.presupuesto_programa_id,
                        principalSchema: "s2_catalogo_fondos",
                        principalTable: "presupuestos_programa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reservas_presupuesto",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    presupuesto_programa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    clave_negocio = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    postulacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ciclo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    beca_id = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo_periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    duracion_periodos = table.Column<int>(type: "integer", nullable: false),
                    creada_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    vinculada_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cerrada_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_liberacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    recurso_vinculado_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas_presupuesto", x => x.id);
                    table.ForeignKey(
                        name: "FK_reservas_presupuesto_presupuestos_programa_presupuesto_prog~",
                        column: x => x.presupuesto_programa_id,
                        principalSchema: "s2_catalogo_fondos",
                        principalTable: "presupuestos_programa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "programa_donantes",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    donante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asociado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programa_donantes", x => x.id);
                    table.ForeignKey(
                        name: "FK_programa_donantes_programas_programa_id",
                        column: x => x.programa_id,
                        principalSchema: "s2_catalogo_fondos",
                        principalTable: "programas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "programa_restricciones_elegibilidad",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    universidad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_universidad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    carrera_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programa_restricciones_elegibilidad", x => x.id);
                    table.ForeignKey(
                        name: "FK_programa_restricciones_elegibilidad_programas_programa_id",
                        column: x => x.programa_id,
                        principalSchema: "s2_catalogo_fondos",
                        principalTable: "programas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "carreras_elegibles",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    universidad_aliada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nombre_normalizado = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carreras_elegibles", x => x.id);
                    table.ForeignKey(
                        name: "FK_carreras_elegibles_universidades_aliadas_universidad_aliada~",
                        column: x => x.universidad_aliada_id,
                        principalSchema: "s2_catalogo_fondos",
                        principalTable: "universidades_aliadas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "costos_matricula",
                schema: "s2_catalogo_fondos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    universidad_aliada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    version_registro = table.Column<int>(type: "integer", nullable: false),
                    registrado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_costos_matricula", x => x.id);
                    table.ForeignKey(
                        name: "FK_costos_matricula_universidades_aliadas_universidad_aliada_id",
                        column: x => x.universidad_aliada_id,
                        principalSchema: "s2_catalogo_fondos",
                        principalTable: "universidades_aliadas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_aportes_donante_periodo",
                schema: "s2_catalogo_fondos",
                table: "aportes",
                columns: new[] { "donante_id", "codigo_periodo" });

            migrationBuilder.CreateIndex(
                name: "ix_aportes_programa",
                schema: "s2_catalogo_fondos",
                table: "aportes",
                column: "programa_id");

            migrationBuilder.CreateIndex(
                name: "ix_carreras_elegibles_universidad_nombre_unico",
                schema: "s2_catalogo_fondos",
                table: "carreras_elegibles",
                columns: new[] { "universidad_aliada_id", "nombre_normalizado" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_costos_matricula_universidad_periodo_version",
                schema: "s2_catalogo_fondos",
                table: "costos_matricula",
                columns: new[] { "universidad_aliada_id", "codigo_periodo", "version_registro" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cupos_ciclo_presupuesto_ciclo_unico",
                schema: "s2_catalogo_fondos",
                table: "cupos_ciclo",
                columns: new[] { "presupuesto_programa_id", "ciclo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_donantes_nombre_unico",
                schema: "s2_catalogo_fondos",
                table: "donantes",
                column: "nombre_normalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inicios_inscripciones_universidad_periodo_unico",
                schema: "s2_catalogo_fondos",
                table: "inicios_inscripciones_periodo",
                columns: new[] { "universidad_id", "codigo_periodo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_pendientes",
                schema: "s2_catalogo_fondos",
                table: "outbox_messages",
                columns: new[] { "ProcessedAtUtc", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "ix_periodos_academicos_codigo_unico",
                schema: "s2_catalogo_fondos",
                table: "periodos_academicos",
                columns: new[] { "calendario_academico_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_presupuestos_programa_programa_unico",
                schema: "s2_catalogo_fondos",
                table: "presupuestos_programa",
                column: "programa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_programa_donantes_unico",
                schema: "s2_catalogo_fondos",
                table: "programa_donantes",
                columns: new[] { "programa_id", "donante_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_programa_restricciones_universidad_carrera",
                schema: "s2_catalogo_fondos",
                table: "programa_restricciones_elegibilidad",
                columns: new[] { "programa_id", "universidad_id", "carrera_nombre" });

            migrationBuilder.CreateIndex(
                name: "ix_programas_nombre_unico",
                schema: "s2_catalogo_fondos",
                table: "programas",
                column: "nombre_normalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_correlacion",
                schema: "s2_catalogo_fondos",
                table: "registro_decisiones",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_fecha",
                schema: "s2_catalogo_fondos",
                table: "registro_decisiones",
                column: "ocurrido_en_utc");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_programa",
                schema: "s2_catalogo_fondos",
                table: "registro_decisiones",
                column: "programa_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_propietario",
                schema: "s2_catalogo_fondos",
                table: "registro_decisiones",
                column: "propietario_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_sujeto",
                schema: "s2_catalogo_fondos",
                table: "registro_decisiones",
                columns: new[] { "tipo_sujeto", "sujeto_id" });

            migrationBuilder.CreateIndex(
                name: "ix_reservas_presupuesto_clave_negocio",
                schema: "s2_catalogo_fondos",
                table: "reservas_presupuesto",
                columns: new[] { "presupuesto_programa_id", "clave_negocio" });

            migrationBuilder.CreateIndex(
                name: "ix_reservas_presupuesto_estado_creada",
                schema: "s2_catalogo_fondos",
                table: "reservas_presupuesto",
                columns: new[] { "estado", "creada_en_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_reservas_presupuesto_operation_id_unico",
                schema: "s2_catalogo_fondos",
                table: "reservas_presupuesto",
                column: "operation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_universidades_aliadas_nombre_unico",
                schema: "s2_catalogo_fondos",
                table: "universidades_aliadas",
                column: "nombre_normalizado",
                unique: true);

            // Endurecimiento append-only de registro_decisiones en PostgreSQL (RF-35, HU-35.1 Anexo A).
            migrationBuilder.Sql(
                Ascenso.Messaging.Outbox.RegistroDecisionSqlAppendOnly.GenerarSqlPostgreSql(
                    CatalogoFondosDbContext.Schema));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aportes",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "carreras_elegibles",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "costos_matricula",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "cupos_ciclo",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "inicios_inscripciones_periodo",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "operaciones_idempotentes_catalogo",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "periodos_academicos",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "programa_donantes",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "programa_restricciones_elegibilidad",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "registro_decisiones",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "reservas_presupuesto",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "donantes",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "universidades_aliadas",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "calendarios_academicos",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "programas",
                schema: "s2_catalogo_fondos");

            migrationBuilder.DropTable(
                name: "presupuestos_programa",
                schema: "s2_catalogo_fondos");
        }
    }
}
