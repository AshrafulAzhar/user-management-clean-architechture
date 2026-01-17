using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Exceptions;
using Xunit;

namespace UserManagement.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IValidator<RegisterUserRequest>> _registerValidatorMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _registerValidatorMock = new Mock<IValidator<RegisterUserRequest>>();
            _emailServiceMock = new Mock<IEmailService>();

            _service = new UserService(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _currentUserServiceMock.Object,
                _registerValidatorMock.Object,
                _emailServiceMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldRegisterUser_WhenDataIsValid()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "+1234567890",
                Password = "Password123!",
                DateOfBirth = DateTime.Today.AddYears(-20),
                TermsVersion = "v1",
                PrivacyVersion = "v1"
            };

            _registerValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(repo => repo.GetByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            _passwordHasherMock.Setup(hasher => hasher.HashPassword(It.IsAny<string>()))
                .Returns("hashed_password");

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("j***@example.com");
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenEmailExists()
        {
            // Arrange
            var request = new RegisterUserRequest { Email = "exists@example.com" };
            
            _registerValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(new User("Old User", request.Email, "+1111111111", "old", "hash", DateTime.Today.AddYears(-25), "v1", "v1", false, "127.0.0.1", "device"));

            // Act
            Func<Task> act = async () => await _service.RegisterAsync(request);

            // Assert
            await act.Should().ThrowAsync<DomainException>().WithMessage("Email already exists.");
        }
    }
}
