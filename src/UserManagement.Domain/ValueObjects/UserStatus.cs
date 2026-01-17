using System.Collections.Generic;
using UserManagement.Domain.Common;

namespace UserManagement.Domain.ValueObjects
{
    public class UserStatus : ValueObject
    {
        public string Value { get; }

        public static UserStatus PendingVerification => new UserStatus("PendingVerification");
        public static UserStatus Active => new UserStatus("Active");
        public static UserStatus Deactivated => new UserStatus("Deactivated");

        private UserStatus(string value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
