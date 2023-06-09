using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlineVeterinary.Controllers.Services;
using OnlineVeterinary.Data;
using OnlineVeterinary.Data.Entity;
using OnlineVeterinary.Models;
using OnlineVeterinary.Models.DTOs;
using OnlineVeterinary.Models.Identity;

namespace OnlineVeterinary.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserManager<IdentityUser> _userManagar;
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;



        public AuthController(UserManager<IdentityUser> userManager,
                                RoleManager<IdentityRole> roleManager,
                                IConfiguration config,
                                DataContext context,
                                IMapper mapper)
        {
            _roleManager = roleManager;
            _context = context;
            _mapper = mapper;
            _config = config;
            _userManagar = userManager;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync([FromBody] UserRegisterationDTO userRegister)
        {
            if (!(ModelState.IsValid))
            {
                return BadRequest(AuthResponse.InvalidInput());
            }

            var userSearchResult = await _userManagar.FindByEmailAsync(userRegister.Email);

            if (userSearchResult != null)
            {
                return BadRequest(AuthResponse.EmailAlreadyExist());
            }

            if (await _roleManager.FindByNameAsync(userRegister.UserRole.ToString()) == null)
            {
                return BadRequest(AuthResponse.NoRole());
            }
            var identityUser = new IdentityUser()
            {
                Email = userRegister.Email,
                UserName = userRegister.UserName


            };

            var createUserResult = await _userManagar.CreateAsync(identityUser, userRegister.Password);

            if (createUserResult.Succeeded)
            {
                await _userManagar.AddToRoleAsync(identityUser, userRegister.UserRole.ToString());
                await _context.SaveChangesAsync();
                await AddingToDataBaseAsync(userRegister);
                var token = await GenerateTokenAsync(identityUser);

                return Ok(AuthResponse.Success(token));

            }

            return BadRequest(AuthResponse.SomethingWentWrong());

        }

        private async Task AddingToDataBaseAsync(UserRegisterationDTO userRegister)
        {
            if (userRegister.UserRole == RoleEnum.Doctor)
            {
                await _context.Doctors.AddAsync(new Doctor()
                {
                    UserName = userRegister.UserName,
                    Email = userRegister.Email,
                    IsAvailable = false, //should complete their profile
                    Dislikes = 0,
                    Likes = 0,
                    SuccesfulVisits = 0

                    

                });
            }
            else
            {
                await _context.CareGivers.AddAsync(new CareGiver()
                {
                    Email = userRegister.Email,
                    UserName = userRegister.UserName
                });
            }
            await _context.SaveChangesAsync();
        }

        private async Task<List<Claim>> AddClaimsAsync(IdentityUser user)
        {

            var claims = new List<Claim>(new[]
             {
                new Claim("id", user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            });

            var userClaims = await _userManagar.GetClaimsAsync(user);

            claims.AddRange(userClaims);

            var userRoles = await _userManagar.GetRolesAsync(user);

            //add role claim 
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));

                var targetRole = await _roleManager.FindByNameAsync(role);
                var targetRoleClaims = await _roleManager.GetClaimsAsync(targetRole);
                foreach (var claim in targetRoleClaims)
                {
                    claims.Add(claim);
                }
            }

            return claims;



        }

        private async Task<string> GenerateTokenAsync(IdentityUser user)
        {

            var key = Encoding.UTF8.GetBytes(_config["JwtConfig:Secret"]);
            var claims = await AddClaimsAsync(user);

            var descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512)

            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody] UserLoginDTO userLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(AuthResponse.InvalidInput());
            }
            var userSearchResult = await _userManagar.Users.SingleOrDefaultAsync(a => a.Email == userLogin.EmailOrUserName || a.UserName == userLogin.EmailOrUserName);
            if (userSearchResult == null)
            {
                return BadRequest(AuthResponse.IncorrectPasswordOrEmail());
            }


            var checkPassResult = await _userManagar.CheckPasswordAsync(userSearchResult, userLogin.Password);
            if (checkPassResult)
            {
                var token = await GenerateTokenAsync(userSearchResult);
                return Ok(AuthResponse.Success(token));

            }
            return BadRequest(AuthResponse.SomethingWentWrong());

        }


    }
}