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
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : null));
            CreateMap<UserCreateDTO, User>()
                .ForMember(dest => dest.DateOfCreation, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore());
            CreateMap<UserUpdateDTO, User>()
                .ForMember(dest => dest.DateOfCreation, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore());

            CreateMap<Professional, ProfessionalDTO>();
            CreateMap<ProfessionalCreateDTO, Professional>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Shifts, opt => opt.Ignore());
            CreateMap<ProfessionalUpdateDTO, Professional>()
                .ForMember(dest => dest.Shifts, opt => opt.Ignore());

            CreateMap<ShiftType, ShiftTypeDTO>();
            CreateMap<ShiftTypeCreateDTO, ShiftType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Shifts, opt => opt.Ignore());
            CreateMap<ShiftTypeUpdateDTO, ShiftType>()
                .ForMember(dest => dest.Shifts, opt => opt.Ignore());

            CreateMap<ShiftSchedule, ShiftScheduleDTO>()
                .ForMember(dest => dest.ShiftTypeName, opt => opt.MapFrom(src => src.ShiftType != null ? src.ShiftType.Name : null));
            CreateMap<ShiftScheduleCreateDTO, ShiftSchedule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ShiftType, opt => opt.Ignore())
                .ForMember(dest => dest.Shifts, opt => opt.Ignore());
            CreateMap<ShiftScheduleUpdateDTO, ShiftSchedule>()
                .ForMember(dest => dest.ShiftType, opt => opt.Ignore())
                .ForMember(dest => dest.Shifts, opt => opt.Ignore());

            CreateMap<Shift, ShiftDTO>()
                .ForMember(dest => dest.ShiftTypeName, opt => opt.MapFrom(src => src.ShiftType != null ? src.ShiftType.Name : null))
                .ForMember(dest => dest.ScheduleHour, opt => opt.MapFrom(src => src.Schedule != null ? src.Schedule.Hour : null))
                .ForMember(dest => dest.ProfessionalName, opt => opt.MapFrom(src => 
                    src.Professional != null ? $"{src.Professional.FirstName} {src.Professional.LastName}".Trim() : null));

            CreateMap<ShiftCreateDTO, Shift>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.Date)) 
                .ForMember(dest => dest.ScheduleId, opt => opt.Ignore()) 
                .ForMember(dest => dest.ShiftTypeId, opt => opt.MapFrom(src => src.TipoTurnoId))
                .ForMember(dest => dest.ShiftStateId, opt => opt.Ignore()) 
                .ForMember(dest => dest.ProfessionalId, opt => opt.Ignore()) 
                .ForMember(dest => dest.Professional, opt => opt.Ignore())
                .ForMember(dest => dest.Schedule, opt => opt.Ignore())
                .ForMember(dest => dest.ShiftState, opt => opt.Ignore())
                .ForMember(dest => dest.ShiftType, opt => opt.Ignore());

            CreateMap<ShiftDTO, Shift>()
                .ForMember(dest => dest.Professional, opt => opt.Ignore())
                .ForMember(dest => dest.Schedule, opt => opt.Ignore())
                .ForMember(dest => dest.ShiftState, opt => opt.Ignore())
                .ForMember(dest => dest.ShiftType, opt => opt.Ignore());

            CreateMap<Holidays, HolidaysDTO>();
            CreateMap<HolidaysCreateDTO, Holidays>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<HolidaysUpdateDTO, Holidays>();

            CreateMap<ContactMessage, ContactMessageDTO>();
            CreateMap<ContactMessageCreateDTO, ContactMessage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Read, opt => opt.Ignore())
                .ForMember(dest => dest.Answered, opt => opt.Ignore())
                .ForMember(dest => dest.Reply, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RepliedAt, opt => opt.Ignore());
        }
    }
}