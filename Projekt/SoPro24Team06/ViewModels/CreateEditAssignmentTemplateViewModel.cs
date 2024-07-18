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
                    if (fd != null) // Check for null before accessing properties
                    {
                        ForDepartmentsList.Add(fd.Name ?? ""); // Ensure fd.Name is not null
                    }
                }
            }

            ForContractsList = new List<string>();
            if (assignmentTemplate.ForContractsList != null)
            {
                foreach (var fc in assignmentTemplate.ForContractsList)
                {
                    if (fc != null) // Check for null before accessing properties
                    {
                        ForContractsList.Add(fc.Label ?? ""); // Ensure fc.Label is not null
                    }
                }
            }

            string vorNach = "nach Arbeitsbeginn"; // Default value
            if (assignmentTemplate.DueIn != null)
            {
                if (
                    assignmentTemplate.DueIn.Days < 0
                    || assignmentTemplate.DueIn.Weeks < 0
                    || assignmentTemplate.DueIn.Months < 0
                )
                {
                    vorNach = "vor Start";
                }
            }

            Id = assignmentTemplate.Id;
            this.processId = processId ?? null;
            Title = assignmentTemplate.Title;
            Instructions = assignmentTemplate.Instructions ?? ""; // Ensure Instructions is not null
            DueIn = assignmentTemplate.DueIn?.Label ?? ""; // Ensure DueIn.Label is not null
            AssigneeType = assignmentTemplate.AssigneeType.ToString() ?? "";
            AssignedRole = assignmentTemplate.AssignedRole?.Name ?? ""; // Ensure AssignedRole.Name is not null
            Days = assignmentTemplate.DueIn?.Days ?? 0; // Ensure DueIn.Days is not null
            Weeks = assignmentTemplate.DueIn?.Weeks ?? 0; // Ensure DueIn.Weeks is not null
            Months = assignmentTemplate.DueIn?.Months ?? 0; // Ensure DueIn.Months is not null
            VorNach = vorNach;
        }
    }
}
