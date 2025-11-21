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
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            // DTO -> Entity (para crear/editar)
            CreateMap<ShiftTypeDTO, ShiftType>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<ShiftTypeCreateDTO, ShiftType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled));

            CreateMap<ShiftTypeUpdateDTO, ShiftType>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            // ShiftSchedule -> ShiftScheduleDTO (ya existía)
            CreateMap<ShiftSchedule, ShiftScheduleDTO>()
                .ForMember(dest => dest.Hour, opt => opt.MapFrom(src => src.Hour))
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => true));

            CreateMap<ShiftScheduleCreateDTO, ShiftSchedule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Hour, opt => opt.MapFrom(src => src.Hour))
                .ForMember(dest => dest.ShiftTypeId, opt => opt.MapFrom(src => src.ShiftTypeId));

            CreateMap<ShiftScheduleUpdateDTO, ShiftSchedule>()
                .ForMember(dest => dest.Hour, opt => opt.MapFrom(src => src.Hour))
                .ForMember(dest => dest.ShiftTypeId, opt => opt.MapFrom(src => src.ShiftTypeId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            // Professional mappings
            CreateMap<Professional, ProfessionalDTO>();

            CreateMap<ProfessionalCreateDTO, Professional>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));

            CreateMap<ProfessionalUpdateDTO, Professional>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled));

            // Holidays mappings
            CreateMap<Holidays, HolidaysDTO>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message));

            CreateMap<HolidaysCreateDTO, Holidays>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message));

            CreateMap<HolidaysUpdateDTO, Holidays>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message));
        }
    }
}