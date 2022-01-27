using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebFileProcessing.ActionResults;

namespace WebFileProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FilesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("direct-download")]
        public async Task<ActionResult> DirectDownload()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var contentstream = await httpClient.GetStreamAsync("https://raw.githubusercontent.com/dotnet/aspnetcore/main/src/SignalR/docs/GettingStarted.md");
            return new FileStreamResult(contentstream, new MediaTypeHeaderValue("text/plain"))
            {
                FileDownloadName = "SignalR-getting-started.md"
            };
        }

        [HttpGet("download-zip")]
        public IActionResult Get()
        {
            var filenamesAndUrls = new Dictionary<string, string>
            {
                { "GettingStarted.md", "https://raw.githubusercontent.com/dotnet/aspnetcore/main/src/SignalR/docs/GettingStarted.md" },
                { "HubProtocol.md", "https://raw.githubusercontent.com/dotnet/aspnetcore/main/src/SignalR/docs/specs/HubProtocol.md" },
                { "TransportProtocols.md","https://raw.githubusercontent.com/dotnet/aspnetcore/main/src/SignalR/docs/specs/TransportProtocols.md"}
            };

            //new FileStreamResult();
            //new FileBufferingWriteStream()

            return new FileCallbackResult(new MediaTypeHeaderValue("application/octet-stream"), async (outputStream, _) =>
            {
                var httpClient = _httpClientFactory.CreateClient();
                using (var zipArchive = new ZipArchive(new BufferedStream(outputStream), ZipArchiveMode.Create))
                {
                    foreach (var kvp in filenamesAndUrls)
                    {
                        var zipEntry = zipArchive.CreateEntry(kvp.Key);
                        using (var zipStream = zipEntry.Open())
                        using (var stream = await httpClient.GetStreamAsync(kvp.Value))
                            await stream.CopyToAsync(zipStream);
                    }
                }
            })
            {
                FileDownloadName = "MyZipfile.zip"
            };
        }
    }
}
