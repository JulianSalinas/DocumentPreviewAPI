using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.Graph;
using DocumentPreviewAPI.Utilities;
using Azure.Identity;
using Azure.Storage.Blobs;
using System.Web;

namespace DocumentPreviewAPI.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class FileControllerController : ControllerBase
    {
        private IConfiguration Configuration;

        //private readonly GraphServiceClient _graphServiceClient;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<FileControllerController> _logger;

        public FileControllerController(ILogger<FileControllerController> logger, IConfiguration configuration)//, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            Configuration = configuration;
            //_graphServiceClient = graphServiceClient;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string fileName, ImageSize imageSize)
        {
            var decodedFileName = HttpUtility.UrlDecode(fileName);

            var previewFileName = $"{imageSize.GetPrefix()}/{Path.GetFileNameWithoutExtension(decodedFileName)}-{imageSize.GetWidth()}x{imageSize.GetHeight()}{Path.GetExtension(decodedFileName)}";

            var options = new DefaultAzureCredentialOptions
            {
            };

            var credential = new DefaultAzureCredential();

            var uri = new Uri("https://jslearning.blob.core.windows.net/");

            var blobServiceClient = new BlobServiceClient(uri, credential);

            var blobContainerClient = blobServiceClient.GetBlobContainerClient("preview-documents");

            var blobClient = blobContainerClient.GetBlobClient(previewFileName);

            try
            {
                var stream = await blobClient.OpenReadAsync();

                return File(stream, "application/octet-stream");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file");

                throw;
            }
        }

        [HttpGet("Endpoint")]
        public IActionResult GetBlobStorageEndpoint()
        {
            return Ok(Configuration["BlobStorageURL"]);
        }
    }
}
