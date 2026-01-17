using System;

namespace UserManagement.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public string Role { get; set; }
        public int ProfileVersion { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
