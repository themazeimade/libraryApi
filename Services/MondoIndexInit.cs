using MongoDB.Driver;
using Microsoft.Extensions.Hosting;

namespace LibraryApi.Services
{
    public class MongoIndexInitializer : IHostedService
    {
        private readonly MongoDbService _mongo;

        public MongoIndexInitializer(MongoDbService mongo)
        {
            _mongo = mongo;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _mongo.UsernameUniqueIndex();
            await _mongo.UserEmailUniqueIndex();
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
