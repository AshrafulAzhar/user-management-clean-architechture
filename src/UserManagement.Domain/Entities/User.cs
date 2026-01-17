using System;
using UserManagement.Domain.Common;
using UserManagement.Domain.ValueObjects;
using UserManagement.Domain.Exceptions;

namespace UserManagement.Domain.Entities
{
    public class User : Entity
    {
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public DateTime DateOfBirth { get; private set; }
        public UserStatus Status { get; private set; }
        public UserRole Role { get; private set; }
        
        public bool TermsAccepted { get; private set; }
        public string TermsVersion { get; private set; }
        public bool PrivacyPolicyAccepted { get; private set; }
        public string PrivacyPolicyVersion { get; private set; }
        public bool MarketingConsent { get; private set; }

        // Audit Info
        public string RegistrationIp { get; private set; }
        public string RegistrationDevice { get; private set; }
        public string DeactivationReason { get; private set; }

        public int ProfileVersion { get; private set; } = 1;

        private User() { } // For deserialization

        public User(
            string fullName, 
            string email, 
            string phone, 
            string username, 
            string passwordHash, 
            DateTime dateOfBirth,
            string termsVersion,
            string privacyVersion,
            bool marketingConsent,
            string registrationIp,
            string registrationDevice)
        {
            ValidateRegistration(fullName, dateOfBirth);

            FullName = fullName;
            Email = email.ToLower().Trim();
            Phone = phone; // Should be normalized before calling constructor
            Username = username;
            PasswordHash = passwordHash;
            DateOfBirth = dateOfBirth;
            Status = UserStatus.PendingVerification; // Story: Default state
            Role = UserRole.User;
            
            TermsAccepted = true;
            TermsVersion = termsVersion;
            PrivacyPolicyAccepted = true;
            PrivacyPolicyVersion = privacyVersion;
            MarketingConsent = marketingConsent;

            RegistrationIp = registrationIp;
            RegistrationDevice = registrationDevice;
            
            CreatedAt = DateTime.UtcNow;
        }

        private void ValidateRegistration(string fullName, DateTime dob)
        {
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 2 || fullName.Length > 80)
                throw new DomainException("Full name length must be between 2 and 80 characters.");

            var age = DateTime.Today.Year - dob.Year;
            if (dob.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < 18) throw new DomainException("User must be at least 18 years old.");
        }

        public void UpdateProfile(string fullName, int currentVersion)
        {
            if (currentVersion != ProfileVersion)
                throw new DomainException("Concurrency conflict: The profile has been updated by another process.");

            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 2 || fullName.Length > 80)
                throw new DomainException("Invalid full name.");

            FullName = fullName;
            ProfileVersion++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (Status == UserStatus.Deactivated)
                throw new DomainException("Cannot change password for a deactivated user.");

            PasswordHash = newPasswordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("A reason is required for deactivation.");

            Status = UserStatus.Deactivated;
            DeactivationReason = reason;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            Status = UserStatus.Active;
            DeactivationReason = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeRole(UserRole newRole, UserRole actorRole)
        {
            if (!actorRole.IsHigherThan(newRole) && actorRole != UserRole.SuperAdmin)
                throw new DomainException("Insufficient permissions to assign this role.");
            
            if (Status == UserStatus.Deactivated)
                throw new DomainException("Cannot change role for a deactivated user.");

            Role = newRole;
            UpdatedAt = DateTime.UtcNow;
        }

        public void VerifyEmail()
        {
            if (Status == UserStatus.PendingVerification)
            {
                Status = UserStatus.Active;
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
