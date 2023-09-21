using MagicVilla_API.Datos;
using MagicVilla_API.Modelos;
using MagicVilla_API.Repositorio.IRepositorio;

namespace MagicVilla_API.Repositorio
{
    public class VillaRepositorio : Repositorio<Villa>, IVillaRepositorio
    {

        private readonly ApplicationDbContext _db;
        public VillaRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        

        public async Task<Villa> Actualizar(Villa entidadVilla)
        {
            entidadVilla.FechaActualizacion = DateTime.Now;
            _db.Villas.Update(entidadVilla);
            await _db.SaveChangesAsync();
            return entidadVilla;
        }
    }
}
