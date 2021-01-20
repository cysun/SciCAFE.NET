using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SciCAFE.NET.Models
{
    public class File
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string ContentType { get; set; }

        public long Size { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;
    }
}
