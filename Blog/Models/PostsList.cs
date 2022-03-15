namespace Blog.Models
{
    public sealed class PostsList
    {
        public Post[] Posts { get; set; }

        /// <summary>
        /// Количество постов, подходящее под поисковый запрос без учёта Limit и Offset.
        /// </summary>
        public long Total { get; set; }
    }
}
