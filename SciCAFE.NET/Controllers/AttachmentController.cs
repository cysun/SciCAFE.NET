using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    public class AttachmentController : Controller
    {
        private readonly EventService _eventService;
        private readonly RewardService _rewardService;
        private readonly FileService _fileService;

        private readonly ILogger<AttachmentController> _logger;

        public AttachmentController(EventService eventService, RewardService rewardService,
            FileService fileService, ILogger<AttachmentController> logger)
        {
            _eventService = eventService;
            _rewardService = rewardService;
            _fileService = fileService;
            _logger = logger;
        }

        public IActionResult View(string type, int id)
        {
            return Download(type, id, true);
        }

        public IActionResult Download(string type, int id, bool inline = false)
        {
            int? fileId;
            switch (type.ToLower())
            {
                case "event":
                    var eventAttachment = _eventService.GetAttachment(id);
                    fileId = eventAttachment?.FileId;
                    break;

                case "reward":
                    var rewardAttachment = _rewardService.GetAttachment(id);
                    fileId = rewardAttachment?.FileId;
                    break;

                default:
                    _logger.LogWarning("Unsupported attachment type {type}", type);
                    return BadRequest();
            }

            return fileId != null ? DownloadFile((int)fileId, inline) : NotFound();
        }

        private IActionResult DownloadFile(int id, bool inline = false)
        {
            var file = _fileService.GetFile(id);
            if (file == null) return NotFound();

            var diskFile = _fileService.GetDiskFile(file.Id);

            inline = inline && !_fileService.IsAttachmentType(file.Name);
            return !inline ? PhysicalFile(diskFile, file.ContentType, file.Name) :
                PhysicalFile(diskFile, _fileService.IsTextType(file.Name) ? "text/plain" : file.ContentType);
        }
    }
}
