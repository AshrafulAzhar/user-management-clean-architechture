using System;
using System.Threading.Tasks;
using UserManagement.Application.DTOs;

namespace UserManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(RegisterUserRequest request);
        Task<UserDto> GetProfileAsync(Guid userId);
        Task<UserDto> UpdateProfileAsync(UpdateUserProfileRequest request);
        Task ChangePasswordAsync(ChangePasswordRequest request);
        Task UpdateStatusAsync(UpdateUserStatusRequest request);
        Task AssignRoleAsync(AssignUserRoleRequest request);
        Task<PagedResult<UserDto>> SearchAsync(SearchUsersRequest request);
    }
}
