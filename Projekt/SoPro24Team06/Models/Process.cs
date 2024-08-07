//-------------------------
// Author: Tamas Varadi
//-------------------------

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SoPro24Team06.Container;
using SoPro24Team06.Enums;

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
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "DueDate is required")]
        [JsonProperty("dueDate")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Worker of reference is required")]
        [JsonProperty("workerOfReference")]
        public ApplicationUser WorkerOfReference { get; set; }

        [Required(ErrorMessage = "Supervisor is required")]
        [JsonProperty("supervisor")]
        public ApplicationUser Supervisor { get; set; }

        [Required(ErrorMessage = "At least one assignment is required")]
        [JsonProperty("assignments")]
        public List<Assignment> Assignments { get; set; }

        [Required(ErrorMessage = "Vertragstyp ist erforderlich")]
        [JsonProperty("contractOfRefWorker")]
        public Contract ContractOfRefWorker { get; set; }

        [Required(ErrorMessage = "Abteilung ist erforderlich")]
        [JsonProperty("departmentOfRefWorker")]
        public Department DepartmentOfRefWorker { get; set; }

        [JsonProperty("isArchived")]
        public bool IsArchived { get; set; }

        /// <summary>
        /// Creates new process using a process template as base
        /// </summary>
        /// <param name="Template"></param>
        public Process(ProcessTemplate Template)
        {
            this.Title = Template.Title;
            this.Description = Template.Description;
            List<Assignment> assignments = new List<Assignment>();
            //for uncomment for procuction use
            // List<AssignmentTemplate> assignmentTemplates = Template.AssignmentTemplates.Where
            // (
            // 	temp =>
            // 	(
            // 		temp.ForContractsList == null || temp.ForContractsList.Contains
            // 		(
            // 			this.ContractOfRefWorker
            // 		) && (
            // 			temp.ForDepartmentsList == null || temp.ForDepartmentsList.Contains
            // 			(
            // 				this.DepartmentOfRefWorker
            // 			)
            // 		)
            // 	)
            // ).ToList();
            // foreach (AssignmentTemplate temp in assignmentTemplates)
            Assignments = new List<Assignment>(); //delete for production use
            foreach (AssignmentTemplate temp in Template.AssignmentTemplates) //delete for production use
            {
                Assignment assignment = temp.ToAssignment(null, this.DueDate); //change
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
                assignments.Add(assignment);
            }
            this.ContractOfRefWorker = Template.ContractOfRefWorker;
            this.DepartmentOfRefWorker = Template.DepartmentOfRefWorker;
            if (Assignments.Count() > 0)
            {
                this.DueDate = this.Assignments.Max(assignment => assignment.DueDate);
            }
            else
            {
                this.DueDate = DateTime.Now;
            }
        }
        
        /// <summary>
        /// Default contructor a process
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="assignments"></param>
        /// <param name="workerOfReference"></param>
        /// <param name="supervisor"></param>
        /// <param name="contractOfRefWorker"></param>
        /// <param name="departmentOfRefWorker"></param>
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
            if (Assignments.Count() > 0)
            {
                this.DueDate = this.Assignments.Max(assignment => assignment.DueDate);
            }
            else
            {
                this.DueDate = DateTime.Now;
            }
        }

        /// <summary>
        /// Empty contructor for process
        /// </summary>
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
