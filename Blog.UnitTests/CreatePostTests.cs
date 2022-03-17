using System;
using Blog.Models;
using FluentAssertions;
using NUnit.Framework;

namespace Blog.UnitTests
{
    [TestFixture]
    internal sealed class CreatePostTests
    {
        private IBlogRepository blogRepository;

        [SetUp]
        public void SetUp()
        {
            new BlogRepository().Posts.Database.DropCollection("posts");
            this.blogRepository = new BlogRepository();
        }

        [Test]
        public void GotCorrectPost_WhenCreate()
        {
            var createInfo = new PostCreateInfo
            {
                Title = "Спортивное питание",
                Text = "текст",
                Tags = new[] {"food", "sport"},
            };

            var post = this.blogRepository.CreatePostAsync(createInfo, default).Result;

            Guid.TryParse(post.Id, out _).Should().BeTrue();
            post.Title.Should().Be(createInfo.Title);
            post.Text.Should().Be(createInfo.Text);
            post.Tags.Should().BeEquivalentTo(createInfo.Tags);
            post.Comments.Should().BeNull();
            post.CreatedAt.Should().BeWithin(TimeSpan.FromSeconds(1)).Before(DateTime.UtcNow);
        }
    }
}