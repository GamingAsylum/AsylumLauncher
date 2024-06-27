using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsylumLauncher.Models
{
    public class News
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? NewsURL { get; set; }

        public string? Author { get; set; }

        public DateOnly? ReleaseDate { get ; set; }
    }
}
