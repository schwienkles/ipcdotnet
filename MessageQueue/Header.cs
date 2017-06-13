using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueue
{
    /// <summary>
    /// The message header used in the MessageQueue.Message
    /// </summary>
    public class Header
    {
        private readonly List<KeyValuePair> m_items;

        /// <summary>
        /// gets or sets the value of 'key'.
        /// with setting, creates a new pair if the given key is not found
        /// </summary>
        /// <param name="key">the keyvaluepair of 'key' to find</param>
        /// <returns>value of the keyvaluepair of 'key'</returns>
        public string this[string key]
        {
            get { return m_items.FirstOrDefault(x => x.Key == key)?.Value; }
            set
            {
                KeyValuePair kvp = m_items.FirstOrDefault(x => x.Key == key);
                if (kvp == null)
                {
                    kvp = new KeyValuePair(key);
                    m_items.Add(kvp);
                }

                kvp.Value = value;
            }
        }

        /// <summary>
        /// Creates a new Header object
        /// </summary>
        internal Header()
        {
            m_items = new List<KeyValuePair>();
        }

        /// <summary>
        /// removes an entry from the header
        /// </summary>
        /// <param name="key">the key of the entry to remove</param>
        public void Remove(string key)
        {
            m_items.RemoveAll(x => x.Key == key);
        }

        /// <summary>
        /// the header formatted
        /// </summary>
        /// <returns>[soh][keyvaluepair][keyvaluepair n][eot]</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Queue.StartOfHead);
            foreach (var pair in m_items)
            {
                builder.Append(pair);
            }

            builder.Append(Queue.EndOfTransmissionChar);
            return builder.ToString();
        }
    }
}
