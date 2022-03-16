using System;
using System.Threading.Tasks;
using Blog.Exceptions;
using Blog.Models;
using FluentAssertions;
using NUnit.Framework;

namespace Blog.UnitTests
{
    [TestFixture]
    internal sealed class CreateCommentTests
    {
        private IBlogRepository blogRepository;

        [SetUp]
        public void SetUp()
        {
            this.blogRepository = new BlogRepository();
        }

        [Test]
        public void GotPostWithComment_WhenCommentCreated()
        {
            var post = this.blogRepository.CreatePostAsync(new PostCreateInfo(), default).Result;
            var commentCreateInfo = new CommentCreateInfo { Username = "user", Text = "Текст комментария" };

            this.blogRepository.CreateCommentAsync(post.Id, commentCreateInfo, default).Wait();

            var updatedPost = this.blogRepository.GetPostAsync(post.Id, default).Result;
            updatedPost.Comments.Should().HaveCount(1);
            updatedPost.Comments[0].Username.Should().Be(commentCreateInfo.Username);
            updatedPost.Comments[0].Text.Should().Be(commentCreateInfo.Text);
            updatedPost.Comments[0].CreatedAt.Should().BeWithin(TimeSpan.FromSeconds(1)).Before(DateTime.UtcNow);
        }

        [Test]
        public async Task ThrowPostNotFoundException_WhenPostNotFound()
        {
            var commentCreateInfo = new CommentCreateInfo();
            Func<Task> action = () =>
                this.blogRepository.CreateCommentAsync(Guid.NewGuid().ToString(), commentCreateInfo, default);

            await action.Should().ThrowAsync<PostNotFoundException>();
        }
    }
}
