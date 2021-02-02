using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace SciCAFE.NET.Services
{
    public class FileSettings
    {
        public string Directory { get; set; }

        // Attachment Types are files (e.g. doc) whose Content-Disposition header should be
        // set to attachment even for View File operation. These files cannot be displayed
        // directly in browser so browsers will try to save them. Providing a file name to
        // PhysicalFile() will ensure that the file is saved with the right name instead of
        // having id as its name.
        public HashSet<string> AttachmentTypes { get; set; }

        // Text Types are files (e.g. java) that should be displayed directly in browser.
        // Browsers may not display them because of their content types, so we'll overwrite
        // their content types with "text/plain".
        public HashSet<string> TextTypes { get; set; }
    }

    public class FileService
    {
        private readonly AppDbContext _db;

        private readonly FileSettings _settings;

        public FileService(AppDbContext db, IOptions<FileSettings> settings)
        {
            _db = db;
            _settings = settings.Value;
        }

        public Models.File GetFile(int id)
        {
            return _db.Files.Find(id);
        }

        public Models.File UploadFile(IFormFile uploadedFile)
        {
            string name = Path.GetFileName(uploadedFile.FileName);
            var file = new Models.File
            {
                Name = name,
                ContentType = uploadedFile.ContentType,
                Size = uploadedFile.Length,
            };
            _db.Files.Add(file);
            _db.SaveChanges();

            string diskFile = Path.Combine(_settings.Directory, file.Id.ToString());
            using (var fileStream = new FileStream(diskFile, FileMode.Create))
            {
                uploadedFile.CopyTo(fileStream);
            }

            return file;
        }

        public string GetDiskFile(int id)
        {
            return Path.Combine(_settings.Directory, id.ToString());
        }

        public void DeleteFile(int id)
        {
            var file = _db.Files.Find(id);
            if (file != null)
            {
                File.Delete(GetDiskFile(id));
                _db.Files.Remove(file);
                _db.SaveChanges();
            }
        }
    }
}
