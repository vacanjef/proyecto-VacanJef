using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Models;

namespace VacanJefWeb.Data;

public class VacanJefContext : DbContext
{
    public VacanJefContext(DbContextOptions<VacanJefContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ContactoEmergencia> ContactosEmergencia => Set<ContactoEmergencia>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<TipoServicio> TiposServicio => Set<TipoServicio>();
    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<AccesoCamara> AccesosCamara => Set<AccesoCamara>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // ── Clientes ──────────────────────────────────────────────
        b.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.HasKey(x => x.IdCliente);
            e.Property(x => x.IdCliente).HasColumnName("id_cliente");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Apellido).HasColumnName("apellido");
            e.Property(x => x.Telefono).HasColumnName("telefono");
            e.Property(x => x.Direccion).HasColumnName("direccion");
            e.Property(x => x.Correo).HasColumnName("correo");
            e.Property(x => x.FechaRegistro).HasColumnName("fecha_registro");
            e.Property(x => x.Activo).HasColumnName("activo");
        });

        // ── Contactos_Emergencia ──────────────────────────────────
        b.Entity<ContactoEmergencia>(e =>
        {
            e.ToTable("Contactos_Emergencia");
            e.HasKey(x => x.IdContacto);
            e.Property(x => x.IdContacto).HasColumnName("id_contacto");
            e.Property(x => x.IdCliente).HasColumnName("id_cliente");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Apellido).HasColumnName("apellido");
            e.Property(x => x.Telefono).HasColumnName("telefono");
            e.Property(x => x.Correo).HasColumnName("correo");
            e.Property(x => x.Parentesco).HasColumnName("parentesco");
            e.HasOne(x => x.Cliente).WithMany(c => c.Contactos)
                .HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Mascotas ──────────────────────────────────────────────
        b.Entity<Mascota>(e =>
        {
            e.ToTable("Mascotas");
            e.HasKey(x => x.IdMascota);
            e.Property(x => x.IdMascota).HasColumnName("id_mascota");
            e.Property(x => x.IdCliente).HasColumnName("id_cliente");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Especie).HasColumnName("especie");
            e.Property(x => x.Raza).HasColumnName("raza");
            e.Property(x => x.Sexo).HasColumnName("sexo").HasMaxLength(1);
            e.Property(x => x.FechaNacimiento).HasColumnName("fecha_nacimiento");
            e.Property(x => x.Color).HasColumnName("color");
            e.Property(x => x.PesoKg).HasColumnName("peso_kg").HasColumnType("decimal(5,2)");
            e.Property(x => x.Alergias).HasColumnName("alergias");
            e.Property(x => x.Medicamentos).HasColumnName("medicamentos");
            e.Property(x => x.Observaciones).HasColumnName("observaciones");
            e.Property(x => x.CarnetVacunas).HasColumnName("carnet_vacunas");
            e.Property(x => x.Activo).HasColumnName("activo");
            e.HasOne(x => x.Cliente).WithMany(c => c.Mascotas)
                .HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Cargos ────────────────────────────────────────────────
        b.Entity<Cargo>(e =>
        {
            e.ToTable("Cargos");
            e.HasKey(x => x.IdCargo);
            e.Property(x => x.IdCargo).HasColumnName("id_cargo");
            e.Property(x => x.NombreCargo).HasColumnName("nombre_cargo");
        });

        // ── Empleados ─────────────────────────────────────────────
        b.Entity<Empleado>(e =>
        {
            e.ToTable("Empleados");
            e.HasKey(x => x.IdEmpleado);
            e.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Apellido).HasColumnName("apellido");
            e.Property(x => x.Telefono).HasColumnName("telefono");
            e.Property(x => x.Direccion).HasColumnName("direccion");
            e.Property(x => x.Correo).HasColumnName("correo");
            e.Property(x => x.IdCargo).HasColumnName("id_cargo");
            e.Property(x => x.Salario).HasColumnName("salario").HasColumnType("decimal(12,2)");
            e.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso");
            e.Property(x => x.Activo).HasColumnName("activo");
            e.Property(x => x.EmergenciaNombre).HasColumnName("emergencia_nombre");
            e.Property(x => x.EmergenciaTelefono).HasColumnName("emergencia_telefono");
            e.Property(x => x.EmergenciaParentesco).HasColumnName("emergencia_parentesco");
            e.Property(x => x.TurnoInicio).HasColumnName("turno_inicio");
            e.Property(x => x.TurnoFin).HasColumnName("turno_fin");
            e.Property(x => x.DiasLaborales).HasColumnName("dias_laborales").HasMaxLength(40);
            e.HasOne(x => x.Cargo).WithMany(c => c.Empleados)
                .HasForeignKey(x => x.IdCargo).OnDelete(DeleteBehavior.SetNull);
        });

        // ── Tipos_Servicio ────────────────────────────────────────
        b.Entity<TipoServicio>(e =>
        {
            e.ToTable("Tipos_Servicio");
            e.HasKey(x => x.IdTipoServicio);
            e.Property(x => x.IdTipoServicio).HasColumnName("id_tipo_servicio");
            e.Property(x => x.NombreServicio).HasColumnName("nombre_servicio");
            e.Property(x => x.Descripcion).HasColumnName("descripcion");
            e.Property(x => x.PrecioBase).HasColumnName("precio_base").HasColumnType("decimal(12,2)");
        });

        // ── Metodos_Pago ──────────────────────────────────────────
        b.Entity<MetodoPago>(e =>
        {
            e.ToTable("Metodos_Pago");
            e.HasKey(x => x.IdMetodoPago);
            e.Property(x => x.IdMetodoPago).HasColumnName("id_metodo_pago");
            e.Property(x => x.NombreMetodo).HasColumnName("nombre_metodo");
        });

        // ── Reservas ──────────────────────────────────────────────
        b.Entity<Reserva>(e =>
        {
            e.ToTable("Reservas");
            e.HasKey(x => x.IdReserva);
            e.Property(x => x.IdReserva).HasColumnName("id_reserva");
            e.Property(x => x.IdCliente).HasColumnName("id_cliente");
            e.Property(x => x.IdMascota).HasColumnName("id_mascota");
            e.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
            e.Property(x => x.IdTipoServicio).HasColumnName("id_tipo_servicio");
            e.Property(x => x.FechaInicio).HasColumnName("fecha_inicio");
            e.Property(x => x.FechaFin).HasColumnName("fecha_fin");
            e.Property(x => x.EstadoReserva).HasColumnName("estado_reserva").HasMaxLength(20);
            e.Property(x => x.Observaciones).HasColumnName("observaciones");
            e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion");

            e.HasOne(x => x.Cliente).WithMany(c => c.Reservas).HasForeignKey(x => x.IdCliente);
            e.HasOne(x => x.Mascota).WithMany(m => m.Reservas).HasForeignKey(x => x.IdMascota);
            e.HasOne(x => x.Empleado).WithMany(emp => emp.ReservasAsignadas)
                .HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.TipoServicio).WithMany(t => t.Reservas).HasForeignKey(x => x.IdTipoServicio);
        });

        // ── Facturas ──────────────────────────────────────────────
        b.Entity<Factura>(e =>
        {
            e.ToTable("Facturas");
            e.HasKey(x => x.IdFactura);
            e.Property(x => x.IdFactura).HasColumnName("id_factura");
            e.Property(x => x.IdCliente).HasColumnName("id_cliente");
            e.Property(x => x.IdReserva).HasColumnName("id_reserva");
            e.Property(x => x.IdMetodoPago).HasColumnName("id_metodo_pago");
            e.Property(x => x.FechaEmision).HasColumnName("fecha_emision");
            e.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(12,2)");
            e.Property(x => x.Descuento).HasColumnName("descuento").HasColumnType("decimal(12,2)");
            e.Property(x => x.ImpuestoPct).HasColumnName("impuesto_pct").HasColumnType("decimal(5,2)");
            // Columna calculada PERSISTED en SQL Server: EF nunca debe escribirla.
            e.Property(x => x.Total).HasColumnName("total").HasColumnType("decimal(12,2)")
                .ValueGeneratedOnAddOrUpdate().Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
            e.Property(x => x.EstadoPago).HasColumnName("estado_pago").HasMaxLength(20);
            e.Property(x => x.Notas).HasColumnName("notas");

            e.HasOne(x => x.Cliente).WithMany(c => c.Facturas).HasForeignKey(x => x.IdCliente);
            e.HasOne(x => x.Reserva).WithOne(r => r.Factura!).HasForeignKey<Factura>(x => x.IdReserva);
            e.HasOne(x => x.MetodoPago).WithMany(m => m.Facturas).HasForeignKey(x => x.IdMetodoPago);
        });

        // ── Usuarios (tabla nueva para autenticación) ────────────
        b.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.IdUsuario).HasColumnName("id_usuario");
            e.Property(x => x.Correo).HasColumnName("correo").HasMaxLength(150);
            e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            e.Property(x => x.Rol).HasColumnName("rol").HasMaxLength(20);
            e.Property(x => x.IdCliente).HasColumnName("id_cliente");
            e.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
            e.Property(x => x.Activo).HasColumnName("activo");
            e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion");
            e.HasIndex(x => x.Correo).IsUnique();
            e.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.SetNull);
        });

        // ── AccesosCamara (tokens de acceso por reserva) ─────────
        b.Entity<AccesoCamara>(e =>
        {
            e.ToTable("AccesosCamara");
            e.HasKey(x => x.IdAcceso);
            e.Property(x => x.IdAcceso).HasColumnName("id_acceso");
            e.Property(x => x.IdReserva).HasColumnName("id_reserva");
            e.Property(x => x.Token).HasColumnName("token").HasMaxLength(64);
            e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion");
            e.Property(x => x.FechaExpiracion).HasColumnName("fecha_expiracion");
            e.Property(x => x.Activo).HasColumnName("activo");
            e.HasIndex(x => x.Token).IsUnique();
            e.HasOne(x => x.Reserva).WithMany()
                .HasForeignKey(x => x.IdReserva).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
