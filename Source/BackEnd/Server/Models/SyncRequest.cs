using System.ComponentModel.DataAnnotations;

namespace Sinco.Server.Models
{
    public class SyncRequest
    {
        [Required]
        public string Controller { get; set; } = string.Empty;

        [Required]
        public string FormId { get; set; } = string.Empty;

        [Required]
        public string[] PrimaryKey { get; set; } = Array.Empty<string>();

        [Required]
        public string[] Value { get; set; } = Array.Empty<string>();

        public string Type { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Language { get; set; } = "vn";

        public string? Unit { get; set; }

        public string? IdVC { get; set; }

        public string? VCDate { get; set; }

        public string? UserId { get; set; }
        public List<string> Ids { get; set; } = new();
        public string IdSync { get; set; } = string.Empty;
    }
}
