
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Prueba2Hotel
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<Cita> Cita { get; set; }
        public DbSet<Paciente> Paciente { get; set; }
        public DbSet<Medico> Medico { get; set; }
        public DbSet<Consultorio> Consultorio { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Medico>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Apellido).IsRequired().HasMaxLength(100);

            });

            modelBuilder.Entity<Paciente>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(r => r.Nombre).IsRequired();
                entity.Property(c => c.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Correo).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Direccion).IsRequired().HasMaxLength(500);
            });

            modelBuilder.Entity<Cita>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Fecha).IsRequired();
                entity.Property(r => r.Hora).IsRequired();
                entity.Property(r => r.Consultorio).IsRequired();

                entity.HasOne(r => r.Paciente)
                      .WithMany(p => p.Reservas)
                      .HasForeignKey(r => r.PacienteId);

                entity.HasOne(r => r.Medico)
                      .WithMany(m => m.Reservas)
                      .HasForeignKey(r => r.DoctorId);
            });

            modelBuilder.Entity<Consultorio>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Numero).IsRequired();
                entity.Property(s => s.Piso).IsRequired();

            });
        }
    }

    public class Medico
    {
        [JsonIgnore]
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public required string Especialidad { get; set; }
        [JsonIgnore]
        public List<Cita>? Reservas { get; set; }
    }

    public class Paciente
    {
        [JsonIgnore]
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public required string? Direccion { get; set; }
        public required string Correo { get; set; }

        [JsonIgnore]
        public List<Cita>? Reservas { get; set; }
    }

    public class Cita
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public required int PacienteId { get; set; }
        public required int DoctorId { get; set; }
        public required DateTime? Fecha { get; set; }
        //hora
        public required TimeOnly? Hora { get; set; }
        public required int Consultorio { get; set; }

        [JsonIgnore]
        public Paciente? Paciente { get; set; }
        [JsonIgnore]
        public Medico? Medico { get; set; }
    }

    public class Consultorio
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public required int? Numero { get; set; }
        public required int? Piso { get; set; }
    }
}