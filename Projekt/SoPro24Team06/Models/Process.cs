using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SoPro24Team06.Models
{
    public class Process
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public string? Description { get; set; }

        public DateTime StartDate { get; }

        [Required(ErrorMessage = "DueDate is required")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Worker of reference is required")]
        public User WorkerOfReference { get; set; }

        [Required(ErrorMessage = "Supervisor is required")]
        public User Supervisor { get; set; }

        [Required(ErrorMessage = "At least one assignment is required")]
        public List<Assignment> Assignments { get; set; }

        [Required(ErrorMessage = "Contract is required")]
        public Contract ContractOfRefWorker { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public Department DepartmentOfRefWorker { get; set; }

        public Process(ProcessTemplate Template, User WorkerOfReference, User Supervisor, DateTime DueDate)
        {
            this = Template.ToProcess();
            /*
            this.Name = Template.Name;
            this.Description = Template.Description;
            this.Assignments = Template.Assignments.ConvertAll(x => x.ToAssignment());
            this.ContractOfRefWorker = Template.ContractOfRefWorker;
            this.DepartmentOfRefWorker = Template.DepartmentOfRefWorker;
            */
            this.WorkerOfReference = WorkerOfReference;
            this.Supervisor = Supervisor;
            this.DueDate = DueDate;
        }
    }
}