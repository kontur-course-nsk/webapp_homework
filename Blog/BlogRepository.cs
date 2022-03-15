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
        private IMongoCollection<Post> collection;

        public BlogRepository(IMongoCollection<Post> collection)
        {
            this.collection = collection;
        }

        public async Task<Post> GetPostAsync(string id, CancellationToken token)
        {
            var result = await collection.FindAsync(post => post.Id == id, cancellationToken: token);

            var post = await result.FirstOrDefaultAsync();

            if (post is null)
                throw new PostNotFoundException(id);

            return post;
        }

        public async Task<PostsList> SearchPostsAsync(PostSearchInfo searchInfo, CancellationToken token)
        {
            if (searchInfo == null)
            {
                throw new ArgumentNullException(nameof(searchInfo));
            }

            var builder = Builders<Post>.Filter;

            var filter = builder.AnyEq(post => post.Tags, searchInfo.Tag);

            if (searchInfo.FromCreatedAt != null)
                filter &= builder.Gte(post => post.CreatedAt, searchInfo.FromCreatedAt);

            if (searchInfo.ToCreatedAt != null)
                filter &= builder.Lt(post => post.CreatedAt, searchInfo.ToCreatedAt);

            var limit = searchInfo.Limit ?? 10;
            var offset = searchInfo.Offset ?? 0;

            var searchResult = collection.Find(filter);
            var count = await searchResult.CountDocumentsAsync(token);
            var posts = await searchResult.Skip(offset).Limit(limit).ToListAsync(token);

            return new PostsList { Posts = posts.ToArray(), Total = count };
        }

        public async Task<Post> CreatePostAsync(PostCreateInfo createInfo, CancellationToken token)
        {
            // Convert date to ISO-8601 date.
            var time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            var timeUtc = DateTime.Parse(time).ToUniversalTime();

            var post = new Post
            {
                Id = Guid.NewGuid().ToString(),
                Title = createInfo.Title,
                Text = createInfo.Text,
                Tags = createInfo.Tags,
                CreatedAt = createInfo.CreatedAt is null ?
                    timeUtc : createInfo.CreatedAt.Value.ToUniversalTime(),
            };
            await collection.InsertOneAsync(post, cancellationToken: token);

            return post;
        }

        public async Task UpdatePostAsync(string id, PostUpdateInfo updateInfo, CancellationToken token)
        {
            if(updateInfo is null)
                throw new ArgumentNullException(nameof(updateInfo));

            var post = await GetPostAsync(id, token);

            if (post is null)
                throw new PostNotFoundException(id);

            var update = updateInfo.Title is null ? 
                Builders<Post>.Update.Set(p => p.Title, post.Title) :
                Builders<Post>.Update.Set(p => p.Title, updateInfo.Title);

            if (updateInfo.Text != null)
                update = update.Set(p => p.Text, updateInfo.Text);

            if (updateInfo.Tags != null)
                update = update.Set(p => p.Tags, updateInfo.Tags);

            var updateResult = await collection.UpdateOneAsync(p => p.Id == id, update, cancellationToken: token);
        }

        public async Task DeletePostAsync(string id, CancellationToken token)
        {
            var deleteResult = await collection.DeleteOneAsync(post => post.Id == id, cancellationToken: token);

            if (deleteResult.DeletedCount == 0)
                throw new PostNotFoundException(id);
        }

        public async Task CreateCommentAsync(string postId, CommentCreateInfo createInfo, CancellationToken token)
        {
            if (createInfo == null)
                throw new ArgumentNullException(nameof(createInfo));

            var post = await GetPostAsync(postId, token);

            if (post is null)
                throw new PostNotFoundException(postId);

            var comment = new Comment 
            { 
                Username = createInfo.Username, 
                Text = createInfo.Text, 
                CreatedAt = DateTime.UtcNow 
            };

            var updateDefinition = post.Comments is null ?
                Builders<Post>.Update.Set(p => p.Comments, new Comment[] { comment }) :
                Builders<Post>.Update.Push(p => p.Comments, comment);
            
            await collection.UpdateOneAsync(p => p.Id == postId, updateDefinition, cancellationToken: token);
        }
    }
}
