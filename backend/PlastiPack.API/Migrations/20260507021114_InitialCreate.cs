using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlastiPack.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    nit = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "selladoras",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_selladoras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    rol_id = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_rol_id",
                        column: x => x.rol_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: true),
                    vendedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destino = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedidos", x => x.id);
                    table.ForeignKey(
                        name: "FK_pedidos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_pedidos_usuarios_vendedor_id",
                        column: x => x.vendedor_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "planillas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    selladora_id = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planillas", x => x.id);
                    table.ForeignKey(
                        name: "FK_planillas_selladoras_selladora_id",
                        column: x => x.selladora_id,
                        principalTable: "selladoras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_planillas_usuarios_creado_por",
                        column: x => x.creado_por,
                        principalTable: "usuarios",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "referencias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    referencia_corta = table.Column<string>(type: "text", nullable: true),
                    nombre = table.Column<string>(type: "text", nullable: true),
                    grupo = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: false),
                    tipo_producto = table.Column<string>(type: "text", nullable: true),
                    materia_prima = table.Column<string>(type: "text", nullable: true),
                    color = table.Column<string>(type: "text", nullable: true),
                    troquelado = table.Column<string>(type: "text", nullable: true),
                    ancho = table.Column<decimal>(type: "numeric", nullable: true),
                    fuelle_izquierdo = table.Column<decimal>(type: "numeric", nullable: true),
                    fuelle_derecho = table.Column<decimal>(type: "numeric", nullable: true),
                    alto = table.Column<decimal>(type: "numeric", nullable: true),
                    fuelle_superior = table.Column<decimal>(type: "numeric", nullable: true),
                    fuelle_fondo = table.Column<decimal>(type: "numeric", nullable: true),
                    calibre = table.Column<decimal>(type: "numeric", nullable: true),
                    impresion = table.Column<bool>(type: "boolean", nullable: false),
                    colores_impresion = table.Column<string>(type: "text", nullable: true),
                    tipo_cliente = table.Column<string>(type: "text", nullable: true),
                    tipo_impresion = table.Column<string>(type: "text", nullable: true),
                    tipo_sellado = table.Column<string>(type: "text", nullable: true),
                    tratado_cara = table.Column<string>(type: "text", nullable: true),
                    medida = table.Column<string>(type: "text", nullable: true),
                    costo_produccion = table.Column<decimal>(type: "numeric", nullable: true),
                    impuesto = table.Column<decimal>(type: "numeric", nullable: true),
                    codigo_barras = table.Column<string>(type: "text", nullable: true),
                    presentacion = table.Column<string>(type: "text", nullable: true),
                    unidad_medida = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referencias", x => x.id);
                    table.ForeignKey(
                        name: "FK_referencias_usuarios_creado_por",
                        column: x => x.creado_por,
                        principalTable: "usuarios",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pedido_historial_estado",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pedido_id = table.Column<int>(type: "integer", nullable: false),
                    estado_anterior = table.Column<string>(type: "text", nullable: true),
                    estado_nuevo = table.Column<string>(type: "text", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    observacion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido_historial_estado", x => x.id);
                    table.ForeignKey(
                        name: "FK_pedido_historial_estado_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pedido_historial_estado_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "inventario",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    referencia_id = table.Column<int>(type: "integer", nullable: false),
                    stock_disponible = table.Column<int>(type: "integer", nullable: false),
                    stock_reservado = table.Column<int>(type: "integer", nullable: false),
                    ultima_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventario", x => x.id);
                    table.ForeignKey(
                        name: "FK_inventario_referencias_referencia_id",
                        column: x => x.referencia_id,
                        principalTable: "referencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ordenes_produccion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pedido_id = table.Column<int>(type: "integer", nullable: false),
                    referencia_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad_requerida = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    creado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordenes_produccion", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordenes_produccion_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ordenes_produccion_referencias_referencia_id",
                        column: x => x.referencia_id,
                        principalTable: "referencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ordenes_produccion_usuarios_creado_por",
                        column: x => x.creado_por,
                        principalTable: "usuarios",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pedido_detalle",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pedido_id = table.Column<int>(type: "integer", nullable: false),
                    referencia_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido_detalle", x => x.id);
                    table.ForeignKey(
                        name: "FK_pedido_detalle_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pedido_detalle_referencias_referencia_id",
                        column: x => x.referencia_id,
                        principalTable: "referencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "precios_referencia",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    referencia_id = table.Column<int>(type: "integer", nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: false),
                    precio = table.Column<decimal>(type: "numeric", nullable: false),
                    vigente_desde = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precios_referencia", x => x.id);
                    table.ForeignKey(
                        name: "FK_precios_referencia_referencias_referencia_id",
                        column: x => x.referencia_id,
                        principalTable: "referencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rollos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    numero_rollo = table.Column<string>(type: "text", nullable: false),
                    referencia_id = table.Column<int>(type: "integer", nullable: false),
                    tiene_impresion = table.Column<bool>(type: "boolean", nullable: false),
                    marca_impresa = table.Column<string>(type: "text", nullable: true),
                    peso_kg = table.Column<decimal>(type: "numeric", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rollos", x => x.id);
                    table.ForeignKey(
                        name: "FK_rollos_referencias_referencia_id",
                        column: x => x.referencia_id,
                        principalTable: "referencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orden_procesos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    orden_produccion_id = table.Column<int>(type: "integer", nullable: false),
                    nombre_proceso = table.Column<string>(type: "text", nullable: false),
                    secuencia = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orden_procesos", x => x.id);
                    table.ForeignKey(
                        name: "FK_orden_procesos_ordenes_produccion_orden_produccion_id",
                        column: x => x.orden_produccion_id,
                        principalTable: "ordenes_produccion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "planilla_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    planilla_id = table.Column<int>(type: "integer", nullable: false),
                    orden_proceso_id = table.Column<int>(type: "integer", nullable: false),
                    posicion = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planilla_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_planilla_items_orden_procesos_orden_proceso_id",
                        column: x => x.orden_proceso_id,
                        principalTable: "orden_procesos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_planilla_items_planillas_planilla_id",
                        column: x => x.planilla_id,
                        principalTable: "planillas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "registros_sellado",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    planilla_item_id = table.Column<int>(type: "integer", nullable: false),
                    operario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rollo_id = table.Column<int>(type: "integer", nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    cantidad_unidades = table.Column<int>(type: "integer", nullable: true),
                    peso_desperdicio = table.Column<decimal>(type: "numeric", nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registros_sellado", x => x.id);
                    table.ForeignKey(
                        name: "FK_registros_sellado_planilla_items_planilla_item_id",
                        column: x => x.planilla_item_id,
                        principalTable: "planilla_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registros_sellado_rollos_rollo_id",
                        column: x => x.rollo_id,
                        principalTable: "rollos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registros_sellado_usuarios_operario_id",
                        column: x => x.operario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inventario_referencia_id",
                table: "inventario",
                column: "referencia_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orden_procesos_orden_produccion_id",
                table: "orden_procesos",
                column: "orden_produccion_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordenes_produccion_creado_por",
                table: "ordenes_produccion",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "IX_ordenes_produccion_pedido_id",
                table: "ordenes_produccion",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordenes_produccion_referencia_id",
                table: "ordenes_produccion",
                column: "referencia_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_detalle_pedido_id",
                table: "pedido_detalle",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_detalle_referencia_id",
                table: "pedido_detalle",
                column: "referencia_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_historial_estado_pedido_id",
                table: "pedido_historial_estado",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_historial_estado_usuario_id",
                table: "pedido_historial_estado",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_cliente_id",
                table: "pedidos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_vendedor_id",
                table: "pedidos",
                column: "vendedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_planilla_items_orden_proceso_id",
                table: "planilla_items",
                column: "orden_proceso_id");

            migrationBuilder.CreateIndex(
                name: "IX_planilla_items_planilla_id",
                table: "planilla_items",
                column: "planilla_id");

            migrationBuilder.CreateIndex(
                name: "IX_planillas_creado_por",
                table: "planillas",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "IX_planillas_selladora_id",
                table: "planillas",
                column: "selladora_id");

            migrationBuilder.CreateIndex(
                name: "IX_precios_referencia_referencia_id",
                table: "precios_referencia",
                column: "referencia_id");

            migrationBuilder.CreateIndex(
                name: "IX_referencias_creado_por",
                table: "referencias",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "IX_registros_sellado_operario_id",
                table: "registros_sellado",
                column: "operario_id");

            migrationBuilder.CreateIndex(
                name: "IX_registros_sellado_planilla_item_id",
                table: "registros_sellado",
                column: "planilla_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_registros_sellado_rollo_id",
                table: "registros_sellado",
                column: "rollo_id");

            migrationBuilder.CreateIndex(
                name: "IX_rollos_referencia_id",
                table: "rollos",
                column: "referencia_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_rol_id",
                table: "usuarios",
                column: "rol_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventario");

            migrationBuilder.DropTable(
                name: "pedido_detalle");

            migrationBuilder.DropTable(
                name: "pedido_historial_estado");

            migrationBuilder.DropTable(
                name: "precios_referencia");

            migrationBuilder.DropTable(
                name: "registros_sellado");

            migrationBuilder.DropTable(
                name: "planilla_items");

            migrationBuilder.DropTable(
                name: "rollos");

            migrationBuilder.DropTable(
                name: "orden_procesos");

            migrationBuilder.DropTable(
                name: "planillas");

            migrationBuilder.DropTable(
                name: "ordenes_produccion");

            migrationBuilder.DropTable(
                name: "selladoras");

            migrationBuilder.DropTable(
                name: "pedidos");

            migrationBuilder.DropTable(
                name: "referencias");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
