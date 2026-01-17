using System;
using System.Linq;
using FluentValidation;
using UserManagement.Application.DTOs;

namespace UserManagement.Application.Validators
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserRequest>
    {
        private static readonly string[] ReservedUsernames = { "admin", "support", "system", "root" };
        private static readonly string[] BlockedEmailDomains = { "mailinator.com", "guerrillamail.com" };

        public RegisterUserValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .Length(2, 80)
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Full name must not contain symbols or digits.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .Must(domain => !BlockedEmailDomains.Any(blocked => domain.EndsWith(blocked, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("This email domain is blocked.");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .Matches(@"^\+[1-9]\d{1,14}$").WithMessage("Phone must be in E.164 format.");

            RuleFor(x => x.Username)
                .Must(u => !ReservedUsernames.Contains(u?.ToLower()))
                .WithMessage("This username is reserved.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(12)
                .Must(HasComplexity).WithMessage("Password must include at least 3 of 4 categories: upper, lower, digit, symbol.")
                .Must((x, pass) => !string.IsNullOrEmpty(x.Email) && !pass.Contains(x.Email.Split('@')[0]))
                .WithMessage("Password cannot contain email parts.");

            RuleFor(x => x.DateOfBirth)
                .Must(dob => dob <= DateTime.Today.AddYears(-18))
                .WithMessage("You must be at least 18 years old.");

            RuleFor(x => x.TermsVersion).NotEmpty();
            RuleFor(x => x.PrivacyVersion).NotEmpty();
        }

        private bool HasComplexity(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            int categories = 0;
            if (password.Any(char.IsLower)) categories++;
            if (password.Any(char.IsUpper)) categories++;
            if (password.Any(char.IsDigit)) categories++;
            if (password.Any(ch => !char.IsLetterOrDigit(ch))) categories++;
            return categories >= 3;
        }
    }
}
