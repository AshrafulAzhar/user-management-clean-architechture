using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.ValueObjects;

namespace UserManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<RegisterUserRequest> _registerValidator;
        private readonly IEmailService _emailService;

        public UserService(
            IUserRepository userRepository, 
            IPasswordHasher passwordHasher, 
            ICurrentUserService currentUserService,
            IValidator<RegisterUserRequest> registerValidator,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _currentUserService = currentUserService;
            _registerValidator = registerValidator;
            _emailService = emailService;
        }

        public async Task<UserDto> RegisterAsync(RegisterUserRequest request)
        {
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null) throw new DomainException("Email already exists.");

            var existingPhone = await _userRepository.GetByPhoneAsync(request.Phone);
            if (existingPhone != null) throw new DomainException("Phone number already exists.");

            if (!string.IsNullOrEmpty(request.Username))
            {
                var existingUsername = await _userRepository.GetByUsernameAsync(request.Username);
                if (existingUsername != null) throw new DomainException("Username already exists.");
            }

            var user = new User(
                request.FullName, request.Email, request.Phone, request.Username,
                _passwordHasher.HashPassword(request.Password), request.DateOfBirth,
                request.TermsVersion, request.PrivacyVersion, request.MarketingConsent,
                request.IpAddress, request.DeviceInfo);

            await _userRepository.AddAsync(user);

            // Send welcome email (Non-blocking to ensure registration succeeds even if email fails)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        user.Email, 
                        "Welcome to Our System", 
                        $"<h1>Welcome {user.FullName}!</h1><p>Your account has been successfully created.</p>");
                }
                catch (Exception ex)
                {
                    // Log the error (In a real app, use a proper logger)
                    Console.WriteLine($"Failed to send welcome email: {ex.Message}");
                }
            });

            return MapToDto(user);
        }

        public async Task<UserDto> GetProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new DomainException("User not found.");

            var currentUserId = _currentUserService.UserId;
            var currentUserRole = _currentUserService.UserRole;
            bool isAdmin = currentUserRole == UserRole.Admin.Name || currentUserRole == UserRole.SuperAdmin.Name;

            if (currentUserId != user.Id && !isAdmin)
                throw new DomainException("Access denied.");

            return MapToDto(user, isAdmin);
        }

        public async Task<UserDto> UpdateProfileAsync(UpdateUserProfileRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null) throw new DomainException("User not found.");

            var currentUserId = _currentUserService.UserId;
            var currentUserRole = _currentUserService.UserRole;
            bool isAdmin = currentUserRole == UserRole.Admin.Name || currentUserRole == UserRole.SuperAdmin.Name;

            if (currentUserId != user.Id && !isAdmin)
                throw new DomainException("Access denied.");

            user.UpdateProfile(request.FullName, request.Version);
            await _userRepository.UpdateAsync(user);
            return MapToDto(user, isAdmin);
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null) throw new DomainException("User not found.");

            if (_currentUserService.UserId != user.Id)
                throw new DomainException("Access denied.");

            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                throw new DomainException("Current password is incorrect.");

            user.ChangePassword(_passwordHasher.HashPassword(request.NewPassword));
            await _userRepository.UpdateAsync(user);
        }

        public async Task UpdateStatusAsync(UpdateUserStatusRequest request)
        {
            bool isAdmin = _currentUserService.UserRole == UserRole.Admin.Name || _currentUserService.UserRole == UserRole.SuperAdmin.Name;
            if (!isAdmin) throw new DomainException("Admin permissions required.");

            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null) throw new DomainException("User not found.");

            if (!request.IsActive && user.Id == _currentUserService.UserId)
                throw new DomainException("You cannot deactivate your own account.");

            if (request.IsActive) user.Activate();
            else user.Deactivate(request.Reason);

            await _userRepository.UpdateAsync(user);
        }

        public async Task AssignRoleAsync(AssignUserRoleRequest request)
        {
            var actorRoleName = _currentUserService.UserRole;
            if (actorRoleName != UserRole.Admin.Name && actorRoleName != UserRole.SuperAdmin.Name)
                throw new DomainException("Admin permissions required.");

            var actorRole = actorRoleName == UserRole.SuperAdmin.Name ? UserRole.SuperAdmin : UserRole.Admin;
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null) throw new DomainException("User not found.");

            UserRole targetRole = request.NewRole switch
            {
                "Admin" => UserRole.Admin,
                "User" => UserRole.User,
                "SuperAdmin" => UserRole.SuperAdmin,
                _ => throw new DomainException("Invalid role.")
            };

            user.ChangeRole(targetRole, actorRole);
            await _userRepository.UpdateAsync(user);
        }

        public async Task<PagedResult<UserDto>> SearchAsync(SearchUsersRequest request)
        {
            bool isAdmin = _currentUserService.UserRole == UserRole.Admin.Name || _currentUserService.UserRole == UserRole.SuperAdmin.Name;
            if (!isAdmin) throw new DomainException("Admin permissions required.");

            if (request.PageSize > 50) request.PageSize = 50;
            if (request.Page < 1) request.Page = 1;

            var (users, total) = await _userRepository.ListAsync(request.Page, request.PageSize, request.SearchTerm, request.Role, request.Status);
            return new PagedResult<UserDto>
            {
                Items = users.Select(u => MapToDto(u, true)),
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        private UserDto MapToDto(User user, bool isAdmin = false)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = isAdmin ? user.Email : MaskEmail(user.Email),
                Phone = isAdmin ? user.Phone : MaskPhone(user.Phone),
                Username = user.Username,
                Status = user.Status.Value,
                Role = user.Role.Name,
                ProfileVersion = user.ProfileVersion,
                CreatedAt = user.CreatedAt
            };
        }

        private string MaskEmail(string email)
        {
            var parts = email.Split('@');
            return $"{parts[0][0]}***@{parts[1]}";
        }

        private string MaskPhone(string phone)
        {
            return $"{phone.Substring(0, 4)}***{phone.Substring(phone.Length - 2)}";
        }
    }
}
