using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi.Controllers
{
    public class FrankController : ApiController
    {
        [HttpPost, HttpGet]
        public HttpResponseMessage SendEvent(CancellationToken clientDisconnectToken)
        {
            //https://stackoverflow.com/questions/44851970/implement-sending-server-sent-events-in-c-sharp-no-asp-net-mvc
            //https://forums.asp.net/t/1885055.aspx?ASP+NET+Web+API+and+HTML+5+Server+Sent+Events+aka+EventSource+
            //https://hk.saowen.com/a/2b72c434a011e8d6c1958df273feca62fe3bc5a7cd01e85fd9343d0906e1b875
            var response = Request.CreateResponse();
            response.Content = new PushStreamContent(async (stream, httpContent, transportContext) =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    using (var consumer = new BlockingCollection<string>())
                    {
                        var eventGeneratorTask = EventGeneratorAsync(consumer, clientDisconnectToken);
                        foreach (var @event in consumer.GetConsumingEnumerable(clientDisconnectToken))
                        {
                            await writer.WriteLineAsync("data: " + @event);
                            await writer.WriteLineAsync();
                            await writer.FlushAsync();
                        }
                        await eventGeneratorTask;
                    }
                }
            }, "text/event-stream");
            return response;
        }

        private async Task EventGeneratorAsync(BlockingCollection<string> producer, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    producer.Add(DateTime.Now.ToString(), cancellationToken);
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                producer.CompleteAdding();
            }
        }
    }
}
