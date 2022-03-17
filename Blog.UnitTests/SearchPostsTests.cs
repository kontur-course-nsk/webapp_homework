using System;
using System.Threading;
using Blog.Models;
using FluentAssertions;
using NUnit.Framework;

namespace Blog.UnitTests
{
    [TestFixture]
    internal sealed class SearchPostsTests
    {
        private IBlogRepository blogRepository;

        [SetUp]
        public void SetUp()
        {
            new BlogRepository().Posts.Database.DropCollection("posts");
            this.blogRepository = new BlogRepository();
        }

        [Test]
        public void GotPostsCreatedAfterDate_WhenFromCreatedAtNotEmpty()
        {
            var post9 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { CreatedAt = new DateTime(2022, 3, 9) }, default).Result;
            var post10 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { CreatedAt = new DateTime(2022, 3, 10) }, default).Result;
            var post20 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { CreatedAt = new DateTime(2022, 3, 20) }, default).Result;

            var postsList = this.blogRepository
                .SearchPostsAsync(new PostSearchInfo { FromCreatedAt = new DateTime(2022, 3, 10) }, default).Result;

            postsList.Posts.Should().BeEquivalentTo(new[] { post10, post20 });
        }

        [Test]
        public void GotPostsCreatedBeforeDate_WhenToCreatedAtNotEmpty()
        {
            var post10 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { CreatedAt = new DateTime(2022, 3, 10) }, default).Result;
            var post19 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { CreatedAt = new DateTime(2022, 3, 19) }, default).Result;
            var post20 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { CreatedAt = new DateTime(2022, 3, 20) }, default).Result;

            var postsList = this.blogRepository
                .SearchPostsAsync(new PostSearchInfo { ToCreatedAt = new DateTime(2022, 3, 20) }, default).Result;

            postsList.Posts.Should().BeEquivalentTo(new[] { post10, post19 });
        }

        [Test]
        public void GotPostsWithTag_WhenTagNotEmpty()
        {
            var postWithoutTags = this.blogRepository
                .CreatePostAsync(new PostCreateInfo(), default).Result;
            var post1 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { Tags = new[] { "tag1" } }, default).Result;
            var post2 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { Tags = new[] { "tag2" } }, default).Result;
            var post12 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { Tags = new[] { "tag1", "tag2" } }, default).Result;

            var postsList = this.blogRepository
                .SearchPostsAsync(new PostSearchInfo { Tag = "tag1" }, default).Result;

            postsList.Posts.Should().BeEquivalentTo(new[] { post1, post12 });
        }

        [Test]
        public void GotCorrectTotal_WhenTagWithLimitAndOffsetNotEmpty()
        {
            var post1 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { Tags = new[] { "tag1" } }, default).Result;
            var postWithOtherTag = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { Tags = new[] { "tag2" } }, default).Result;
            var post2 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { Tags = new[] { "tag1" } }, default).Result;
            var post3 = this.blogRepository
                .CreatePostAsync(new PostCreateInfo { Tags = new[] { "tag1" } }, default).Result;

            var postsList = this.blogRepository
                .SearchPostsAsync(new PostSearchInfo { Tag = "tag1", Limit = 1, Offset = 1 }, default).Result;

            postsList.Posts.Should().BeEquivalentTo(new[] { post2 });
            postsList.Total.Should().Be(3);
        }

        [Test]
        public void GotTenPosts_WhenLimitIsEmpty()
        {
            for (var i = 0; i < 15; i++)
            {
                this.blogRepository.CreatePostAsync(new PostCreateInfo(), default).Wait();
            }

            var postsList = this.blogRepository.SearchPostsAsync(new PostSearchInfo(), CancellationToken.None).Result;

            postsList.Posts.Should().HaveCount(10);
        }
    }
}
