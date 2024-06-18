using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SoPro24Team06.Models.SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels
{
    public class AssignmentViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("title")]
        public string? Title { get; set; }
        [JsonProperty("instructions")]
        public string? Instructions { get; set; }
        [JsonProperty("dueTime")]
        public DueTime DueIn { get; set; }
        [JsonProperty("forDepartments")]
        public List<Department> ForDepartmentsList { get; set; }
        [JsonProperty("forContract")]
        public List<Contract> ForContractsList { get; set; }
        [JsonProperty("assigneeType")]
        public AssigneeType AssigneeType { get; set; }
        [JsonProperty("assignedRoles")]
        public List<Role> AssignedRolesList { get; set; }

        // #TODO: Additional Attributes
        public AssignmentViewModel(AssignmentTemplate template)
        {
            this.Title = template.Title;
            this.Instructions = template.Instructions;
            this.DueIn = template.DueIn;
            this.ForDepartmentsList = template.ForDepartmentsList;
            this.ForContractsList = template.ForContractsList;
            this.AssigneeType = template.AssigneeType;
        }
    }

}