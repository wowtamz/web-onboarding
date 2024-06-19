using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using SoPro24Team06.Enums;

namespace SoPro24Team06.Models
{
    public class AssignmentTemplate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonProperty("id")]

        public int Id { get; set; }

        [Required(ErrorMessage = "Title ist erforderlich")]
        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("instructions")]
        public string? Instructions { get; set; }

        [Required(ErrorMessage = "Zeit bis wann die Aufgabe erledigt werden muss ist erforderlich.")]
        [JsonProperty("dueTime")]
        public DueTime DueIn { get; set; }

        [JsonProperty("forDepartments")]
        public List<Department> ForDepartmentsList { get; set; }

        [JsonProperty("forContract")]
        public List<Contract> ForContractsList { get; set; }

        [JsonProperty("assigneeType")]
        public AssigneeType AssigneeType { get; set; }

        [JsonProperty("assignedRoles")]
        public List<IdentityRole> AssignedRolesList { get; set; }

        public Assignment ToAssignment(AssignmentTemplate template)
        {
            DateTime dueDate = DateTime.Now; // sollte berechnet werden
            return new Assignment(template, dueDate);
        }

    }

}