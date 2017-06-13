using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueue
{
    /// <summary>
    /// KeyValuePair class that is used in the MessageQueue.Header class
    /// </summary>
    public class KeyValuePair
    {
        /// <summary>
        /// The key of the key-value-pair
        /// </summary>
        public string Key { get; private set; }
        /// <summary>
        /// The value of the key-value-pair
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// instantiates a new KeyValuePair, only sets the key
        /// </summary>
        /// <param name="key">key of the pair</param>
        internal KeyValuePair(string key)
        {
            Key = key;
        }

        /// <summary>
        /// instantiates a new key value pair
        /// </summary>
        /// <param name="key">key of the pair</param>
        /// <param name="value">value of the pair</param>
        internal KeyValuePair(string key, string value) : this(key)
        {
            Value = value;
        }

        /// <summary>
        /// the keyvaluepair formatted for heading
        /// </summary>
        /// <returns>[stx][key][etx][value][null] (withouth the brackets) </returns>
        public override string ToString()
        {
            return $"{Key}{Queue.EndOfTextChar}{Value}{Queue.NewlineChar}";
        }
    }
}
