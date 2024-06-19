using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoPro24Team06.Models
{
    public class Process
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; }
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

        public Process(ProcessTemplate Template, ApplicationUser? WorkerOfReference, ApplicationUser? Supervisor, Contract contractOfRefWorker, Department departmentOfRefWorker)
        {
            this.Title = Template.Title;
            this.Description = Template.Description;
            this.Assignments = Template.AssignmentTemplates.ConvertAll(template => template.ToAssignment(template));
            this.ContractOfRefWorker = Template.ContractOfRefWorker;
            this.DepartmentOfRefWorker = Template.DepartmentOfRefWorker;
            this.WorkerOfReference = WorkerOfReference;
            this.Supervisor = Supervisor;
            this.DueDate = this.Assignments.Max(assignment => assignment.DueDate);
            this.DepartmentOfRefWorker = departmentOfRefWorker;
            this.ContractOfRefWorker = contractOfRefWorker;
        }
    }
}