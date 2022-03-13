using System;
using System.Threading;
using System.Threading.Tasks;
using Blog.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Blog
{
    public sealed class BlogRepository : IBlogRepository
    {
        public BlogRepository()
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);
            IMongoDatabase database = client.GetDatabase("BlogDB");
        }

        public Task<Post> GetPostAsync(string id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<PostsList> SearchPostsAsync(PostSearchInfo searchInfo, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<Post> CreatePostAsync(PostCreateInfo createInfo, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePostAsync(string id, PostUpdateInfo updateInfo, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DeletePostAsync(string id, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task CreateCommentAsync(string postId, CommentCreateInfo createInfo, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
