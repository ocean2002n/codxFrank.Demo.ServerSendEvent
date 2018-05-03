using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReceiverClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Task t = MainAsync(args);
            t.Wait();
        }

        static async Task MainAsync(string[] args)
        {
            using (var client = new HttpClient())
            {
                using (var stream = await client.GetStreamAsync("http://localhost/SSE/api/Frank/SendEvent"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        while (true)
                        {
                            Console.WriteLine(reader.ReadLine());
                        }
                    }
                }
            }
        }
    }
}
