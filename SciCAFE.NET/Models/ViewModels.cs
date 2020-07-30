using System;

namespace SciCAFE.NET.Models
{
    public class StatusViewModel
    {
        public string PageTitle { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
