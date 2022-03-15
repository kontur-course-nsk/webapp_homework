using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models
{
    public sealed class Connection
    {
        private IMongoCollection<Post> collection;
        public IMongoCollection<Post> Collection { get => collection; }
        public Connection()
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);
            IMongoDatabase database = client.GetDatabase("blog");
            collection = database.GetCollection<Post>("posts");
        }
    }
}
