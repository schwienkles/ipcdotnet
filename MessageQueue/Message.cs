using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MessageQueue
{
    /// <summary>
    /// Message for the MessageQueue.Message
    /// This message can be send directly to any port
    /// </summary>
    public class Message
    {
        private byte[] m_body;

        /// <summary>
        /// The header for the current Message
        /// Header entries are meant as metadata for the body
        /// </summary>
        public MessageQueue.Header Header { get; private set; }

        /// <summary>
        /// The body for the current message
        /// The body is meant to be used as the data to be send
        /// </summary>
        public byte[] Body
        {
            get { return m_body ?? (m_body = new byte[1]); }
            set { m_body = value; }
        }

        /// <summary>
        /// gets or sets a value at key in the Message header
        /// </summary>
        /// <param name="key">key to set</param>
        /// <returns>the value</returns>
        public string this[string key]
        {
            get { return Header[key]; }
            set { Header[key] = value; }
        }

        /// <summary>
        /// Instantiates a new Message
        /// </summary>
        public Message()
        {
            Header = new Header();
        }

        /// <summary>
        /// Sends the current message to the given port
        /// the '__port' header key is used for the returnport
        /// </summary>
        /// <param name="port">port to send to</param>
        /// <param name="returnport">return port is embedded in the message header so a response can be send to 'returnport'</param>]
        /// <returns>true on success</returns>
        public bool SendTo(int port, int returnport = 0)
        {
            bool success = false;

            if (returnport > 0)
            {
                Header["__port"] = returnport.ToString();
            }
            else
            {
                Header.Remove("__port");
            }

            try
            {
                var client = new TcpClient();
                client.Connect(new IPEndPoint(IPAddress.Loopback, port));
                var stream = client.GetStream();

                var headerBytes = Encoding.UTF8.GetBytes(Header.ToString());
                stream.Write(headerBytes, 0, headerBytes.Length);

                if (Body != null)
                {
                    stream.Write(Body, 0, Body.Length);
                }

                client.Close();
                success = true;
            }
            catch (SocketException sexe)
            {
            }
            finally
            {
                Header.Remove("__port");
            }

            return success;
        }

        /// <summary>
        /// Creates a new message from stream data (receiving end)
        /// </summary>
        /// <param name="stream">stream to read</param>
        /// <returns>Message received from stream</returns>
        internal static Message FromStream(Stream stream)
        {
            var message = new Message();
            var text = new StringBuilder();
            
            AssertChar(stream, Queue.StartOfHead);
            FillHead(stream, message);

            var body = new List<byte>();
            var buffer = new byte[32];
            var read = 0;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                byte[] data;
                if (read == buffer.Length)
                {
                    data = buffer;
                }
                else
                {
                    data = new byte[read];
                    Buffer.BlockCopy(buffer, 0, data, 0, read);
                }

                body.AddRange(data);
            }
            message.Body = body.ToArray();
            return message;
        }

        /// <summary>
        /// throws an exception if the next readable char (or the stream is closed) is not equal to 'expected' char
        /// </summary>
        /// <param name="stream">stream to read</param>
        /// <param name="expected">char for comparison</param>
        private static void AssertChar(Stream stream, char expected)
        {
            var buffer = new byte[1];
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length || buffer[0] != expected)
            {
                throw new InvalidDataException($"expected '{expected}' but received '{buffer[0]}'");
            }
        }


        /// <summary>
        /// fills the head with header entries based on the given stream
        /// </summary>
        /// <param name="stream">stream to read</param>
        /// <param name="message">message to alter</param>
        private static void FillHead(Stream stream, Message message)
        {
            var builder = new StringBuilder();
            var buffer = new byte[1];
            while (stream.Read(buffer, 0, buffer.Length) == buffer.Length && buffer[0] != Queue.EndOfTransmissionChar)
            {
                builder.Append((char) buffer[0]);
            }

            var regex = new Regex($"(.*?){Queue.EndOfTextChar}(.*?)\n", RegexOptions.Multiline);
            var matches = regex.Matches(builder.ToString());

            foreach (Match match in matches)
            {
                message.Header[match.Groups[1].Value] = match.Groups[2].Value;
            }
        }
    }
}
