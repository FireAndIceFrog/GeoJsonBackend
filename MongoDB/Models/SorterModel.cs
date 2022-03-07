using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Models
{
    public class SorterModel
    {
        public string Key { get; set; }
        public bool IsAscending { get; set; }

        public SorterModel(string key, bool isAscending)
        {
            Key = key;
            IsAscending = isAscending;
        }
    }
}
