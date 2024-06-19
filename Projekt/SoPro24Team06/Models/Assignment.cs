using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using SoPro24Team06.Enums;

namespace SoPro24Team06.Models
{
    public class Assignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("instructions")]
        public string? Instructions { get; set; }

        [JsonProperty("dueDate")]
        public DateTime DueDate { get; set; }

        [JsonProperty("forDepartments")]
        public List<Department> ForDepartmentsList { get; set; }

        [JsonProperty("forContract")]
        public List<Contract> ForContractsList { get; set; }

        [JsonProperty("assigneeType")]
        public AssigneeType AssigneeType { get; set; }

        [JsonProperty("assignedRoles")]
        public List<IdentityRole> AssignedRolesList { get; set; }

        [JsonProperty("status")]
        public AssignmentStatus Status { get; set; }

        // #TODO: Additional Attributes
        public Assignment(AssignmentTemplate template, DateTime dueDate)
        {
            this.Title = template.Title;
            this.Instructions = template.Instructions;
            this.ForDepartmentsList = template.ForDepartmentsList;
            this.ForContractsList = template.ForContractsList;
            this.AssigneeType = template.AssigneeType;
            this.AssignedRolesList = template.AssignedRolesList;
            this.DueDate = dueDate;
        }
    }

}