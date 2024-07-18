using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SoPro24Team06.Models
{
    public class Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Der Name ist erforderlich.")]
        [JsonProperty("name")]
        public string Name { get; set; }

        // Für DB relationen
        public List<Assignment>? Assignments { get; set; }
        public List<AssignmentTemplate>? AssignmentsTemplates { get; set; }

        public Department(string name)
        {
            Name = name;
        }

        public Department() { }
    }
}
