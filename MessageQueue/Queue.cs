using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueue
{
    /// <summary>
    /// The Queue to receive messages over the local loopback
    /// </summary>
    public class Queue
    {
        #region constants

        internal const char StartOfHead = (char)1;
        internal const char EndOfTextChar = (char) 3;
        internal const char EndOfTransmissionChar = (char) 4;
        internal const char NewlineChar = (char) 10;
        /// <summary>
        /// the default maximum amount of messages in the queue
        /// </summary>
        public const int DefaultMaxQueueLength = 10;
        /// <summary>
        /// the default amount of milliseconds to wait at a blocking call
        /// </summary>
        public const int DefaultTimeout = 1500;

        #endregion

        #region privates

        private bool m_stop;
        private object m_lock;
        private TcpListener m_listener;
        private Thread m_backgroundWorker;
        private List<Message> m_messages;
        private int m_maxQueueLength;

        #endregion


        /// <summary>
        /// MessageReceived is raised when a message has been received from the local loopback and has been parsed succesfully
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// instantiates a new MessageQueue.Queue
        /// </summary>
        public Queue(int maxQueueLength = DefaultMaxQueueLength)
        { 
            m_lock = new object();
            m_messages = new List<Message>();
            m_maxQueueLength = maxQueueLength;
        }

        /// <summary>
        /// starts the receiver thread for incoming connections
        /// </summary>
        /// <param name="port">the port to run on</param>
        /// <returns></returns>
        public bool BeginReceive(int port)
        {
            lock (m_lock)
            {
                if (m_backgroundWorker != null)
                {
                    return false;
                }
                
                m_backgroundWorker = new Thread(() => BackgroundWorkerListener(port))
                {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal
                };
            }
            m_stop = false;
            m_backgroundWorker.Start();
            return true;
        }

        /// <summary>
        /// stops the receiver thread
        /// </summary>
        /// <returns>false if thread was not running, true if it is joined/killed</returns>
        public bool EndReceive()
        {
            lock (m_lock)
            {
                if (m_backgroundWorker == null)
                {
                    return false;
                }
                m_stop = true;
                m_listener?.Stop();
                m_backgroundWorker.Join();
                m_backgroundWorker = null;
                return true;
            }
        }

        /// <summary>
        /// waits for a message to be received
        /// </summary>
        /// <param name="timeout">maximum amount of time to wait</param>
        /// <returns>received Message</returns>
        public Message WaitForMessage(int timeout = DefaultTimeout)
        {
            TimeSpan span = TimeSpan.FromMilliseconds(timeout);
            DateTime starttime = DateTime.UtcNow;
            while (!m_messages.Any() && DateTime.UtcNow - starttime < span)
            {
                Thread.Sleep(10);
            }

            return m_messages.Any() ? m_messages[0] : null;
        }

        /// <summary>
        /// Waits for a message to be received within the time limit, of which the given header key and header value match 
        /// </summary>
        /// <param name="headerKey">key to be available in header</param>
        /// <param name="headerValue">value to match at header key</param>
        /// <param name="timeout">max time to wait</param>
        /// <returns>message or null (if not found)</returns>
        public Message WaitForMessage(string headerKey, string headerValue, int timeout = DefaultTimeout)
        {
            var span = TimeSpan.FromMilliseconds(timeout); 
            var starttime = DateTime.UtcNow;
            Message message = null;

            while ((message = m_messages.FirstOrDefault(x => x[headerKey] == headerValue)) == null && DateTime.UtcNow - starttime < span)
            {
                Thread.Sleep(10);
            }

            if (message != null)
            {
                m_messages.Remove(message);
            }

            return message;
        }

        /// <summary>
        /// the background thread that listens for incoming connections
        /// </summary>
        /// <param name="port"></param>
        private void BackgroundWorkerListener(int port)
        {
            m_listener = new TcpListener(IPAddress.Loopback, port);
            m_listener.Start();

            
            while (!m_stop && m_listener != null && m_backgroundWorker!= null && m_backgroundWorker.IsAlive)
            {
                try
                {
                    var client = m_listener.AcceptTcpClient();
                    var message = Message.FromStream(client.GetStream());
                    if (client.Connected)
                    {
                        client.Close();
                    }

                    if (m_maxQueueLength > 0)
                    {
                        m_messages.Add(message);
                        if (m_messages.Count > m_maxQueueLength)
                        {
                            m_messages.RemoveAt(0);
                        }
                    }

                    MessageReceived?.Invoke(this, new MessageEventArgs(message));
                }
                catch (SocketException sexe)
                {
                    //stop?
                    //do not null the m_listener
                    //the EndReceive has a lock, this is an other thread
                    //if null'd, will result in a deadlock
                }
            }
        }
    }
}
