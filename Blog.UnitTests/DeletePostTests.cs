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
            this.blogRepository = new BlogRepository();
        }

        [Test]
        public async Task ThrowPostNotFoundException_WhenGetDeletedPost()
        {
            var post = this.blogRepository.CreatePostAsync(new PostCreateInfo(), default).Result;

            this.blogRepository.DeletePostAsync(post.Id, default).Wait();

            Func<Task> action = () => this.blogRepository.GetPostAsync(post.Id, default);

            await action.Should().ThrowAsync<PostNotFoundException>();
        }

        [Test]
        public async Task ThrowPostNotFoundException_WhenPostNotFound()
        {
            Func<Task> action = () => this.blogRepository.DeletePostAsync(Guid.NewGuid().ToString(), default);

            await action.Should().ThrowAsync<PostNotFoundException>();
        }
    }
}
