using ErrorOr;
using MapsterMapper;
using MediatR;
using OnlineVeterinary.Application.Common.Interfaces.Persistence;
using OnlineVeterinary.Application.Common.Interfaces.Services;
using OnlineVeterinary.Application.Features.Auth.Common;
using OnlineVeterinary.Domain.Users.Entities;

namespace OnlineVeterinary.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<AuthResult>>
    {
        private readonly IMapper _mapper;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;


        public RegisterCommandHandler(IUserRepository userRepository,
                                      IMapper mapper,
                                      IJwtGenerator jwtGenerator,
                                      IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtGenerator = jwtGenerator;
            _unitOfWork = unitOfWork;
        }
        public async Task<ErrorOr<AuthResult>> Handle(RegisterCommand request,
                                                      CancellationToken cancellationToken)
        {
            var user = _mapper.Map<User>(request);

            var isExist = await _userRepository.GetByEmailAsync(request.Email);
            if (isExist is not null)
            {
                return Error.Failure(description : "this email is already exist ");
            }

            _userRepository.Add(user);
            await _unitOfWork.SaveChangesAsync();

            var token = _jwtGenerator.GenerateToken(user);
            var mapUser = _mapper.Map<AuthResult>(user);
            var authResult = mapUser with { Token = token };

            return authResult;






        }
    }
}