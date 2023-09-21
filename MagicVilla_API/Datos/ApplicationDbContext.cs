using MagicVilla_API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_API.Datos
{
    public class ApplicationDbContext : DbContext
    {
        //Esto es lo que se tiene qué hacer para reconocer la inyección de dependencias y tener todo organizado y detallado para poder trabajar con entityframework
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        //aquí le indicamos a la base de datos que debe tomar la clase Villa para crear la tabla Villas dentro de SQLServer
        public DbSet<Villa> Villas { get; set; }
        //con esto creamos la nueva tabla en la base de datos (similar a la línea anterior)
        public DbSet<NumeroVilla> NumeroVillas { get; set; }

        //agregamos nuevos registros a la tabla Villas en la bd de sql
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Villa>().HasData(
                new Villa()
                {
                    Id = 1,
                    Nombre = "Villa Real",
                    Detalle = "Detalle de la villa... ",
                    ImagenUrl = "",
                    Ocupantes = 5,
                    MetrosCuadrados = 50,
                    Tarifa = 150,
                    Amenidad="",
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now
                },
                new Villa()
                {
                    Id = 2,
                    Nombre = "Villa Premium",
                    Detalle = "Detalle de la villa Premium... ",
                    ImagenUrl = "",
                    Ocupantes = 6,
                    MetrosCuadrados = 70,
                    Tarifa = 350,
                    Amenidad = "",
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now
                }
            );
        }
    }
}
