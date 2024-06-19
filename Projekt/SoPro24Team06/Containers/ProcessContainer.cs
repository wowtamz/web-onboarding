using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Data;
using SoPro24Team06.Models;


namespace SoPro24Team06.Containers;

public class ProcessContainer
{
    private readonly ModelContext _context;

    public ProcessContainer(ModelContext context)
    {
        _context = context;
    }

    public async Task<List<Process>> GetProcessesAsync()
    {
        List<Process> processList = await _context.Processes.ToListAsync();
        return processList;
    }

    public async Task<Process> GetProcessByIdAsync(int id)
    {
        Process process = await _context.Processes.FindAsync(id) ?? throw new InvalidOperationException($"No Process found with Id {id}");
        return process;
    }

    public async Task AddProcessAsync(Process processToAdd)
    {
        _context.Processes.Add(processToAdd);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProcessAsync(int id, string title, string description, List<Assignment> assignments, ApplicationUser supervisor, ApplicationUser workerOfRef, Contract contractOfRefWorker, Department departmentOfRefWorker)
    {
        Process processToUpdate = await _context.Processes.FirstOrDefaultAsync(x => x.Id.Equals(id)) ?? throw new InvalidOperationException($"No Process found with Id {id}");

        processToUpdate.Title = title;
        processToUpdate.Description = description;
        processToUpdate.Assignments = assignments;
        processToUpdate.Supervisor = supervisor;
        processToUpdate.WorkerOfReference = workerOfRef;
        processToUpdate.ContractOfRefWorker = contractOfRefWorker;
        processToUpdate.DepartmentOfRefWorker = departmentOfRefWorker;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteProcessAsync(int id)
    {
        Process process = await _context.Processes.FirstOrDefaultAsync(x => x.Id == id);
        _context.Processes.Remove(process);
        await _context.SaveChangesAsync();
    }
}