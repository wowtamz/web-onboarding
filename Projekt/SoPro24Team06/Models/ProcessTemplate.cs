using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace SoPro24Team06.Models;
public class Process
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; }

    [Required(ErrorMessage = "Title ist erforderlich")]
    public string Title { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Mindestens eine Aufgabe ist erforderlich")]
    public List<AssignmentTemplate> AssignmentTemplates { get; set; }

    [Required(ErrorMessage = "Vertrag ist erforderlich")]
    public Contract ContractOfRefWorker

    [Required(ErrorMessage = "Abteilung ist erforderlich")]
    public Department DepartmentOfRefWorker

    public Process(string title, string description, List<AssignmentTemplate> assignmentTemplates, Contract contractOfRefWorker, Department departmentOfRefWorker)
    {
        this.Title = title;
        this.Description = description;
        this.AssignmentTemplates = assignmentTemplates;
        this.ContractOfRefWorker = contractOfRefWorker;
        this.DepartmentOfRefWorker = departmentOfRefWorker;
    }
}