using System;

namespace Blog.Models
{
    public sealed class PostSearchInfo 
    {
        public string Tag { get; set; }

        /// <summary>
        /// С какой даты создания (включительно)
        /// </summary>
        public DateTime? FromCreatedAt { get; set; }

        /// <summary>
        /// До какой даты создания (не включительно)
        /// </summary>
        public DateTime? ToCreatedAt { get; set; }

        /// <summary>
        /// Ограничение на количество постов в результате. Если не указано, то равно 10.
        /// </summary>
        public int? Limit { get; set; }

        public int? Offset { get; set; }
    }
}
