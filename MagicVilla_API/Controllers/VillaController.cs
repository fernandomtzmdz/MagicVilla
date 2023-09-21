using AutoMapper;
using MagicVilla_API.Datos;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.DTO;
using MagicVilla_API.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Net;

namespace MagicVilla_API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {

        private readonly ILogger<VillaController> _logger; //aplicando logger para tratamiento de errores o información        
        //private readonly ApplicationDbContext _db; //inyectamos el DbContext para acceder a los datos de la db        
        private readonly IVillaRepositorio _villaRepo; //utilizamos esto para no usar directamente el dbContext
        private readonly IMapper _mapper; //creamos un objeto para utilizar el automapper
        protected APIResponse _response;

        //inyectar el dbContext dentro del controlador
        public VillaController(ILogger<VillaController> logger, IVillaRepositorio villaRepo, IMapper mapper)
        {
            _logger = logger;                        
            //_db = db; //variable de nuestro DbContext para comunicarnos con la base de datos
            _villaRepo = villaRepo;
            _mapper = mapper;
            _response = new();
        }

        

        //ENDPOINT que obtiene listado completo de villas
        //El ActionResult indica un código de estado, si el código de respuesta es 200 indica que fue exitoso
        [HttpGet]
        //documentamos los endpoint en caso de error o éxito
        [ProducesResponseType(StatusCodes.Status200OK)]
        //con async Task<método> indicamos que se trata de una llamada asíncrona
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            try
            {
                _logger.LogInformation("Obtener las villas");

                //IEnumerable<Villa> villaList = await _db.Villas.ToListAsync();  //utilizando dbContext
                IEnumerable<Villa> villaList = await _villaRepo.ObtenerTodos(); //implementando la interfaz

                _response.Resultado = _mapper.Map<IEnumerable<VillaDto>>(villaList);
                _response.statusCode = HttpStatusCode.OK;

                //de esta manera hacemos uso de DbContext para extraer los datos directamente de nuestra BD
                //return Ok(_mapper.Map<IEnumerable<VillaDto>>(villaList));

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() }; 
            }

            return _response;

        }

        //Nuevo ENDPOINT que realiza búsqueda individual de villas
        //El ActionResult indica un código de estado, si el código de respuesta es 200 indica que fue
        //exitoso, con el ActionResult podemos realizar diversas validaciones como las siguientes
        [HttpGet("id:int", Name = "GetNewVilla")]
        //documentar los estatus e código enviados a través del ActionResult
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //con async Task<método> indicamos que se trata de una llamada asíncrona
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {

            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer villa con Id " + id);
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    return BadRequest(_response);
                }

                //de esta forma traemos la información de la lista de VillaStore
                //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);

                //de esta forma accedemos a la información de la base de datos mediante el DbContext
                //var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);
                var villa = await _villaRepo.Obtener(v => v.Id == id);

                if (villa == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.IsExitoso=false;
                    return NotFound(_response);
                }

                //haciendo uso de automapper
                _response.Resultado = _mapper.Map<VillaDto>(villa);
                _response.statusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {

                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            
            return _response;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //con async Task<método> indicamos que se trata de una llamada asíncrona
        public async Task<ActionResult<APIResponse>> CrearVilla([FromBody] VillaCreateDto creaVillaDto)
        {
            try
            {
                //validamos el modelo antes de guardar datos
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                //validación tomando la información directo de la BD mediante nuestro dbcontext
                //if (await _db.Villas.FirstOrDefaultAsync(v => v.Nombre.ToLower() == creaVillaDto.Nombre.ToLower()) != null)
                if (await _villaRepo.Obtener(v => v.Nombre.ToLower() == creaVillaDto.Nombre.ToLower()) != null)
                {
                    ModelState.AddModelError("NombreExiste", "Ya existe una villa con ese nombre.");
                    return BadRequest(ModelState);
                }

                if (creaVillaDto == null)
                {
                    return BadRequest(creaVillaDto);
                }

                //con esta línea de códigos evitamos realizar el mapeo de abajo
                Villa modelo = _mapper.Map<Villa>(creaVillaDto);

                //creamos el modelo para insertar datos a la base
                /*Villa modelo = new()
                {
                    Nombre = creaVillaDto.Nombre,
                    Detalle = creaVillaDto.Detalle,
                    ImagenUrl = creaVillaDto.ImagenUrl,
                    Ocupantes = creaVillaDto.Ocupantes,
                    Tarifa = creaVillaDto.Tarifa,
                    MetrosCuadrados = creaVillaDto.MetrosCuadrados,
                    Amenidad = creaVillaDto.Amenidad
                };*/

                //con estas líneas hacemos el insert a la base de datos utilizando dbContext
                //await _db.Villas.AddAsync(modelo);
                //await _db.SaveChangesAsync();

                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;
                await _villaRepo.Crear(modelo);
                _response.Resultado = modelo;
                _response.statusCode = HttpStatusCode.Created;

                //para crear el registro llamamos al http GetNewVilla y enviamos los parámetros necesarios que se indican 
                return CreatedAtRoute("GetNewVilla", new { id = modelo.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsExitoso = false;
                    _response.statusCode=HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var villa = await _villaRepo.Obtener(v => v.Id == id);

                if (villa == null)
                {
                    _response.IsExitoso=false;
                    _response.statusCode=HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                //eliminamos la villa por medio del dbContext
                //_db.Villas.Remove(villa);
                //await _db.SaveChangesAsync();

                await _villaRepo.Remover(villa);

                _response.statusCode = HttpStatusCode.NoContent;

                //siempre que trabajamos con delete se regresa esto
                //return NoContent();
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return BadRequest(_response);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDto villaUpdateDto) 
        { 
            if(villaUpdateDto == null || id != villaUpdateDto.Id)
            {
                _response.IsExitoso = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response); 
            }

            //con esta línea evitamos mapear el modelo manualmente como en las líneas comentadas /**/
            Villa modelo = _mapper.Map<Villa>(villaUpdateDto);

            /*Villa modelo = new()
            {
                Id = villaUpdateDto.Id,
                Nombre = villaUpdateDto.Nombre,
                Detalle = villaUpdateDto.Detalle,
                ImagenUrl = villaUpdateDto.ImagenUrl,
                Ocupantes = villaUpdateDto.Ocupantes,
                Tarifa = villaUpdateDto.Tarifa,
                MetrosCuadrados = villaUpdateDto.MetrosCuadrados,
                Amenidad = villaUpdateDto.Amenidad
            };*/

            //utilizando dbContext
            //_db.Villas.Update(modelo);
            //await _db.SaveChangesAsync();

            await _villaRepo.Actualizar(modelo);
            
            _response.statusCode = HttpStatusCode.NoContent;

            return Ok(_response);
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDto> patchDto)
        {
            if (patchDto == null || id == 0) 
            {
                _response.IsExitoso = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response); 
            }

            //con dbContext
            //var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);

            var villa = await _villaRepo.Obtener(v => v.Id == id, tracked:false);

            //con esta línea de código evitamos mapear manualmente los campos como hacemos en las líneas comentadas /**/
            VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);

            /*VillaUpdateDto villaDto = new()
            {
                Id = villa.Id,
                Nombre = villa.Nombre,
                Detalle = villa.Detalle,
                ImagenUrl = villa.ImagenUrl,
                Ocupantes = villa.Ocupantes,
                Tarifa = villa.Tarifa,
                MetrosCuadrados = villa.MetrosCuadrados,
                Amenidad = villa.Amenidad
            };*/

            if(villa == null) { return BadRequest(); }

            patchDto.ApplyTo(villaDto, ModelState);

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //con esta línea de código evitamos mapear manualmente los campos como hacemos en las líneas comentadas /**/
            Villa modelo = _mapper.Map<Villa>(villaDto);

            /*Villa modelo = new()
            {
                Id = villaDto.Id,
                Nombre = villaDto.Nombre,
                Detalle = villaDto.Detalle,
                ImagenUrl = villaDto.ImagenUrl,
                Ocupantes = villaDto.Ocupantes,
                Tarifa = villaDto.Tarifa,
                MetrosCuadrados = villaDto.MetrosCuadrados,
                Amenidad = villaDto.Amenidad
            };*/

            await _villaRepo.Actualizar(modelo);
            
            _response.statusCode = HttpStatusCode.NoContent;

            return Ok(_response);
        }
    }
}

