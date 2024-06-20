using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoPro24Team06.Models
{
    public class Process
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime StartDate { get; }

        [Required(ErrorMessage = "DueDate is required")]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Worker of reference is required")]
        public ApplicationUser WorkerOfReference { get; set; }

        [Required(ErrorMessage = "Supervisor is required")]
        public ApplicationUser Supervisor { get; set; }

        [Required(ErrorMessage = "At least one assignment is required")]
        public List<Assignment> Assignments { get; set; }

        [Required(ErrorMessage = "Contract is required")]
        public Contract ContractOfRefWorker { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public Department DepartmentOfRefWorker { get; set; }

        public Process(ProcessTemplate Template)
        {
            this.Title = Template.Title;
            this.Description = Template.Description;
            this.Assignments = Template.AssignmentTemplates.ConvertAll(template => template.ToAssignment(template));
            this.ContractOfRefWorker = Template.ContractOfRefWorker;
            this.DepartmentOfRefWorker = Template.DepartmentOfRefWorker;
            this.DueDate = this.Assignments.Max(assignment => assignment.DueDate);
        }

        public Process(string title, string description, List<Assignment> assignments, ApplicationUser workerOfReference, ApplicationUser supervisor, Contract contractOfRefWorker, Department departmentOfRefWorker)
        {
            this.Title = title;
            this.Description = description;
            this.Assignments = assignments;
            this.WorkerOfReference = workerOfReference;
            this.Supervisor = supervisor;
            this.ContractOfRefWorker = contractOfRefWorker;
            this.DepartmentOfRefWorker = departmentOfRefWorker;
            this.StartDate = DateTime.Now;
            this.DueDate = this.Assignments.Max(assignment => assignment.DueDate);
        }

        public Process()
        {
            this.Title = "";
            this.Description = "";
            this.Assignments = new List<Assignment>{};
            this.WorkerOfReference = new ApplicationUser { FullName = "Bob"};
            this.Supervisor = new ApplicationUser{ FullName = "Jenny"};
            this.ContractOfRefWorker = new Contract("None");
            this.DepartmentOfRefWorker = new Department("None");
            this.StartDate = DateTime.Now;
            this.DueDate = DateTime.Now.AddDays(14);
        }
    }
}