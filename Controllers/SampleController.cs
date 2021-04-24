using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController : ControllerBase
    {
        [RequestSizeLimit(long.MaxValue)]
        [HttpPost("bugged")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadMultipartUsingReader()
        {
            Request.EnableBuffering(); // This causes issues for some reason
            var boundary = GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
            var reader = new MultipartReader(boundary, Request.Body, 80 * 1024);

            MultipartSection section;

            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                var contentDispo = section.GetContentDispositionHeader();

                if (contentDispo.IsFileDisposition())
                {
                    var fileSection = section.AsFileSection();

                    try
                    {

                        // exception incoming here
                        var archive = new ZipArchive(fileSection.FileStream);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            return Ok();
        }

        [RequestSizeLimit(long.MaxValue)]
        [HttpPost("working")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> WorkingUpload()
        {
            //Request.EnableBuffering();

            // When above is removed, all works as expected
            var boundary = GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
            var reader = new MultipartReader(boundary, Request.Body, 80 * 1024);

            MultipartSection section;

            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                var contentDispo = section.GetContentDispositionHeader();

                if (contentDispo.IsFileDisposition())
                {
                    var fileSection = section.AsFileSection();

                    try
                    {

                        // no exception
                        var archive = new ZipArchive(fileSection.FileStream);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            return Ok();
        }

        private static string GetBoundary(MediaTypeHeaderValue contentType)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
            return boundary;
        }

    }
}
