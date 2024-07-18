//-------------------------
// Author: Tamas Varadi
//-------------------------

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class EditProcessViewModel
{
    public int? Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    public List<AssignmentTemplate>? AssignmentTemplates { get; set; }

    public List<Assignment>? Assignments { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }

    [Required(ErrorMessage = "Vorgangsverantwortlicher ist erforderlich")]
    public ApplicationUser Supervisor { get; set; }

    [Required(ErrorMessage = "Bezugsperson ist erforderlich")]
    public ApplicationUser WorkerOfReference { get; set; }

    [Required(ErrorMessage = "Vertragsart ist erforderlich")]
    public Contract ContractOfRefWorker { get; set; }

    [Required(ErrorMessage = "Abteilung ist erforderlich")]
    public Department DepartmentOfRefWorker { get; set; }

    public EditProcessViewModel(Process process)
    {
        this.Title = process.Title;
        this.Description = process.Description;
        this.StartDate = process.StartDate;
        this.DueDate = process.DueDate;
        this.Assignments = process.Assignments;
        this.Supervisor = process.Supervisor;
        this.WorkerOfReference = process.WorkerOfReference;
        this.ContractOfRefWorker = process.ContractOfRefWorker;
        this.DepartmentOfRefWorker = process.DepartmentOfRefWorker;
    }

    public EditProcessViewModel(ProcessTemplate? processTemplate)
    {
        if (processTemplate != null)
        {
            Process process = new Process(processTemplate);

            this.Title = process.Title;
            this.Description = process.Description;
            this.AssignmentTemplates = new List<AssignmentTemplate> { };
            this.ContractOfRefWorker = process.ContractOfRefWorker;
            this.DepartmentOfRefWorker = process.DepartmentOfRefWorker;
        }
    }

    public EditProcessViewModel()
    {
        this.Id = null;
        this.Title = "";
        this.Description = "";
        this.AssignmentTemplates = new List<AssignmentTemplate> { };
        this.StartDate = DateTime.Now;
        this.DueDate = DateTime.Now.AddDays(7);
        this.Supervisor = new ApplicationUser { FullName = "Der Verantwortlicher" };
        this.WorkerOfReference = new ApplicationUser { FullName = "Der Bezugsperson" };
        this.ContractOfRefWorker = new Contract("Vollzeit");
        this.DepartmentOfRefWorker = new Department("Operations");
    }

    public Process ToProcess()
    {
        return new Process(
            Title,
            Description,
            new List<Assignment> { },
            WorkerOfReference,
            Supervisor,
            ContractOfRefWorker,
            DepartmentOfRefWorker
        );
    }

    public string AssigneeTypeAsString(AssigneeType type)
    {
        if (type == AssigneeType.ROLES)
        {
            return "Rolle";
        }
        if (type == AssigneeType.SUPERVISOR)
        {
            return "Vorgangsverantwortlicher";
        }
        if (type == AssigneeType.WORKER_OF_REF)
        {
            return "Bezugsperson";
        }

        return "Rolle";
    }
}
