using ChatAPP.API.Contexts;
using ChatAPP.API.Utils;

namespace ChatAPP.API.Services
{
    public class FileUploadServices: BaseServices<FileUpload>
    {
        public FileUploadServices(ChatAPPContext context, ILogger<FileUpload> logger) : base(context, logger)
        {
        }

        public FileUpload? GetFile(int fileId)
        {
            try
            {
                var file = _context.FileUploads.Where(x => x.Id == fileId).FirstOrDefault() ?? null;
                return file!;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }

        public DefaultResponse UploadFile(IFormFile file)
        {
            try
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "uploads", fileName);

                var fileUpload = new FileUpload
                {
                   Name = fileName,
                   Path = filePath,
                   Size = int.Parse(file.Length.ToString()),
                };
                fileUpload.Id = Add(fileUpload);
                if (fileUpload.Id == 0)
                {
                    return new DefaultResponse { Message = "Upload failed" };
                }
                return new DefaultResponse { Message = "Upload success", Data = fileUpload };
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

    }
}
