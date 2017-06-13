using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MessageQueue;
namespace mquetest
{
    class Program
    {
        private static void Main(string[] args)
        {
            //bool stop = false;
            //int inport;
            //int outport;
            //
            //Console.Write("port to receive on: ");
            //inport = int.Parse(Console.ReadLine());
            //Console.Write("port to send to: ");
            //outport = int.Parse(Console.ReadLine());
            //
            //new Thread(() =>
            //{
            //    MessageQueue.Queue queue = new MessageQueue.Queue();
            //    queue.BeginReceive(inport);
            //    queue.MessageReceived += (sender, eventArgs) =>
            //    {
            //        if (eventArgs.Message["command"] == "quit")
            //        {
            //            Environment.Exit(0);
            //        }
            //
            //        if (eventArgs.Message.Body != null)
            //        {
            //            Console.WriteLine("received: " + Encoding.UTF8.GetString(eventArgs.Message.Body));
            //        }
            //    };
            //
            //    while (!stop)
            //    {
            //        Thread.Sleep(50);
            //    }
            //    queue.EndReceive();
            //})
            //{
            //    IsBackground = true,
            //    Priority = ThreadPriority.BelowNormal
            //}.Start();
            //
            //
            //string text = string.Empty;
            //while (text != "exit")
            //{
            //    text = Console.ReadLine();
            //    Message m = new Message {["command"] ="quit",Body = Encoding.UTF8.GetBytes(text)};
            //    m.SendTo(outport, inport);
            //}
            //
            //try
            //{
            //    Message m = new Message {Header = {["command"] = "quit"}};
            //    m.SendTo(outport, inport);
            //}
            //catch (Exception e /*if the other end is already closed, ignore*/)
            //{
            //    
            //}
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < 10; i ++)
            {
                Thread t = new Thread(bg)
                {
                    Name = "udp #" + i,
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal
                };
                threads.Add(t);
                t.Start();
            }


            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse("127.0.0.1")));
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
            s.Connect(new IPEndPoint(IPAddress.Loopback, 9291));

            string text = string.Empty;
            while (text != "exit")
            {
                text = Console.ReadLine();
                byte[] data = Encoding.UTF8.GetBytes(text);
                s.Send(data, 0, data.Length, SocketFlags.None);
            }

            foreach (var t in threads)
            {
                t.Abort();
            }
    }

        static void bg()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.1")));
            s.Bind(new IPEndPoint(IPAddress.Loopback, 9291));


            while (true)
            {
                byte[] data = new byte[100];
                s.Receive(data);
                Console.WriteLine($"thread {Thread.CurrentThread.Name} received datagram: '{Encoding.UTF8.GetString(data)}'");
            }
        }
    }
}
