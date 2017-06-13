using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessageQueue;

namespace mqueuetest2
{
    class Program
    {
        static void Main(string[] args)
        {
            MessageQueue.Queue queue = new Queue();
            queue.BeginReceive(9292);

            while (true)
            {
                Message message = queue.WaitForMessage("command", "quit");
                if (message != null)
                {
                    Console.WriteLine(Encoding.UTF8.GetString(message.Body));
                }
                else
                {
                    Console.WriteLine("timeout expired, no message recelived");
                }
            }
        }
    }
}
