//-------------------------
// Author: Vincent Steiner
//-------------------------

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels
{
    public class CreateEditAssignmentTemplateViewModel
    {
        public int Id { get; set; }
        public int? processId { get; set; }

        [Required(ErrorMessage = "Der Titel ist erforderlich")]
        public string Title { get; set; }

        public string? Instructions { get; set; }

        [Required(ErrorMessage = "Ein Fälligkeit ist erforderlich")]
        public string DueIn { get; set; }

        public List<string>? ForDepartmentsList { get; set; }
        public List<string>? ForContractsList { get; set; }

        [Required(ErrorMessage = "Ein Aufgabenverantwortlicher ist erforderlich")]
        public string AssigneeType { get; set; }

        public string AssignedRole { get; set; }

        public int Days { get; set; }
        public int Weeks { get; set; }
        public int Months { get; set; }

        public string? VorNach { get; set; }

        public CreateEditAssignmentTemplateViewModel() { }

        public CreateEditAssignmentTemplateViewModel( 
            int id,
            int? processId,
            string title,
            string? instructions,
            string dueIn,
            List<string>? forDepartmentsList,
            List<string>? forContractsList,
            string assigneeType,
            string assignedRole,
            int days,
            int weeks,
            int months,
            string? vorNach
        )
        {
            Id = id;
            this.processId = processId;
            Title = title;
            Instructions = instructions;
            DueIn = dueIn;
            ForDepartmentsList = forDepartmentsList;
            ForContractsList = forContractsList;
            AssigneeType = assigneeType;
            AssignedRole = assignedRole;
            Days = days;
            Weeks = weeks;
            Months = months;
            VorNach = vorNach;
        }

        public CreateEditAssignmentTemplateViewModel(int? processId)
        {
            this.processId = processId ?? null;

            ForDepartmentsList = new List<string>();
            ForContractsList = new List<string>();

            Title = "";
            Instructions = "";
            DueIn = "";
            AssigneeType = "";
            AssignedRole = "";
            Days = 0;
            Weeks = 0;
            Months = 0;
            VorNach = "nach Arbeitsbeginn";
        }

        public CreateEditAssignmentTemplateViewModel( 
            AssignmentTemplate assignmentTemplate,
            int? processId
        )
        {
            ForDepartmentsList = new List<string>();
            if (assignmentTemplate.ForDepartmentsList != null)
            {
                foreach (var fd in assignmentTemplate.ForDepartmentsList)
                {
                    if (fd != null) 
                    {
                        ForDepartmentsList.Add(fd.Name ?? ""); 
                    }
                }
            }

            ForContractsList = new List<string>();
            if (assignmentTemplate.ForContractsList != null)
            {
                foreach (var fc in assignmentTemplate.ForContractsList)
                {
                    if (fc != null) 
                    {
                        ForContractsList.Add(fc.Label ?? "");
                    }
                }
            }
            int daysHelper = 0;
            int weeksHelper = 0;
            int monthsHelper = 0;
            string vorNach = "nach Arbeitsbeginn"; 
            if (assignmentTemplate.DueIn != null)
            {
                if (
                    assignmentTemplate.DueIn.Days < 0
                    || assignmentTemplate.DueIn.Weeks < 0
                    || assignmentTemplate.DueIn.Months < 0
                )
                {
                    daysHelper = assignmentTemplate.DueIn.Days * -1;
                    weeksHelper = assignmentTemplate.DueIn.Weeks * -1;
                    monthsHelper = assignmentTemplate.DueIn.Months * -1;
                    vorNach = "vor Start";
                }else{
                    daysHelper = assignmentTemplate.DueIn.Days;
                    weeksHelper = assignmentTemplate.DueIn.Weeks;
                    monthsHelper = assignmentTemplate.DueIn.Months;
                }
            }

            Id = assignmentTemplate.Id;
            this.processId = processId ?? null;
            Title = assignmentTemplate.Title;
            Instructions = assignmentTemplate.Instructions ?? ""; 
            DueIn = assignmentTemplate.DueIn?.Label ?? ""; 
            AssigneeType = assignmentTemplate.AssigneeType.ToString() ?? "";
            AssignedRole = assignmentTemplate.AssignedRole?.Name ?? ""; 
            Days = daysHelper;
            Weeks = weeksHelper; 
            Months = monthsHelper; 
            VorNach = vorNach;
        }
    }
}
