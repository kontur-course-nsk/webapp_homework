using System;
using System.Threading;
using System.Threading.Tasks;
using Blog.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using Blog.Exceptions;

namespace Blog
{
    public sealed class BlogRepository : IBlogRepository
    {
        private readonly IMongoCollection<Post> posts;
        private readonly IMongoCollection<Comment> comments;

        public BlogRepository()
        {
            string connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("BlogDB");
            posts = database.GetCollection<Post>("Posts");            //  {DateTime.UtcNow}
            comments = database.GetCollection<Comment>("Comments");
        }

        public async Task<Post> GetPostAsync(string id, CancellationToken token)
        {
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

        public Task<PostsList> SearchPostsAsync(PostSearchInfo searchInfo, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task<Post> CreatePostAsync(PostCreateInfo createInfo, CancellationToken token)
        {
            var post = new Post() {
                Id = Guid.NewGuid().ToString(),
                Title = createInfo.Title,
                Text = createInfo.Text,
                Tags = createInfo.Tags,
                CreatedAt = createInfo.CreatedAt ?? DateTime.UtcNow
            };
            await posts.InsertOneAsync(post, cancellationToken: token);
            return post;
        }

        public Task UpdatePostAsync(string id, PostUpdateInfo updateInfo, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task DeletePostAsync(string id, CancellationToken token)
        {
            var filter = Builders<Post>.Filter.Eq(x => x.Id, id);
            await posts.DeleteOneAsync(filter, cancellationToken: token);
        }

        public Task CreateCommentAsync(string postId, CommentCreateInfo createInfo, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
