using MongoDB.Driver;

public class MongoRepository
{
    private readonly IMongoCollection<PostDescription> _collection;

    public MongoRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<PostDescription>("post_descriptions");
    }

    public async Task InsertAsync(PostDescription doc)
    {
        await _collection.InsertOneAsync(doc);
    }

    public async Task<PostDescription> GetByPostIdAsync(int postId)
    {
        return await _collection
            .Find(x => x.PostId == postId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateDescriptionAsync(string id, string desc)
    {
        var update = Builders<PostDescription>.Update
            .Set(x => x.Description, desc)
            .Set(x => x.Status, "done")
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(
            x => x.Id == id,
            update
        );
    }
}
