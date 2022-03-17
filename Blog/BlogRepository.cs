using System;
using System.Threading;
using System.Threading.Tasks;
using Blog.Exceptions;
using Blog.Models;
using MongoDB.Driver;

namespace Blog
{
    public sealed class BlogRepository : IBlogRepository
    {
        private readonly IMongoCollection<Post> postsCollection;

        public BlogRepository()
        {
            var connectionString = "mongodb://localhost:27017/blog";
            var connection = new MongoUrlBuilder(connectionString);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(connection.DatabaseName);
            postsCollection = database.GetCollection<Post>("posts");
        }

        public IMongoCollection<Post> Posts => postsCollection;

        public async Task<Post> GetPostAsync(string id, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException();

            var post = await postsCollection
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync(token);

            if (post == null)
                throw new PostNotFoundException(id);

            return post;
        }

        public async Task<PostsList> SearchPostsAsync(PostSearchInfo searchInfo, CancellationToken token)
        {
            if (searchInfo == null)
                throw new ArgumentNullException();

            var builder = Builders<Post>.Filter;
            var filter = FilterDefinition<Post>.Empty;

            if (searchInfo.Tag != null)
            {
                filter &= builder.AnyEq(p => p.Tags, searchInfo.Tag);
            }

            if (searchInfo.FromCreatedAt != null)
            {
                filter &= builder.Gte(p => p.CreatedAt, searchInfo.FromCreatedAt);
            }

            if (searchInfo.ToCreatedAt != null)
            {
                filter &= builder.Lt(p => p.CreatedAt, searchInfo.ToCreatedAt);
            }

            var limit = searchInfo.Limit ?? 10;
            var offset = searchInfo.Offset ?? 0;

            var result = postsCollection.Find(filter);
            var total = await result.CountDocumentsAsync(token);

            var posts = await result
                .Skip(offset)
                .Limit(limit)
                .ToListAsync(token);

            return new PostsList {Posts = posts.ToArray(), Total = total};
        }

        public async Task<Post> CreatePostAsync(PostCreateInfo createInfo, CancellationToken token)
        {
            if (createInfo == null)
                throw new ArgumentNullException();

            var post = new Post
            {
                Id = Guid.NewGuid().ToString(),
                Title = createInfo.Title,
                Text = createInfo.Text,
                Tags = createInfo.Tags,
                CreatedAt = createInfo.CreatedAt ?? DateTime.UtcNow
            };

            await postsCollection.InsertOneAsync(post, new InsertOneOptions(), token);
            return await GetPostAsync(post.Id, token);
        }

        public async Task UpdatePostAsync(string id, PostUpdateInfo updateInfo, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(id) || updateInfo == null)
                throw new ArgumentNullException();

            var post = await postsCollection
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync(cancellationToken: token);

            if (post == null)
                throw new PostNotFoundException(id);

            var filter = Builders<Post>.Filter.Eq(p => p.Id, post.Id);
            var updateBuilder = Builders<Post>.Update;

            if (updateInfo.Title != null)
            {
                var update = updateBuilder.Set(p => p.Title, updateInfo.Title);
                await postsCollection.UpdateOneAsync(filter, update, cancellationToken: token);
            }

            if (updateInfo.Text != null)
            {
                var update = updateBuilder.Set(p => p.Text, updateInfo.Text);
                await postsCollection.UpdateOneAsync(filter, update, cancellationToken: token);
            }

            if (updateInfo.Tags != null)
            {
                var update = updateBuilder.Set(p => p.Tags, updateInfo.Tags);
                await postsCollection.UpdateOneAsync(filter, update, cancellationToken: token);
            }
        }

        public async Task DeletePostAsync(string id, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException();

            var post = await GetPostAsync(id, token);

            if (post == null)
                throw new PostNotFoundException(id);

            await postsCollection.DeleteOneAsync(p => p.Id == id, token);
        }

        public async Task CreateCommentAsync(string postId, CommentCreateInfo createInfo, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(postId) || createInfo == null)
                throw new ArgumentNullException();

            var post = await GetPostAsync(postId, token);

            if (post == null)
                throw new PostNotFoundException(postId);

            var comment = new Comment
            {
                Username = createInfo.Username,
                Text = createInfo.Text,
                CreatedAt = DateTime.UtcNow
            };

            var filter = Builders<Post>.Filter.Eq(p => p.Id, post.Id);
            var updateBuilder = Builders<Post>.Update;

            var update = post.Comments == null
                ? updateBuilder.Set(p => p.Comments, new[] {comment})
                : updateBuilder.Push(p => p.Comments, comment);

            await postsCollection.UpdateOneAsync(filter, update, cancellationToken: token);
        }
    }
}