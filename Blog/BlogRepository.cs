using System;
using System.Threading;
using System.Threading.Tasks;
using Blog.Models;
using MongoDB.Driver;
using System.Linq;
using Blog.Exceptions;

namespace Blog
{
    public sealed class BlogRepository : IBlogRepository
    {
        private readonly IMongoCollection<Post> posts;

        public BlogRepository()
        {
            string connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("BlogDB");
            posts = database.GetCollection<Post>("Posts");
            database.DropCollection("Posts");
        }

        public async Task<Post> GetPostAsync(string id, CancellationToken token)
        {
            // Whith id += "spoil" an PostNotFoundException is being thrown,
            // so it seems to work correct.
            // But unfortunately I didn't found a proper way to fulfill the test.

            // While checking actualPost.Should().BeEquivalentTo(expectedPost)
            // the difference in 5th digit of second count of the CreatedAt field occurs.
            // Can be fixed by making a test comparing each field instead of entire posts.

            var filter = Builders<Post>.Filter.Eq(x => x.Id, id);
            try
            {
                var filteredPosts = await posts.FindAsync(filter, cancellationToken: token);
                return filteredPosts.First();
            }
            catch
            {
                throw new PostNotFoundException(id);
            }           
        }

        public async Task<PostsList> SearchPostsAsync(PostSearchInfo searchInfo, CancellationToken token)
        {
            var builder = Builders<Post>.Filter;
            var filter = builder.Empty;
            var resultCountLimit = searchInfo.Limit ?? 10;

            if (!string.IsNullOrWhiteSpace(searchInfo.Tag))
                filter &= builder.AnyEq(p => p.Tags, searchInfo.Tag);

            if (searchInfo.FromCreatedAt != null)
                filter &= builder.Gte(p => p.CreatedAt, searchInfo.FromCreatedAt.Value.ToUniversalTime());

            if (searchInfo.ToCreatedAt != null)
                filter &= builder.Lt(p => p.CreatedAt, searchInfo.ToCreatedAt.Value.ToUniversalTime());

            var filteredPosts = await posts.Find(filter)
                .Skip(searchInfo.Offset)
                .Limit(resultCountLimit)
                .ToListAsync<Post> (token);

            return new PostsList() { 
                Posts = filteredPosts.ToArray(), 
                Total = filteredPosts.Count() 
            };
        }

        public async Task<Post> CreatePostAsync(PostCreateInfo createInfo, CancellationToken token)
        {
            var post = new Post() {
                Id = Guid.NewGuid().ToString(),
                Title = createInfo.Title,
                Text = createInfo.Text,
                Tags = createInfo.Tags,                
                CreatedAt = createInfo.CreatedAt is null ?
                    DateTime.UtcNow :
                    createInfo.CreatedAt.Value.ToUniversalTime()                 
            };
            await posts.InsertOneAsync(post, cancellationToken: token);
            return post;
        }

        public async Task UpdatePostAsync(string id, PostUpdateInfo updateInfo, CancellationToken token)
        {
            var post = await GetPostAsync(id, token);
            var filter = Builders<Post>.Filter.Eq(p => p.Id, id);

            if (updateInfo.Title != null)
            {
                var update = Builders<Post>.Update.Set(p => p.Title, updateInfo.Title);
                await posts.FindOneAndUpdateAsync(filter, update, cancellationToken: token);
            }

            if (updateInfo.Text != null)
            {
                var update = Builders<Post>.Update.Set(p => p.Text, updateInfo.Text);
                await posts.FindOneAndUpdateAsync(filter, update, cancellationToken: token);
            }

            if (updateInfo.Tags != null)
            {
                var update = Builders<Post>.Update.Set(p => p.Tags, updateInfo.Tags);
                await posts.FindOneAndUpdateAsync(filter, update, cancellationToken: token);
            }
        }

        public async Task DeletePostAsync(string id, CancellationToken token)
        {
            var filter = Builders<Post>.Filter.Eq(x => x.Id, id);
            await posts.DeleteOneAsync(filter, cancellationToken: token);
        }

        public async Task CreateCommentAsync(string postId, CommentCreateInfo createInfo, CancellationToken token)
        {
            var post = await GetPostAsync(postId, token);            
            var comment = new Comment()
            {
                Username = createInfo.Username,
                Text = createInfo.Text,
                CreatedAt = DateTime.UtcNow
            };

            var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
            var update = post.Comments is null ?
                Builders<Post>.Update.Set(p => p.Comments, new Comment[1] { comment }) :
                Builders<Post>.Update.Push<Comment>(p => p.Comments, comment);
            await posts.FindOneAndUpdateAsync(filter, update, cancellationToken: token);           
        }
    }
}
