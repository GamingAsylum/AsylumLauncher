using System;

namespace AsylumLauncher.Models
{
    public class News
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? NewsURL { get; set; }

        public string? Author { get; set; }

        public DateOnly? ReleaseDate { get; set; }
    }
}
