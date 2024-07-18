using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SoPro24Team06.Models;

public class Contract
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Label is required")]
    [JsonProperty("label")]
    public string Label { get; set; }

    // Für DB relationen
    public List<Assignment>? Assignments { get; set; }
    public List<AssignmentTemplate>? AssignmentsTemplates { get; set; }

    public Contract(string label)
    {
        this.Label = label;
    }

    public Contract() { }
}
