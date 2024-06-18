using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;

public class ProcessContainer 
{
    private readonly context;

    public async Task<List<Process>> GetProcessesAsync()
    {
        List<Process> processList = await context.Processes.ToListAsync();
        return processList;
    }

    public async Task<Process> GetProcessByIdAsync(int id)
    {
        Process process = await context.Processes.FindAsync(id) ?? throw new InvalidOperationException($"No Process found with Id {id}");
        return process;
    }

    public async Task AddProcessAsync(ProcessTemplate template, User supervisor, User workerOfRef, Contract contractOfRefWorker, Department departmentOfRefWorker)
    {
        Process newProcess = new Process(template, supervisor, workerOfRef, contractOfRefWorker, departmentOfRefWorker);
        context.Processes.Add(newProcess);
        await context.SaveChangesAsync();
    }

    public async Task UpdateProcessAsync(int id, string title, string description, List<Assignment> assignments,  User supervisor, User workerOfRef, Contract contractOfRefWorker, Department departmentOfRefWorker)
    {
        Process processToUpdate = await context.Processes.FirstOrDefaultAsync(x => x.Id.Equals(id)) ?? throw new InvalidOperationException($"No Process found with Id {id}");

        processToUpdate.Title = title;
        processToUpdate.Description = description;
        processToUpdate.Assignments = assignments;
        processToUpdate.Supervisor = supervisor;
        processToUpdate.WorkerOfReference = workerOfRef;
        processToUpdate.ContractOfRefWorker = contractOfRefWorker;
        processToUpdate.DepartmentOfRefWorker = departmentOfRefWorker;

        await context.SaveChangesAsync():
    }

    public async Task DeleteProcessAsync(int id)
    {
        Process process = await context.Processes.FirstOrDefaultAsync(x => x.Id == id);
        context.Processes.Remove(process);
        await context.SaveChangesAsync();
    }
}