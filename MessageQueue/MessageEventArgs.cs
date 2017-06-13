using System;

namespace MessageQueue
{
    /// <summary>
    /// Eventargs with a MessageQueue.Message 
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// The message
        /// </summary>
        public Message Message { get; private set; }

        /// <summary>
        /// instantiates new MessageEventArgs
        /// </summary>
        /// <param name="message">the message</param>
        public MessageEventArgs(Message message)
        {
            Message = message;
        }
    }
}