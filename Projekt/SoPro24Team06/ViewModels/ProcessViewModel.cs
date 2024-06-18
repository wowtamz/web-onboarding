using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class ProcessViewModel 
{
    public int? Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<Assignment> Assignments { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public User Supervisor { get; set; }
    public User WorkerOfReference { get; set; }
    public Contract ContractOfRefWorker { get; set; }
    public Department DepartmentOfRefWorker { get; set; }

    public ProcessViewModel(Process process) 
    {
        this.Id = process.Id;
        this.Title = process.Title;
        this.Description = process.Description;
        this.Assignments = process.Assignments;
        this.StartDate = process.StartDate;
        this.DueDate = process.DueDate;
        this.Supervisor = process.Supervisor;
        this.WorkerOfReference = process.WorkerOfReference;
        this.ContractOfRefWorker = process.ContractOfRefWorker;
        this.DepartmentOfRefWorker = process.DepartmentOfRefWorker;
    }
}