using System;
using AutoMapper;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Negocio.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings (existentes)
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : null))
                .ForMember(dest => dest.DateOfCreation, opt => opt.MapFrom(src => src.DateOfCreation));

            CreateMap<UserCreateDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DateOfCreation, opt => opt.MapFrom(_ => DateTime.UtcNow))
                // src.Enabled es bool (no nullable) -> mapear directamente
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled));

            CreateMap<UserUpdateDTO, User>()
                .ForMember(dest => dest.DateOfCreation, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt =>
                {
                    opt.Condition(src => !string.IsNullOrWhiteSpace(src.Password));
                    opt.MapFrom(src => src.Password);
                })
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            // ShiftType mappings
            CreateMap<ShiftType, ShiftTypeDTO>()
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Name));

            // (Si necesitas mapear DTO -> Entity en el futuro)
            CreateMap<ShiftTypeDTO, ShiftType>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            // ShiftSchedule -> ShiftScheduleDTO (Disponible se calcula en servicio)
            CreateMap<ShiftSchedule, ShiftScheduleDTO>()
                .ForMember(dest => dest.Hora, opt => opt.MapFrom(src => src.Hour))
                .ForMember(dest => dest.Disponible, opt => opt.MapFrom(src => true)); // por defecto true
        }
    }
}