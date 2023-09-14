using MagicVilla_API.Datos;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {


        //aplicando logger para tratamiento de errores o información
        private readonly ILogger<VillaController> _logger;

        //inyectamos el DbContext para acceder a los datos de la db
        private readonly ApplicationDbContext _db;


        //inyectar el dbContext dentro del controlador
        public VillaController(ILogger<VillaController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            
            //variable de nuestro DbContext para comunicarnos con la base de datos
            _db = db;
        }

        //De esta forma trabajamos directamente con el modelo o la clase Villa, esto se debe modificar
        //para que se trabaje con la clase DTO del modelo, es decir, con VillaDto
        /*public IEnumerable<Villa> GetVillas() { 
            return new List<Villa>
            {
                new Villa { Id = 1, Nombre="Vista a la piscina"},
                new Villa { Id = 2,Nombre="Vista a la playa"}
            };
        }*/

        //ENDPOINT que obtiene listado completo de villas
        //El ActionResult indica un código de estado, si el código de respuesta es 200 indica que fue
        //exitoso


        [HttpGet]
        //documentamos los endpoint en caso de error o éxito
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDto>> GetVillas()
        {
            //aquí retornamos la información de manera directa, pero lo cambiamos como está en la 
            //siguiente línea después de los comentarios
            /*return new List<VillaDto>
            {
                new VillaDto { Id = 1, Nombre="Vista a la piscina"},
                new VillaDto { Id = 2, Nombre="Vista a la playa"}
            };*/

            //haciendo uso del logger para enviar mensaje informativo
            _logger.LogInformation("Obtener las villas");
            //de esta manera consultamos los datos que trae la lista en VillaStore (que simula una tabla en la base de datos)
            //return Ok(VillaStore.villaList);

            //de esta manera hacemos uso de DbContext para extraer los datos directamente de nuestra BD
            return Ok(_db.Villas.ToList());

        }

        //Nuevo ENDPOINT que realiza búsqueda individual de villas
        //El ActionResult indica un código de estado, si el código de respuesta es 200 indica que fue
        //exitoso, con el ActionResult podemos realizar diversas validaciones como las siguientes
        [HttpGet("id:int", Name = "GetNewVilla")]
        //documentar los estatus e código enviados a través del ActionResult
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDto> GetVilla(int id)
        {
            if(id == 0)
            {
                _logger.LogError("Error al traer villa con Id " + id);
                return BadRequest();
            }

            //de esta forma traemos la información de la lista de VillaStore
            //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);

            //de esta forma accedemos a la información de la base de datos mediante el DbContext
            var villa = _db.Villas.FirstOrDefault(v => v.Id == id);

            if (villa == null)
            {
                return NotFound();
            }

            //con esta función obtenemos el primer registro encontrado
            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VillaDto> CrearVilla([FromBody] VillaDto villaDto)
        {
            //validamos el modelo antes de guardar datos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //validación personalizada tomando los datos de la tabla ficticia villastore
            //if(VillaStore.villaList.FirstOrDefault(v => v.Nombre.ToLower() == villaDto.Nombre.ToLower()) != null)

            //validación tomando la información directo de la BD mediante nuestro dbcontext
            if (_db.Villas.FirstOrDefault(v => v.Nombre.ToLower() == villaDto.Nombre.ToLower()) != null)
            {
                ModelState.AddModelError("NombreExiste", "Ya existe una villa con ese nombre.");
                return BadRequest(ModelState);
            }

            if (villaDto == null){ return BadRequest(villaDto); }

            if (villaDto.Id > 0) { return StatusCode(StatusCodes.Status500InternalServerError); }

            /*villaDto.Id = VillaStore.villaList.OrderByDescending(v => v.Id).FirstOrDefault().Id + 1;
            VillaStore.villaList.Add(villaDto);*/

            //creamos el modelo para insertar datos a la base
            Villa modelo = new()
            {
                Nombre = villaDto.Nombre,
                Detalle = villaDto.Detalle,
                ImagenUrl = villaDto.ImagenUrl,
                Ocupantes = villaDto.Ocupantes,
                Tarifa = villaDto.Tarifa,
                MetrosCuadrados = villaDto.MetrosCuadrados,
                Amenidad = villaDto.Amenidad
            };

            //con estas líneas hacemos el insert a la base de datos
            _db.Villas.Add(modelo);
            _db.SaveChanges();


            //return Ok(villaDto);
            //para crear el registro llamamos al http GetNewVilla y enviamos los parámetros necesarios
            //que se indican 
            return CreatedAtRoute("GetNewVilla", new {id=villaDto.Id}, villaDto);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVilla(int id)
        {
            if(id == 0) { return BadRequest(); }

            //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);

            var villa = _db.Villas.FirstOrDefault(v => v.Id == id);

            if (villa == null) 
            { 
                return NotFound();
            }

            //VillaStore.villaList.Remove(villa);
            
            //eliminamos la villa por medio del dbContext
            _db.Villas.Remove(villa);
            _db.SaveChanges();

            //siempre que trabajamos con delete se regresa esto
            return NoContent();
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateVilla(int id, [FromBody] VillaDto villaDto) 
        { 
            if(villaDto == null || id != villaDto.Id){ return BadRequest(); }

            /*var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            villa.Nombre= villaDto.Nombre;
            villa.Ocupantes= villaDto.Ocupantes;
            villa.MetrosCuadrados = villaDto.MetrosCuadrados;*/

            Villa modelo = new()
            {
                Id = villaDto.Id,
                Nombre = villaDto.Nombre,
                Detalle = villaDto.Detalle,
                ImagenUrl = villaDto.ImagenUrl,
                Ocupantes = villaDto.Ocupantes,
                Tarifa = villaDto.Tarifa,
                MetrosCuadrados = villaDto.MetrosCuadrados,
                Amenidad = villaDto.Amenidad
            };
            
            _db.Villas.Update(modelo);
            _db.SaveChanges();

            return NoContent();
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDto> patchDto)
        {
            if (patchDto == null || id == 0) { return BadRequest(); }

            //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);

            var villa = _db.Villas.AsNoTracking().FirstOrDefault(v => v.Id == id);

            VillaDto villaDto = new()
            {
                Id = villa.Id,
                Nombre = villa.Nombre,
                Detalle = villa.Detalle,
                ImagenUrl = villa.ImagenUrl,
                Ocupantes = villa.Ocupantes,
                Tarifa = villa.Tarifa,
                MetrosCuadrados = villa.MetrosCuadrados,
                Amenidad = villa.Amenidad
            };

            if(villa == null) { return BadRequest(); }

            patchDto.ApplyTo(villaDto, ModelState);

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Villa modelo = new()
            {
                Id = villaDto.Id,
                Nombre = villaDto.Nombre,
                Detalle = villaDto.Detalle,
                ImagenUrl = villaDto.ImagenUrl,
                Ocupantes = villaDto.Ocupantes,
                Tarifa = villaDto.Tarifa,
                MetrosCuadrados = villaDto.MetrosCuadrados,
                Amenidad = villaDto.Amenidad
            };

            _db.Villas.Update(modelo); 
            _db.SaveChanges();

            return NoContent();
        }
    }
}

