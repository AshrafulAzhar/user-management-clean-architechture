using System.Collections.Generic;
using UserManagement.Domain.Common;

namespace UserManagement.Domain.ValueObjects
{
    public class UserRole : ValueObject
    {
        public string Name { get; }
        public int Level { get; }

        public static UserRole User => new UserRole("User", 1);
        public static UserRole Admin => new UserRole("Admin", 10);
        public static UserRole SuperAdmin => new UserRole("SuperAdmin", 100);

        private UserRole(string name, int level)
        {
            Name = name;
            Level = level;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Level;
        }

        public override string ToString() => Name;

        public bool IsHigherThan(UserRole other) => this.Level > other.Level;
        public bool IsSameOrHigherThan(UserRole other) => this.Level >= other.Level;
    }
}
