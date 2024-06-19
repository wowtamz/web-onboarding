using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class ProcessTemplateViewModel
{
    public int? Id { get; }

    public string Title { get; set; }

    public string? Description { get; set; }

    public ApplicationUser WorkerOfReference { get; set; }

    public List<AssignmentTemplate> AssignmentTemplates { get; set; }

    public Contract ContractOfRefWorker { get; set; }

    public Department DepartmentOfRefWorker { get; set; }

    public ProcessTemplateViewModel(string title, string description, ApplicationUser workerOfReference, List<AssignmentTemplate> assignmentTemplates, Contract contractOfRefWorker, Department departmentOfRefWorker)
    {
        this.Title = title;
        this.Description = description;
        this.WorkerOfReference = workerOfReference;
        this.AssignmentTemplates = assignmentTemplates;
        this.ContractOfRefWorker = contractOfRefWorker;
        this.DepartmentOfRefWorker = departmentOfRefWorker;
    }
}