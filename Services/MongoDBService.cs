using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.Models;
// using Microsoft.Extensions.Configuration;

namespace LibraryApi.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("MongoDB:ConnectionString").Value;
            var databaseName = configuration.GetSection("MongoDB:DatabaseName").Value;

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public async Task<IActionResult> SubmitMultipleTransaction(Func<Task<IActionResult>> operation)
        {
            var session = await _database.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var res = await operation();

                await session.CommitTransactionAsync();
                return res;
            }
            catch (MongoException)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        // Collections
        public IMongoCollection<UserModel> Users => _database.GetCollection<UserModel>("users");
        public IMongoCollection<RefreshTokenModel> RefreshTokens => _database.GetCollection<RefreshTokenModel>("refresh_tokens");
        public IMongoCollection<BookLoanModel> BookLoans => _database.GetCollection<BookLoanModel>("book_loans");
        public IMongoCollection<BookCopyModel> BookCopies => _database.GetCollection<BookCopyModel>("book_copies");
        public IMongoCollection<BookModel> Books => _database.GetCollection<BookModel>("books");
        public IMongoCollection<RoomModel> StudyRooms => _database.GetCollection<RoomModel>("study_rooms");
        public IMongoCollection<RoomReservationModel> Reservations =>
          _database.GetCollection<RoomReservationModel>("room reservations");
        public IMongoCollection<EmployeeModel> Employees => _database.GetCollection<EmployeeModel>("employees");

        public async Task UsernameUniqueIndex()
        {
            var indexKeys = Builders<UserModel>.IndexKeys.Ascending(x => x.Username);
            var options = new CreateIndexOptions { Unique = true };
            await Users.Indexes.CreateOneAsync(
                new CreateIndexModel<UserModel>(indexKeys, options)
                );
        }

        public async Task UserEmailUniqueIndex()
        {
            var indexKeys = Builders<UserModel>.IndexKeys.Ascending(x => x.Email);
            var options = new CreateIndexOptions { Unique = true };
            await Users.Indexes.CreateOneAsync(
                new CreateIndexModel<UserModel>(indexKeys, options)
                );
        }
    }
}
