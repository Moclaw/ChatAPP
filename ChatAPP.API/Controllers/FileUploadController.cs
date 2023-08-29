using ChatAPP.API.Models.PostModels;
using ChatAPP.API.Services;
using ChatAPP.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class FileUploadController : ControllerBase
    {
        private readonly FileUploadServices _fileUploadServices;
        private readonly ILogger<FileUploadController> _logger;
        public FileUploadController(FileUploadServices fileUploadServices, ILogger<FileUploadController> logger)
        {
            _fileUploadServices = fileUploadServices;
            _logger = logger;
        }
        [HttpGet]
        public ActionResult GetFile(int fileId)
        {
            var result = _fileUploadServices.GetFile(fileId);
            if (result == null)
            {
                return BadRequest(new DefaultResponse { Message = "File Not Found" });
            }
            return Ok(new DefaultResponse { Message = "File Found", Data = result });
        }

        [HttpPost]
        public ActionResult UploadFile(IFormFile file)
        {
            try
            {
                var result = _fileUploadServices.UploadFile(file);
                if (result.Data == null)
                {
                    return BadRequest(new DefaultResponse { Message = result.Message });
                }
                return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(new DefaultResponse { Message = "Error Uploading File" });
            }
          
        }

    }
}
