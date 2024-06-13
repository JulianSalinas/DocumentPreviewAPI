using Azure.Identity;
using Azure.Storage.Blobs;
using DocumentPreviewAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Web;

namespace DocumentPreviewAPI.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FileControllerController : ControllerBase
    {
        private IConfiguration Configuration;

        private readonly DynamicSettings Settings;

        //private readonly GraphServiceClient _graphServiceClient;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<FileControllerController> _logger;

        public FileControllerController(
            ILogger<FileControllerController> logger,
            IConfiguration configuration,
            IOptionsSnapshot<DynamicSettings> settingsSnapshot)//, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            Configuration = configuration;
            Settings = settingsSnapshot.Value;
            //_graphServiceClient = graphServiceClient;
        }

        [HttpGet("{fileName}/{imageSize}")]
        public async Task<IActionResult> GetFileInSize(string fileName, ImageSize imageSize)
        {
            var message = Settings.TestMessage + fileName + imageSize.GetPrefix();

            return Ok(new { message });
        }

        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> Get(string fileName, ImageSize imageSize)
        {
            var decodedFileName = HttpUtility.UrlDecode(fileName);

            var previewFileName = $"{imageSize.GetPrefix()}/{Path.GetFileNameWithoutExtension(decodedFileName)}-{imageSize.GetWidth()}x{imageSize.GetHeight()}{Path.GetExtension(decodedFileName)}";

            var credential = new ChainedTokenCredential(
                new ManagedIdentityCredential(Configuration["ManagedIdentityClientId"]),
                new VisualStudioCodeCredential());

            //StringValues authorizationToken;

            //HttpContext.Request.Headers.TryGetValue("Authorization", out authorizationToken);

            //var token = authorizationToken.ToString().Replace("Bearer ", "");

            //var secret = Configuration["AzureAD:ClientSecret"];

            //var credential = new ChainedTokenCredential(
            //    new OnBehalfOfCredential(
            //        Configuration["AzureAD:TenantId"],
            //        Configuration["AzureAD:ClientId"],
            //        secret,
            //        token));

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

        [HttpGet("Test")]
        public IActionResult TestConfiguration()
        {
            return Ok(Settings.TestMessage);
        }
    }
}
