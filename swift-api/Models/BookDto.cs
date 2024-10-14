using System.ComponentModel.DataAnnotations;

namespace SwiftAPI.Models;

public class BookDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    [Required]
    public DateTime DatePublished { get; set; }

    [Required]
    public string CoverImage { get; set; } = string.Empty;
}

