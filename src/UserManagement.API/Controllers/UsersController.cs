using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Interfaces;
using UserManagement.Application.DTOs;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            var result = await _userService.RegisterAsync(request);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(Guid id)
        {
            var result = await _userService.GetProfileAsync(id);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(Guid id, UpdateUserProfileRequest request)
        {
            if (id != request.UserId) return BadRequest("ID mismatch.");
            var result = await _userService.UpdateProfileAsync(request);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(Guid id, ChangePasswordRequest request)
        {
            if (id != request.UserId) return BadRequest("ID mismatch.");
            await _userService.ChangePasswordAsync(request);
            return NoContent();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateUserStatusRequest request)
        {
            if (id != request.UserId) return BadRequest("ID mismatch.");
            await _userService.UpdateStatusAsync(request);
            return NoContent();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("{id}/role")]
        public async Task<IActionResult> AssignRole(Guid id, AssignUserRoleRequest request)
        {
            if (id != request.UserId) return BadRequest("ID mismatch.");
            await _userService.AssignRoleAsync(request);
            return NoContent();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchUsersRequest request)
        {
            var result = await _userService.SearchAsync(request);
            return Ok(result);
        }
    }
}
