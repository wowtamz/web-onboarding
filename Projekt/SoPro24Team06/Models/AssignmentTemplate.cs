//-------------------------
// Author: Vincent Steiner
//-------------------------

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
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

        [Required(ErrorMessage = "Titel ist erforderlich")]
        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("instructions")]
        public string? Instructions { get; set; }

        [Required(ErrorMessage = "Fälligkeit ist erforderlich.")]
        [JsonProperty("dueIn")]
        public DueTime? DueIn { get; set; }

        [JsonProperty("forDepartments")]
        public List<Department>? ForDepartmentsList { get; set; }

        [JsonProperty("forContract")]
        public List<Contract>? ForContractsList { get; set; }

        [JsonProperty("assigneeType")]
        public AssigneeType AssigneeType { get; set; }

        [JsonProperty("assignedRoles")]
        public ApplicationRole? AssignedRole { get; set; }

        [ForeignKey("ProcessTemplate")]
        public int? ProcessTemplateId { get; set; }

        public AssignmentTemplate() { }

        public AssignmentTemplate(
            string title,
            string instructions,
            DueTime dueIn,
            List<Department> forDepartmentsList,
            List<Contract> forContractsList,
            AssigneeType assigneeType,
            ApplicationRole assignedRole,
            int? processTemplateId
        )
        {
            this.Title = title;
            this.Instructions = instructions;
            this.DueIn = dueIn;
            this.ForDepartmentsList = forDepartmentsList;
            this.ForContractsList = forContractsList;
            this.AssigneeType = assigneeType;
            this.AssignedRole = assignedRole;
            this.ProcessTemplateId = processTemplateId > 0 ? processTemplateId : null;
        }

        public AssignmentTemplate(
            int id,
            string title,
            string instructions,
            DueTime dueIn,
            List<Department> forDepartmentsList,
            List<Contract> forContractsList,
            AssigneeType assigneeType,
            ApplicationRole assignedRole,
            int? processTemplateId
        )
        {
            this.Id = id;
            this.Title = title;
            this.Instructions = instructions;
            this.DueIn = dueIn;
            this.ForDepartmentsList = forDepartmentsList;
            this.ForContractsList = forContractsList;
            this.AssigneeType = assigneeType;
            this.AssignedRole = assignedRole;
            this.ProcessTemplateId = processTemplateId > 0 ? processTemplateId : null;
        }
        public Assignment ToAssignment(ApplicationUser? user, DateTime? ProcessDueDate)
        {
            DateTime dueDate = DateTime.Today;
            if (ProcessDueDate != null)
            {
                dueDate = ProcessDueDate.Value;
            }
            if (DueIn != null)
            {
                dueDate = dueDate.AddMonths(DueIn.Months);
                dueDate = dueDate.AddDays(DueIn.Weeks * 7);
                dueDate = dueDate.AddDays(DueIn.Days);
            }
            if (DueIn != null && DueIn.Label == "ASAP")
            {
                dueDate = DateTime.Today;
            }
            // sollte berechnet werden
            return new Assignment(this, dueDate, user);
        }
    }
}
