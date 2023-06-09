using ErrorOr;
using MapsterMapper;
using MediatR;
using OnlineVeterinary.Application.Common.Interfaces.Persistence;
using OnlineVeterinary.Application.Features.DTOs;
using OnlineVeterinary.Domain.Pet.Entities;

namespace OnlineVeterinary.Application.Features.Pets.Commands.Add
{
    public class AddPetCommandHandler : IRequestHandler<AddPetCommand, ErrorOr<string>>
    {
        private readonly IPetRepository _petRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        private readonly ICacheService _cacheService;

        public AddPetCommandHandler(
            IPetRepository petRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            ICacheService cacheService)
        {
            _petRepository = petRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _cacheService = cacheService;
        }
        public async Task<ErrorOr<string>> Handle(
            AddPetCommand request,
            CancellationToken cancellationToken)
        {
            Guid id = (request.CareGiverId);

            var user = await _userRepository.GetByIdAsync(id);
            if (user is null )
            {
                return Error.NotFound(description : "you have invalid Id or this user is not exist any more");
            }

           
            var pet = _mapper.Map<Pet>(request);
            _petRepository.Add(pet);
            await _unitOfWork.SaveChangesAsync();
            _cacheService.RemoveData($"{id} pets");
            
            return "pet Added successfully";
        }


    }
}