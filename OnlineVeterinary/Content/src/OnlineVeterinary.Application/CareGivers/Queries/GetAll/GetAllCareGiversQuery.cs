using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using OnlineVeterinary.Application.DTOs.CareGiverDTO;

namespace OnlineVeterinary.Application.CareGivers.Queries.GetAll
{
    public record GetAllCareGiversQuery() : IRequest<List<CareGiverDTO>>;
    
}