using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Persistence;

namespace UserManagement.Infrastructure.Persistence
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly MongoDbContext _context;

        public MongoUserRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.ToLower().Trim();
            return await _context.Users.Find(u => u.Email == normalizedEmail).FirstOrDefaultAsync();
        }

        public async Task<User> GetByPhoneAsync(string phone)
        {
            return await _context.Users.Find(u => u.Phone == phone).FirstOrDefaultAsync();
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.InsertOneAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        public async Task<(IEnumerable<User> Users, long Total)> ListAsync(int page, int pageSize, string searchTerm, string role, string status)
        {
            var builder = Builders<User>.Filter;
            var filter = builder.Empty;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filter &= builder.Regex(u => u.FullName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")) |
                          builder.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
            }

            if (!string.IsNullOrEmpty(role))
            {
                filter &= builder.Eq("Role.Name", role);
            }

            if (!string.IsNullOrEmpty(status))
            {
                filter &= builder.Eq("Status.Value", status);
            }

            var total = await _context.Users.CountDocumentsAsync(filter);
            var users = await _context.Users.Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .SortByDescending(u => u.CreatedAt)
                .ToListAsync();

            return (users, total);
        }
    }
}
