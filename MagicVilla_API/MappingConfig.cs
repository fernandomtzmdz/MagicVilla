using AutoMapper;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.DTO;

namespace MagicVilla_API
{
    public class MappingConfig : Profile
    {

        //se creó esta nueva clase para utilizar la inyección de dependencias en el controlador para que se realice el automapeo y agilizar los insert y update en el controller
        public MappingConfig()
        {
            //Primer forma de mapear la clase con su clase dto respectivo
            CreateMap<Villa, VillaDto>();
            CreateMap<VillaDto, Villa>();

            //segunda manera de mapear las clases
            CreateMap<Villa, VillaCreateDto>().ReverseMap();
            CreateMap<Villa, VillaUpdateDto>().ReverseMap();


        }
        
    }
}
