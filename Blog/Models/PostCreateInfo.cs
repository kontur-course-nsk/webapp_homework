using System;
using MongoDB.Bson;

namespace Blog.Models 
{
    public sealed class PostCreateInfo
    {
        public string Title { get; set; }

        public string Text { get; set; }

        public string[] Tags { get; set; }

        /// <summary>
        /// Дата создания поста. Проставляется автоматически при создании если null.
        /// </summary>
        public DateTime? CreatedAt { get; set; }
    }
}
