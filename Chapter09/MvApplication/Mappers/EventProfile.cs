using AutoMapper;
using MvApplication.DTOs;
using MvDomain.Entities;

namespace MvApplication.Mappers;

public class EventProfile : Profile {
  public EventProfile() {
    CreateMap<Event, EventDto>();
    CreateMap<TicketOrder, TicketOrderDto>();
  }
}
