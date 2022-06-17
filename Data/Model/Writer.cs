using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Model
{
    public class Writer
    {
        [BsonElement("WriterId")]
        public int WriterId { get; set; }

        [BsonElement("WriterName")]
        public string WriterName { get; set; }

    }
}
