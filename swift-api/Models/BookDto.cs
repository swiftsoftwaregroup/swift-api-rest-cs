using System.ComponentModel.DataAnnotations;

namespace SwiftAPI.Models;

public class BookDto
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    [Required]
    public DateTime DatePublished { get; set; }

    [Required]
    [Url]
    public string CoverImage { get; set; } = string.Empty;
}