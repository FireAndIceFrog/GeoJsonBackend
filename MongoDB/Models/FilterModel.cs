using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

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
        private readonly string[] possibleOps = new string[] { "==", ">", "<", ">=", "<=", "!=", "=?" };

        public Expression<Func<BsonDocument, bool>> CompareTo()
        {
            FilterDefinition<BsonDocument>? filterRegex = null;
            if (_operation == "=?")
            {
                var search = Regex.Escape(Comparator);

                var regexFilter = string.Format("^{0}.*", search);
                filterRegex = Builders<BsonDocument>.Filter.Regex(Key, BsonRegularExpression.Create(new Regex(regexFilter, RegexOptions.IgnoreCase)));
            }
            return _operation switch
            {
                "==" => (BsonDocument doc) => doc[Key] == Comparator,
                "!=" => (BsonDocument doc) => doc[Key] != Comparator,
                ">" => (BsonDocument doc) => doc[Key] > Comparator,
                ">=" => (BsonDocument doc) => doc[Key] >= Comparator,
                "<" => (BsonDocument doc) => doc[Key] < Comparator,
                "<=" => (BsonDocument doc) => doc[Key] <= Comparator,
                "=?" => (BsonDocument doc) => filterRegex.Inject(),
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
