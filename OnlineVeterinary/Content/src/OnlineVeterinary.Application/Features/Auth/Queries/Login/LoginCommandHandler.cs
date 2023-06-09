using ErrorOr;
using MapsterMapper;
using MediatR;
using OnlineVeterinary.Application.Common.Interfaces.Persistence;
using OnlineVeterinary.Application.Common.Interfaces.Services;
using OnlineVeterinary.Application.Features.Auth.Common;

namespace OnlineVeterinary.Application.Features.Auth.Queries.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<AuthResult>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IJwtGenerator _jwtGenerator;

        public LoginCommandHandler(
                                    IUserRepository userRepository,
                                    IMapper mapper,
                                    IJwtGenerator jwtGenerator)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtGenerator = jwtGenerator;
        }
        public async Task<ErrorOr<AuthResult>> Handle(LoginCommand request,
                                                      CancellationToken cancellationToken)
        {


            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || user.Password != request.Password)
            {
                return Error.Failure(Error.Failure().Code,
                                     "email or password incorrect");
            }

            
            var token = _jwtGenerator.GenerateToken(user);
            var mapUser = _mapper.Map<AuthResult>(user);
            var authResult = mapUser with { Token = token };


            return authResult;



        }
    }
}