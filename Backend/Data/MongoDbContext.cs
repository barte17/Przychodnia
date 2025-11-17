using MongoDB.Driver;
using Backend.Models.NonRelational;

namespace Backend.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb");
        var mongoUrl = MongoUrl.Create(connectionString);
        var mongoClient = new MongoClient(mongoUrl);
        _database = mongoClient.GetDatabase(mongoUrl.DatabaseName ?? "BazaPrzychodnia");
    }

    public IMongoCollection<Ankieta> Ankiety =>
        _database.GetCollection<Ankieta>("ankiety");

    public async Task InitializeAsync()
    {
        // Indeks na IdPacjenta
        var indexKeysDefinition = Builders<Ankieta>.IndexKeys
            .Ascending(a => a.IdPacjenta);
        var indexModel = new CreateIndexModel<Ankieta>(indexKeysDefinition);
        await Ankiety.Indexes.CreateOneAsync(indexModel);

        // Indeks na PESEL
        var peselIndexKeys = Builders<Ankieta>.IndexKeys
            .Ascending(a => a.PESEL);
        var peselIndexModel = new CreateIndexModel<Ankieta>(peselIndexKeys);
        await Ankiety.Indexes.CreateOneAsync(peselIndexModel);

        // Indeks na typ ankiety
        var typIndexKeys = Builders<Ankieta>.IndexKeys
            .Ascending(a => a.TypAnkiety);
        var typIndexModel = new CreateIndexModel<Ankieta>(typIndexKeys);
        await Ankiety.Indexes.CreateOneAsync(typIndexModel);
    }
}
