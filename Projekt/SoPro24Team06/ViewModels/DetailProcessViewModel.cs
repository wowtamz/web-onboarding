//-------------------------
// Author: Tamas Varadi
//-------------------------

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoPro24Team06.Data;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class DetailProcessViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<Assignment> Assignments { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public ApplicationUser Supervisor { get; set; }
    public ApplicationUser WorkerOfReference { get; set; }
    public Contract ContractOfRefWorker { get; set; }
    public Department DepartmentOfRefWorker { get; set; }

    public DetailProcessViewModel(Process process)
    {
        this.Id = process.Id;
        this.Title = process.Title;
        this.Description = process.Description;
        this.StartDate = process.StartDate;
        this.DueDate =
            process.DueDate
            ?? throw new ArgumentException("Must have a value", nameof(process.DueDate));
        this.Assignments = process.Assignments;
        this.Supervisor = process.Supervisor;
        this.WorkerOfReference = process.WorkerOfReference;
        this.ContractOfRefWorker = process.ContractOfRefWorker;
        this.DepartmentOfRefWorker = process.DepartmentOfRefWorker;
    }

    public Process ToProcess()
    {
        return new Process(
            Title,
            Description,
            new List<Assignment> { },
            WorkerOfReference,
            Supervisor,
            ContractOfRefWorker,
            DepartmentOfRefWorker
        );
    }
}
