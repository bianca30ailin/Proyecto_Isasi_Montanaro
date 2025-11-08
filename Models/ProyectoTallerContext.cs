using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Proyecto_Isasi_Montanaro.Models;

public partial class ProyectoTallerContext : DbContext
{
    public ProyectoTallerContext()
    {
    }

    public ProyectoTallerContext(DbContextOptions<ProyectoTallerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Ciudad> Ciudads { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<DetalleVentaProducto> DetalleVentaProductos { get; set; }

    public virtual DbSet<Direccion> Direccions { get; set; }

    public virtual DbSet<Envio> Envios { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Provincium> Provincia { get; set; }

    public virtual DbSet<TipoUsuario> TipoUsuarios { get; set; }

    public virtual DbSet<Transporte> Transportes { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }

    public DbSet<EstadoVenta> EstadoVenta { get; set; }

    public virtual DbSet<FormaPago> FormaPago { get; set; } = null!;

    public virtual DbSet<NotaCredito> NotaCredito { get; set; }
    public virtual DbSet<DetalleNotaCredito> DetalleNotaCredito { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__categori__CD54BC5A610BB531");

            entity.ToTable("categoria");

            entity.Property(e => e.IdCategoria)
                
                .HasColumnName("id_categoria");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Ciudad>(entity =>
        {
            entity.HasKey(e => e.IdCiudad).HasName("PK__ciudad__B7DC4CD5C121E7BB");

            entity.ToTable("ciudad");

            entity.Property(e => e.IdCiudad)
                
                .HasColumnName("id_ciudad");
            entity.Property(e => e.IdProvincia).HasColumnName("id_provincia");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdProvinciaNavigation).WithMany(p => p.Ciudads)
                .HasForeignKey(d => d.IdProvincia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ciudad__id_provi__4BAC3F29");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.DniCliente).HasName("PK__cliente__F53D4BA5B5774DF0");

            entity.ToTable("cliente");

            entity.Property(e => e.DniCliente)
                .ValueGeneratedNever()
                .HasColumnName("dni_cliente");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<DetalleVentaProducto>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PK__detalle___4F1332DE0C4A86F4");

            entity.ToTable("detalle_venta_producto");

            entity.Property(e => e.IdDetalle)
                
                .HasColumnName("id_detalle");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.IdNroVenta).HasColumnName("id_nro_venta");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal");

            entity.HasOne(d => d.IdNroVentaNavigation).WithMany(p => p.DetalleVentaProductos)
                .HasForeignKey(d => d.IdNroVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__detalle_v__id_nr__71D1E811");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleVentaProductos)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__detalle_v__id_pr__72C60C4A");
        });

        modelBuilder.Entity<Direccion>(entity =>
        {
            entity.HasKey(e => e.IdDireccion).HasName("PK__direccio__25C35D07EA902764");

            entity.ToTable("direccion");

            entity.Property(e => e.IdDireccion)
                
                .HasColumnName("id_direccion");
            entity.Property(e => e.Altura).HasColumnName("altura");
            entity.Property(e => e.IdCiudad).HasColumnName("id_ciudad");
            entity.Property(e => e.NombreCalle)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombreCalle");
            // NUEVO campo dni_cliente (nullable)
            entity.Property<int?>("DniCliente").HasColumnName("dni_cliente");

            entity.HasOne(d => d.IdCiudadNavigation).WithMany(p => p.Direccions)
                .HasForeignKey(d => d.IdCiudad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__direccion__id_ci__4E88ABD4");

            // Relación con Cliente (1:N opcional)
            entity.HasOne(d => d.Cliente)
                .WithMany(p => p.Direcciones)
                .HasForeignKey(d => d.DniCliente)
                .HasConstraintName("FK_direccion_cliente");
        });

        modelBuilder.Entity<Envio>(entity =>
        {
            entity.HasKey(e => e.IdEnvio).HasName("PK__envio__8C48C8CAEABDBD3A");

            entity.ToTable("envio");

            entity.Property(e => e.IdEnvio)
                
                .HasColumnName("id_envio");
            entity.Property(e => e.Costo).HasColumnName("costo");
            entity.Property(e => e.FechaDespacho).HasColumnName("fehca_despacho");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdNroVenta).HasColumnName("id_nro_venta");
            entity.Property(e => e.IdTransporte).HasColumnName("id_transporte");
            entity.Property(e => e.NumSeguimiento)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("num_seguimiento");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Envios)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__envio__id_estado__66603565");

            entity.HasOne(d => d.IdNroVentaNavigation).WithMany(p => p.Envios)
                .HasForeignKey(d => d.IdNroVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__envio__id_nro_ve__68487DD7");

            entity.HasOne(d => d.IdTransporteNavigation).WithMany(p => p.Envios)
                .HasForeignKey(d => d.IdTransporte)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__envio__id_transp__6754599E");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__estado__86989FB20C055FB1");

            entity.ToTable("estado");

            entity.Property(e => e.IdEstado)
                
                .HasColumnName("id_estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__producto__FF341C0D810F0BF8");

            entity.ToTable("producto");

            entity.Property(e => e.IdProducto)
                
                .HasColumnName("id_producto");
            entity.Property(e => e.Baja)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("baja");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio).HasColumnName("precio");
            entity.Property(e => e.StockMinimo).HasColumnName("stock_minimo");
            entity.Property(e => e.IdUsuarioCreacion)
                .HasColumnName("id_usuario_creacion");

            entity.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasDefaultValueSql("GETDATE()");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__producto__id_cat__6B24EA82");

            entity.HasOne(d => d.UsuarioCreacion)
                .WithMany(p => p.ProductosCreados)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_producto_usuario_creacion");
        });

        modelBuilder.Entity<Provincium>(entity =>
        {
            entity.HasKey(e => e.IdProvincia).HasName("PK__provinci__66C18BFDA3FE9356");

            entity.ToTable("provincia");

            entity.Property(e => e.IdProvincia)
                
                .HasColumnName("id_provincia");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<TipoUsuario>(entity =>
        {
            entity.HasKey(e => e.IdTipoUsuario).HasName("PK__tipo_usu__B17D78C8165631FD");

            entity.ToTable("tipo_usuario");

            entity.Property(e => e.IdTipoUsuario)
                .ValueGeneratedNever()
                .HasColumnName("id_tipo_usuario");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("tipo");

            entity.HasMany(d => d.IdUsuarios).WithMany(p => p.IdTipoUsuarios)
                .UsingEntity<Dictionary<string, object>>(
                    "FuncionUsuario",
                    r => r.HasOne<Usuario>().WithMany()
                        .HasForeignKey("IdUsuario")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__funcion_u__id_us__6EF57B66"),
                    l => l.HasOne<TipoUsuario>().WithMany()
                        .HasForeignKey("IdTipoUsuario")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__funcion_u__id_ti__6E01572D"),
                    j =>
                    {
                        j.HasKey("IdTipoUsuario", "IdUsuario").HasName("PK__funcion___759E98823F5F8A42");
                        j.ToTable("funcion_usuario");
                        j.IndexerProperty<int>("IdTipoUsuario").HasColumnName("id_tipo_usuario");
                        j.IndexerProperty<int>("IdUsuario").HasColumnName("id_usuario");
                    });
        });
        modelBuilder.Entity<TipoUsuario>().HasData(
            new TipoUsuario
            {
                IdTipoUsuario = 1,
                Tipo = "Admin",
                Descripcion = "Usuario con acceso total a usuario y solo lectura para las demas áreas"
            },
            new TipoUsuario
            {
                IdTipoUsuario = 2,
                Tipo = "Ventas",
                Descripcion = "Usuario que registra las ventas"
            },
            new TipoUsuario
            {
                IdTipoUsuario = 3,
                Tipo = "Logistica",
                Descripcion = "Usuario que se encarga de actualizar o registrar datos de los envios"
            },
            new TipoUsuario
            {
                IdTipoUsuario = 4,
                Tipo = "Inventario",
                Descripcion = "Usuario que se encarga del inventario"
            },
            new TipoUsuario
            {
                IdTipoUsuario = 5,
                Tipo = "Supervisor",
                Descripcion = "Usuario con permisos para generar informes"
            }

        );

        modelBuilder.Entity<Transporte>(entity =>
        {
            entity.HasKey(e => e.IdTransporte).HasName("PK__transpor__7AC9B3AEB3C4D8CC");

            entity.ToTable("transporte");

            entity.Property(e => e.IdTransporte)
                
                .HasColumnName("id_transporte");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__usuario__4E3E04ADF6B4BB0C");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.Email, "UQ__usuario__AB6E61642F811227").IsUnique();

            entity.HasIndex(e => e.Dni, "UQ__usuario__D87608A7AF6E4AC8").IsUnique();

            entity.Property(e => e.IdUsuario)
                
                .HasColumnName("id_usuario");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Baja)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("baja");
            entity.Property(e => e.Contraseña)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("contraseña");
            entity.Property(e => e.Dni).HasColumnName("dni");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Direccion)
                .HasMaxLength(100) 
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20) // Un tamaño razonable para un teléfono
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.IdNroVenta).HasName("PK__venta__D47121A5607A29DB");

            entity.ToTable("venta");

            entity.Property(e => e.IdNroVenta)
                
                .HasColumnName("id_nro_venta");
            entity.Property(e => e.DniCliente).HasColumnName("dni_cliente");
            entity.Property(e => e.FechaHora).HasColumnName("fechaHora");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.IdFormaPago).HasColumnName("id_forma_pago").HasDefaultValue(1); ;
            entity.Property(e => e.TotalCuotas).HasColumnName("total_cuotas");

            entity.HasOne(d => d.DniClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.DniCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__venta__dni_clien__628FA481");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__venta__id_usuari__6383C8BA");

            entity.HasOne(d => d.IdFormaPagoNavigation)
                  .WithMany(p => p.Ventas)
                  .HasForeignKey(d => d.IdFormaPago)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK__venta__id_forma_pago");
        });

        modelBuilder.Entity<EstadoVenta>().HasData(
             new EstadoVenta
             {
                 IdEstadoVenta = 1,
                 NombreEstado = "Activa",
                 Descripcion = "Venta confirmada y en curso"
             },
            new EstadoVenta
            {
                IdEstadoVenta = 2,
                NombreEstado = "Pendiente de pago",
                Descripcion = "Venta registrada, esperando confirmación del pago"
            },
            new EstadoVenta
            {
                IdEstadoVenta = 3,
                NombreEstado = "Cancelada",
                Descripcion = "Venta anulada por el cliente o vendedor"
            },
            new EstadoVenta
            {
                IdEstadoVenta = 4,
                NombreEstado = "Completada",
                Descripcion = "Venta finalizada con entrega y pago confirmados"
            }
        );

        modelBuilder.Entity<Estado>().HasData(
            new Estado
            {
                IdEstado = 1,
                Nombre = "En preparación",
                Descripcion = "Pedido confirmado, preparando para envío"
            },
            new Estado
            {
                IdEstado = 2,
                Nombre = "En camino",
                Descripcion = "Pedido despachado, en tránsito"
            },
            new Estado
            {
                IdEstado = 3,
                Nombre = "Entregado",
                Descripcion = "Pedido recibido por el cliente"
            },
            new Estado
            {
                IdEstado = 4,
                Nombre = "Pendiente",
                Descripcion = "A la espera de procesamiento"
            },
            new Estado
            {
                IdEstado = 5,
                Nombre = "Cancelado",
                Descripcion = "Envío cancelado antes del despacho"
            },
            new Estado
            {
                IdEstado = 6,
                Nombre = "Devuelto",
                Descripcion = "El pedido fue devuelto al origen"
            }
        );

        modelBuilder.Entity<FormaPago>(entity =>
        {
            entity.HasKey(e => e.IdFormaPago).HasName("PK__forma_pa__DD4926E39FCE6D1E");

            entity.ToTable("forma_pago");

            entity.Property(e => e.IdFormaPago)
                .HasColumnName("id_forma_pago");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<FormaPago>().HasData(
            new FormaPago
            {
                IdFormaPago = 1,
                Nombre = "Efectivo",
                Descripcion = "Pago en efectivo"
            },
            new FormaPago
            {
                IdFormaPago = 2,
                Nombre = "Débito",
                Descripcion = "Pago con tarjeta de débito"
            },
            new FormaPago
            {
                IdFormaPago = 3,
                Nombre = "Crédito",
                Descripcion = "Pago con tarjeta de crédito con 10% de recargo"
            },
            new FormaPago
            {
                IdFormaPago = 4,
                Nombre = "Transferencia",
                Descripcion = "Pago mediante transferencia bancaria"
            }
        );
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
