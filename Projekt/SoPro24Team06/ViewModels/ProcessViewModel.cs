//-------------------------
// Author: Tamas Varadi
//-------------------------

using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class ProcessViewModel
{
    public int? Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public ProcessTemplate Template { get; set; }
    public List<Assignment> Assignments { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public ApplicationUser Supervisor { get; set; }
    public ApplicationUser WorkerOfReference { get; set; }
    public Contract ContractOfRefWorker { get; set; }
    public Department DepartmentOfRefWorker { get; set; }

    public ProcessViewModel(
        int? id,
        string? title,
        string? description,
        List<Assignment>? assignments
    )
    {
        this.Id = id;
        this.Title = title;
        this.Description = description ?? "";
        this.Assignments = assignments;
    }

    public ProcessViewModel(ProcessTemplate Template)
    {
        this.Title = Template.Title;
        this.Description = Template.Description;
        this.ContractOfRefWorker = Template.ContractOfRefWorker;
        this.DepartmentOfRefWorker = Template.DepartmentOfRefWorker;
        this.Template = Template;
    }

    public ProcessViewModel(Process process)
    {
        this.Id = process.Id;
        this.Title = process.Title;
        this.Description = process.Description ?? "";
        this.Assignments = process.Assignments;
        this.StartDate = process.StartDate;
        this.DueDate = DateTime.Now.AddDays(14);
        this.Supervisor = process.Supervisor;
        this.WorkerOfReference = process.WorkerOfReference;
        this.ContractOfRefWorker = process.ContractOfRefWorker;
        this.DepartmentOfRefWorker = process.DepartmentOfRefWorker;

        if (process.Assignments.Count > 0)
        {
            this.DueDate = process.Assignments.Max(a => a.DueDate);
        }
    }

    public ProcessViewModel()
    {
        this.Id = null;
        this.Title = "";
        this.Description = "";
        this.Assignments = new List<Assignment> { };
        this.StartDate = DateTime.Now;
        this.DueDate = DateTime.Now.AddDays(7);
        this.Supervisor = new ApplicationUser { FullName = "Vorgangsverantwortlicher" };
        this.WorkerOfReference = new ApplicationUser { FullName = "Benzugsperson" };
        this.ContractOfRefWorker = new Contract();
        this.DepartmentOfRefWorker = new Department();
    }

    public Process ToProcess()
    {
        return new Process(
            Title,
            Description,
            Assignments,
            WorkerOfReference,
            Supervisor,
            ContractOfRefWorker,
            DepartmentOfRefWorker
        );
    }

    public ComposeProcessViewModel ToComposeProcessViewModel()
    {
        return new ComposeProcessViewModel(this.Template);
    }
}
