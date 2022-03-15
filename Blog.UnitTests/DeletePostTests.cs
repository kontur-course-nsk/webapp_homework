using System;
using System.Threading.Tasks;
using Blog.Exceptions;
using Blog.Models;
using FluentAssertions;
using NUnit.Framework;

namespace Blog.UnitTests
{
    [TestFixture]
    internal sealed class DeletePostTests
    {
        private IBlogRepository blogRepository;

        [SetUp]
        public void SetUp()
        {
            var posts = new Connection().Collection;
            this.blogRepository = new BlogRepository(posts);
        }

        [Test]
        public void ThrowPostNotFoundException_WhenGetDeletedPost()
        {
            var post = this.blogRepository.CreatePostAsync(new PostCreateInfo(), default).Result;

            this.blogRepository.DeletePostAsync(post.Id, default).Wait();

            Func<Task> action = async () =>
                await this.blogRepository.GetPostAsync(post.Id, default);
            action.Should().ThrowAsync<PostNotFoundException>();
        }

        [Test]
        public void ThrowPostNotFoundException_WhenPostNotFound()
        {
            Func<Task> action = async () =>
                await this.blogRepository.DeletePostAsync(Guid.NewGuid().ToString(), default);

            action.Should().ThrowAsync<PostNotFoundException>();
        }
    }
}
