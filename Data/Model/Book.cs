using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Model
{
    public class Book
    {
        [BsonElement("BookId")]
        public int BookId { get; set; }

        [BsonElement("BookName")]
        public string BookName { get; set; }

        [BsonElement("BookShelf")]
        public string BookShelf { get; set; }

        [BsonElement("WriterId")]
        public int WriterId { get; set; }
    }
}
