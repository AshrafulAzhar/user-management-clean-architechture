using System;
using System.Collections.Generic;

namespace UserManagement.Application.DTOs
{
    public class RegisterUserRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string TermsVersion { get; set; }
        public string PrivacyVersion { get; set; }
        public bool MarketingConsent { get; set; }
        public string IpAddress { get; set; }
        public string DeviceInfo { get; set; }
    }

    public class UpdateUserProfileRequest
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public int Version { get; set; }
    }

    public class ChangePasswordRequest
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class UpdateUserStatusRequest
    {
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
        public string Reason { get; set; }
    }

    public class AssignUserRoleRequest
    {
        public Guid UserId { get; set; }
        public string NewRole { get; set; }
    }

    public class SearchUsersRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public long Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
