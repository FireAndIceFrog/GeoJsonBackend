using MongoDB.Bson;
using System.Linq.Expressions;

namespace MongoDB.Models
{
    public class FilterModel
    {
        public string Key { get; set; }
        public string? Operation 
        { 
            get
            {
                return _operation;
            }
            set
            {
                if (possibleOps.Contains(value))
                {
                    _operation = value;
                }
                else
                {
                    _operation = null;
                    throw new InvalidOperationException(value);
                }
            }
        }
        public string Comparator { get; set; }

        private string? _operation;
        private readonly string[] possibleOps = new string[] { "==", ">", "<", ">=", "<=", "!=" };

        public Expression<Func<BsonDocument, bool>> CompareTo()
        {
            return _operation switch
            {
                "==" => (BsonDocument doc) => doc[Key] == Comparator,
                "!=" => (BsonDocument doc) => doc[Key] != Comparator,
                ">" => (BsonDocument doc) => doc[Key] > Comparator,
                ">=" => (BsonDocument doc) => doc[Key] >= Comparator,
                "<" => (BsonDocument doc) => doc[Key] < Comparator,
                "<=" => (BsonDocument doc) => doc[Key] <= Comparator,
                _ => (BsonDocument doc) => true,
            };
        }

        public FilterModel(string key, string operation, string comparator)
        {
            Key = key;
            Operation = operation;
            Comparator = comparator;
        }
     }
}
