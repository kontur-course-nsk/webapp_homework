using System;
using System.Threading;
using System.Threading.Tasks;
using Blog.Models;
using Blog.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Collections.Generic;

namespace Blog
{
    public sealed class BlogRepository : IBlogRepository
    {
        private IMongoCollection<Post> posts;

        public BlogRepository()
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);
            IMongoDatabase database = client.GetDatabase("blog");
            database.DropCollection("posts");
            database.CreateCollection("posts");
            posts = database.GetCollection<Post>("posts");
        }

        public Task<Post> GetPostAsync(string id, CancellationToken token)
        {
            var post = posts.Find(post => post.Id == id).FirstOrDefault(token);

            if (post == null)
                throw new PostNotFoundException(id);

            return Task.FromResult(post);
        }

        public async Task<PostsList> SearchPostsAsync(PostSearchInfo searchInfo, CancellationToken token)
        {
            if (searchInfo == null)
            {
                throw new ArgumentNullException(nameof(searchInfo));
            }

            var builder = Builders<Post>.Filter;

            var filter = builder.All(post => post.Tags, new[] { searchInfo.Tag });

            if (searchInfo.FromCreatedAt != null)
                filter &= builder.Gte(post => post.CreatedAt, searchInfo.FromCreatedAt);

            if (searchInfo.ToCreatedAt != null)
                filter &= builder.Lt(post => post.CreatedAt, searchInfo.ToCreatedAt);

            var limit = searchInfo.Limit ?? 10;
            var offset = searchInfo.Offset ?? 0;

            var list = new List<Post>(limit);
            var totalCount = 0;

            using (var foundPosts = await posts.FindAsync(filter, null, token))
            {
                while (await foundPosts.MoveNextAsync())
                {
                    var people = foundPosts.Current;
                    foreach (var doc in people)
                    {
                        if (totalCount >= offset && totalCount <= offset + limit - 1)
                        {
                            lock (list)
                            {
                                list.Add(doc);
                            }
                        }
                        totalCount++;
                    }
                }
            }
            return new PostsList { Posts = list.ToArray(), Total = totalCount };
        }
        public Task<Post> CreatePostAsync(PostCreateInfo createInfo, CancellationToken token)
        {
            var dateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            var dateTimeUtc = DateTime.Parse(dateTime).ToUniversalTime();

            var post = new Post
            {
                Id = Guid.NewGuid().ToString(),
                Title = createInfo.Title,
                Text = createInfo.Text,
                Tags = createInfo.Tags,
                CreatedAt = createInfo.CreatedAt != null 
                    ? createInfo.CreatedAt.Value.ToUniversalTime() : dateTimeUtc,
            };
            posts.InsertOneAsync(post, null, token);

            return Task.FromResult(post);
        }

        public Task UpdatePostAsync(string id, PostUpdateInfo updateInfo, CancellationToken token)
        {
            if(updateInfo == null)
                throw new ArgumentNullException(nameof(updateInfo));

            var update = Builders<Post>.Update.Set(post => post.Id, id);

            if (updateInfo.Title != null)
                update = update.Set(post => post.Title, updateInfo.Title);

            if (updateInfo.Text != null)
                update = update.Set(post => post.Text, updateInfo.Text);

            if (updateInfo.Tags != null)
                update = update.Set(post => post.Tags, updateInfo.Tags);

            var updateResult = posts.UpdateOneAsync(post => post.Id == id, update, null, token).Result;

            if (updateResult.MatchedCount == 0)
                throw new PostNotFoundException(id);

            return Task.CompletedTask;
        }

        public Task DeletePostAsync(string id, CancellationToken token)
        {
            var post = posts.FindOneAndDeleteAsync(post => post.Id == id, null, token);

            if (post.Result == null)
                throw new PostNotFoundException(id);

            return Task.CompletedTask;
        }

        public Task CreateCommentAsync(string postId, CommentCreateInfo createInfo, CancellationToken token)
        {
            if (createInfo == null)
                throw new ArgumentNullException(nameof(createInfo));

            var comment = new Comment { Username = createInfo.Username, Text = createInfo.Text, CreatedAt = DateTime.UtcNow };

            var pushDefinition = Builders<Post>.Update.PushEach(post => post.Comments, new Comment[] { comment }); 
            
            var updateResult = posts.FindOneAndUpdateAsync(post => post.Id == postId && post.Comments != null, pushDefinition, null, token).Result;

            if (updateResult == null)
            {
                var addDefinition = Builders<Post>.Update.Set(post => post.Comments, new Comment[] { comment });
                updateResult = posts.FindOneAndUpdateAsync(post => post.Id == postId, addDefinition, null, token).Result; ;
            }

            if (updateResult == null)
                throw new PostNotFoundException(postId);

            return Task.CompletedTask;
        }
    }
}
