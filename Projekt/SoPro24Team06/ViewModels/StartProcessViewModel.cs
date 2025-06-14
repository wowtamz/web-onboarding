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

public class StartProcessViewModel
{
    public int? Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    [Required(ErrorMessage = "Prozess ist erforderlich")]
    public ProcessTemplate Template { get; set; }

    [Required(ErrorMessage = "Mindestens eine Aufgaben muss hinzugefügt werden")]
    [MinLength(1, ErrorMessage = "Mindestens eine Aufgaben muss hinzugefügt werden")]
    public List<AssignmentTemplate> AssignmentTemplates { get; set; }
    public DateTime? StartDate { get; set; }
    
    [Required(ErrorMessage = "Zieldatum ist erforderlich")]
    public DateTime DueDate { get; set; }

    [Required(ErrorMessage = "Vorgangsverantwortlicher ist erforderlich")]
    public ApplicationUser Supervisor { get; set; }

    [Required(ErrorMessage = "Bezugsperson ist erforderlich")]
    public ApplicationUser WorkerOfReference { get; set; }

    [Required(ErrorMessage = "Vertragsart ist erforderlich")]
    public Contract ContractOfRefWorker { get; set; }

    [Required(ErrorMessage = "Abteilung ist erforderlich")]
    public Department DepartmentOfRefWorker { get; set; }

    /// <summary>
    /// Create new StartProcessViewModel from
    /// process
    /// </summary>
    /// <param name="process"></param>
    public StartProcessViewModel(Process process)
    {
        this.Title = process.Title;
        this.Description = process.Description;
        this.StartDate = process.StartDate;
        this.DueDate = process.DueDate;
        this.Supervisor = process.Supervisor;
        this.WorkerOfReference = process.WorkerOfReference;
        this.ContractOfRefWorker = process.ContractOfRefWorker;
        this.DepartmentOfRefWorker = process.DepartmentOfRefWorker;
    }

    /// <summary>
    /// Create new StartProcessViewModel from
    /// ProcessTemplate
    /// </summary>
    /// <param name="process"></param>
    public StartProcessViewModel(ProcessTemplate? processTemplate)
    {
        if (processTemplate != null)
        {
            Process process = new Process(processTemplate);

            this.Title = process.Title;
            this.Description = process.Description;
            this.Template = processTemplate;
            this.AssignmentTemplates = new List<AssignmentTemplate> { };
            this.ContractOfRefWorker = process.ContractOfRefWorker;
            this.DepartmentOfRefWorker = process.DepartmentOfRefWorker;
        }
    }

    /// <summary>
    /// Create new emptyStartProcessViewModel
    /// </summary>
    /// <param name="process"></param>
    public StartProcessViewModel()
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
    
    /// <summary>
    /// Converts the StartProcessViewModel to a Process
    /// </summary>
    /// <returns>Process with attributes of StartProcessViewModel</returns>
    public Process ToProcess()
    {
        List<Assignment> assignmentList = new List<Assignment> { };
        //beginn codeownership Jan Pfluger

        foreach (AssignmentTemplate temp in AssignmentTemplates) //delete for production use
        {
            Assignment assignment = temp.ToAssignment(null, this.DueDate); //change !!!!
            switch (temp.AssigneeType)
            {
                case AssigneeType.SUPERVISOR:
                    assignment.Assignee = this.Supervisor;
                    break;
                case AssigneeType.WORKER_OF_REF:
                    assignment.Assignee = this.WorkerOfReference;
                    break;
                default:
                    break;
            }
            
            assignmentList.Add(assignment);
        }

        return new Process(
            Title,
            Description,
            assignmentList,
            WorkerOfReference,
            Supervisor,
            ContractOfRefWorker,
            DepartmentOfRefWorker
        );
    }

    /// <summary>
    /// Converts StartProcessViewModel to a ProcessViewModel
    /// </summary>
    /// <returns></returns>
    public ProcessViewModel ToProcessViewModel()
    {
        ProcessViewModel processViewModel = new ProcessViewModel(this.Template);
        return processViewModel;
    }

    /// <summary>
    /// Converts AssigneeType to a string literal
    /// </summary>
    /// <returns></returns>
    /// 
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
