﻿using MagicVilla_API.Datos;
using MagicVilla_API.Modelos;
using MagicVilla_API.Repositorio.IRepositorio;

namespace MagicVilla_API.Repositorio
{
    public class NumeroVillaRepositorio : Repositorio<NumeroVilla>, INumeroVillaRepositorio
    {

        private readonly ApplicationDbContext _db;
        public NumeroVillaRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        

        public async Task<NumeroVilla> Actualizar(NumeroVilla entidadVilla)
        {
            entidadVilla.FechaActualizacion = DateTime.Now;
            _db.NumeroVillas.Update(entidadVilla);
            await _db.SaveChangesAsync();
            return entidadVilla;
        }
    }
}
