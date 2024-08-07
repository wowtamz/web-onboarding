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

        [Required(ErrorMessage = "Title is Required")]
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("instructions")]
        public string? Instructions { get; set; }

        [Required(ErrorMessage = "DueDate is Required")]
        [JsonProperty("dueDate")]
        public DateTime DueDate { get; set; }

        [JsonProperty("forDepartments")]
        public List<Department> ForDepartmentsList { get; set; } = new List<Department>();

        [JsonProperty("forContract")]
        public List<Contract> ForContractsList { get; set; } = new List<Contract>();

        [Required(ErrorMessage = "AssigneeType is Required")]
        [JsonProperty("assigneeType")]
        public AssigneeType AssigneeType { get; set; }

        [JsonProperty("assingee")]
        public ApplicationUser? Assignee { get; set; }

        [JsonProperty("assignedRole")]
        public ApplicationRole? AssignedRole { get; set; }

        [Required(ErrorMessage = "Status is Required")]
        [JsonProperty("status")]
        public AssignmentStatus Status { get; set; }

        /// <summary>
        /// Construct Assignemt out of Tempalte, with specific due date and responsible User
        /// </summary>
        /// <param name="template">Teplante on wich to base the Assignment will be based</param>
        /// <param name="dueDate">Date until wich the Assignment should be done</param>
        /// <param name="assignee">User responsible for Assignment, can be null if no responsbile user specified</param>
        public Assignment(AssignmentTemplate template, DateTime dueDate, ApplicationUser? assignee)
        {
            if (String.IsNullOrWhiteSpace(template.Title))
            {
                this.Title = "Ohne Titel";
            }
            else
            {
                this.Title = template.Title;
            }
            this.Instructions = template.Instructions;
            this.ForDepartmentsList = template.ForDepartmentsList ?? new List<Department>();
            this.ForContractsList = template.ForContractsList ?? new List<Contract>();
            if (template.AssigneeType == AssigneeType.ROLES)
            {
                if (template.AssignedRole != null)
                    this.AssignedRole = template.AssignedRole;
                this.AssigneeType = template.AssigneeType;
            }
            else
            {
                if (assignee != null)
                    this.Assignee = assignee;
                this.AssigneeType = AssigneeType.USER;
            }
            this.DueDate = dueDate.Date;
            this.Status = AssignmentStatus.NOT_STARTED;
        }

        /// <summary>
        /// creates Empty Assignment
        /// </summary>
        public Assignment()
        {
            Title = "";
        }

        /// <summary>
        /// Returns Days Till DueDate of this Assignment
        /// equals 0 if DueDate is Today
        /// </summary>
        /// <returns></returns>
        public int GetDaysTillDueDate()
        {
            return (this.DueDate.Date - DateTime.Today).Days;
        }
    }
}
