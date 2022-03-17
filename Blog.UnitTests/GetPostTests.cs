using System;
using System.Threading.Tasks;
using Blog.Exceptions;
using Blog.Models;
using FluentAssertions;
using NUnit.Framework;

namespace Blog.UnitTests
{
    [TestFixture]
    internal sealed class GetPostTests
    {
        private IBlogRepository firstBlogRepository;
        private IBlogRepository secondBlogRepository;

        [SetUp]
        public void SetUp()
        {
            this.firstBlogRepository = new BlogRepository();
            this.secondBlogRepository = new BlogRepository();
        }

        [Test]
        public void GotFromSecondRepository_WhenCreateInFirst()
        {
            var createInfo = new PostCreateInfo
            {
                Title = "Спортивное питание",
                Text = "текст",
                Tags = new[] { "food", "sport" },
            };
            var expected = this.firstBlogRepository.CreatePostAsync(createInfo, default).Result;

            var actual = this.secondBlogRepository.GetPostAsync(expected.Id, default).Result;

            actual.Id.Should().Be(expected.Id);
            actual.Title.Should().Be(expected.Title);
            actual.Text.Should().Be(expected.Text);
            actual.Tags.Should().BeEquivalentTo(expected.Tags);
            actual.CreatedAt.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(expected.CreatedAt);
        }

        [Test]
        public async Task ThrowPostNotFoundException_WhenPostNotFound()
        {
            Func<Task> action = () => this.firstBlogRepository.GetPostAsync(Guid.NewGuid().ToString(), default);

            await action.Should().ThrowAsync<PostNotFoundException>();
        }
    }
}
