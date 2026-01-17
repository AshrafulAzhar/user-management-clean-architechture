using System;

namespace UserManagement.Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string UserRole { get; }
    }
}
