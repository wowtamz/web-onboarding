using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class ComposeProcessViewModel
{
    public int? Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public ProcessTemplate Template { get; set; }
    public List<Assignment> Assignments { get; set; }

    public List<int> AssignmentTemplates { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public ApplicationUser Supervisor { get; set; }
    public ApplicationUser WorkerOfReference { get; set; }
    public Contract ContractOfRefWorker { get; set; }
    public Department DepartmentOfRefWorker { get; set; }


    public List<ApplicationUser> AvailableUsers { get; set; }
    public List<ProcessTemplate> AvailableProcessTemplates { get; set; }

    public List<AssignmentTemplate> AvailableAssignmentTemplates { get; set; }

    public List<Contract> AvailableContracts { get; set;}
    public List<Department> AvailableDepartments { get; set; }

    public ComposeProcessViewModel(ProcessTemplate? processTemplate)
    {
        if (processTemplate != null)
        {
            Process process = new Process(processTemplate);

            this.Title = process.Title;
            this.Description = process.Description;
            this.Template = processTemplate;
            this.Assignments = process.Assignments;
            this.AssignmentTemplates = new List<int> {};
            this.ContractOfRefWorker = process.ContractOfRefWorker;
            this.DepartmentOfRefWorker = process.DepartmentOfRefWorker;
            if (Assignments.Count() > 0) {
                this.DueDate = this.Assignments.Max(assignment => assignment.DueDate);
            } else {
                this.DueDate = DateTime.Now;
            }
        }
    }

    public ComposeProcessViewModel(Process process, List<ApplicationUser> availableUsers, List<ProcessTemplate> availableProcessTemplates, List<AssignmentTemplate> availableAssignmentTemplates, List<Contract> availableContracts, List<Department> availableDepartments)
    {
        if(process != null) {
            this.Id = process.Id;
            this.Title = process.Title;
            this.Description = process.Description ?? "";
            this.Assignments = process.Assignments;
            this.StartDate = process.StartDate;
            this.DueDate = process.Assignments.Max(a => a.DueDate);
            this.Supervisor = process.Supervisor;
            this.WorkerOfReference = process.WorkerOfReference;
            this.ContractOfRefWorker = process.ContractOfRefWorker;
            this.DepartmentOfRefWorker = process.DepartmentOfRefWorker;
        }
        this.AvailableUsers = availableUsers;
        this.AvailableProcessTemplates = availableProcessTemplates;
        this.AvailableAssignmentTemplates = availableAssignmentTemplates;
        this.AvailableContracts = availableContracts;
        this.AvailableDepartments = availableDepartments;
    }


    public ComposeProcessViewModel()
    {
        this.Id = null;
        this.Title = "";
        this.Description = "";
        this.Assignments = new List<Assignment> {};
        this.AssignmentTemplates = new List<int> {};
        this.StartDate = DateTime.Now;
        this.DueDate = DateTime.Now.AddDays(7);
        this.Supervisor = new ApplicationUser{ FullName = "Der Verantwortlicher"};
        this.WorkerOfReference = new ApplicationUser { FullName = "Der Bezugsperson"};
        this.ContractOfRefWorker = new Contract("Vollzeit");
        this.DepartmentOfRefWorker = new Department("Operations");

        this.AvailableUsers = new List<ApplicationUser> {};
        this.AvailableProcessTemplates = new List<ProcessTemplate>{};
        this.AvailableContracts = new List<Contract>{};
        this.AvailableDepartments = new List<Department>{};
    }

    public Process ToProcess()
    {
        return new Process(Title, Description, Assignments, WorkerOfReference, Supervisor, ContractOfRefWorker, DepartmentOfRefWorker);
    }

    public ProcessViewModel ToProcessViewModel()
    {
        ProcessViewModel processViewModel = new ProcessViewModel(this.Template);
        return processViewModel;
    }
}