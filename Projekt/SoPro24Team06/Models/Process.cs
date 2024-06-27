using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SoPro24Team06.Models
{
    public class Process
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; }

        [Required(ErrorMessage = "DueDate is required")]
        [JsonProperty("dueDate")]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Worker of reference is required")]
        [JsonProperty("workerOfReference")]
        public ApplicationUser WorkerOfReference { get; set; }

        [Required(ErrorMessage = "Supervisor is required")]
        [JsonProperty("supervisor")]
        public ApplicationUser Supervisor { get; set; }

        [Required(ErrorMessage = "At least one assignment is required")]
        [JsonProperty("assignments")]
        public List<Assignment> Assignments { get; set; }

        [Required(ErrorMessage = "Contract is required")]
        [JsonProperty("contractOfRefWorker")]
        public Contract ContractOfRefWorker { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [JsonProperty("departmentOfRefWorker")]
        public Department DepartmentOfRefWorker { get; set; }

        public Process(ProcessTemplate Template)
        {
            this.Title = Template.Title;
            this.Description = Template.Description;
            this.Assignments = Template.AssignmentTemplates.ConvertAll(template =>
                template.ToAssignment(template)
            );
            this.ContractOfRefWorker = Template.ContractOfRefWorker;
            this.DepartmentOfRefWorker = Template.DepartmentOfRefWorker;
            if (Assignments.Count() > 0) {
                this.DueDate = this.Assignments.Max(assignment => assignment.DueDate);
            } else {
                this.DueDate = DateTime.Now;
            }
        }

        public Process(
            string title,
            string description,
            List<Assignment> assignments,
            ApplicationUser workerOfReference,
            ApplicationUser supervisor,
            Contract contractOfRefWorker,
            Department departmentOfRefWorker
        )
        {
            this.Title = title;
            this.Description = description;
            this.Assignments = assignments;
            this.WorkerOfReference = workerOfReference;
            this.Supervisor = supervisor;
            this.ContractOfRefWorker = contractOfRefWorker;
            this.DepartmentOfRefWorker = departmentOfRefWorker;
            this.StartDate = DateTime.Now;
            if (Assignments.Count() > 0) {
                this.DueDate = this.Assignments.Max(assignment => assignment.DueDate);
            } else {
                this.DueDate = DateTime.Now;
            }
        }

        public Process()
        {
            this.Title = "";
            this.Description = "";
            this.Assignments = new List<Assignment> { };
            this.WorkerOfReference = new ApplicationUser { };
            this.Supervisor = new ApplicationUser();
            this.ContractOfRefWorker = new Contract();
            this.DepartmentOfRefWorker = new Department();
            this.StartDate = DateTime.Now;
            this.DueDate = DateTime.Now.AddDays(14);
        }
    }
}
