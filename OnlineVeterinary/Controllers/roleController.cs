using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace OnlineVeterinary.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class roleController : ControllerBase
    {
        private RoleManager<IdentityRole> _rolemanager;

        public roleController(RoleManager<IdentityRole> rolemanager)
        {
            _rolemanager = rolemanager;
        }
        [HttpPost]
        public async Task<IActionResult> createRole( RoleEnum role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("enter valid role ");
            }
            var CheckRoleExist = await _rolemanager.FindByNameAsync(role.ToString()) != null;
            if (CheckRoleExist)
            {
                return BadRequest("This role already exist");
            }
            await _rolemanager.CreateAsync(new IdentityRole(role.ToString()));
            return Ok(_rolemanager.Roles.ToList());
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRole( RoleEnum role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("enter valid role ");
            }
            var CheckRoleExist = await _rolemanager.FindByNameAsync(role.ToString()) ;
            if (CheckRoleExist == null)
            {
                return BadRequest("This role is not there ");
            }
            await _rolemanager.DeleteAsync(CheckRoleExist);
            return Ok(_rolemanager.Roles.ToList());
        }
    }
}