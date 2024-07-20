//-------------------------
// Author: Tamas Varadi
//-------------------------

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Data;
using SoPro24Team06.Models;

namespace SoPro24Team06.Containers;

public class ProcessContainer
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ProcessContainer(ApplicationDbContext context)
    {
        _context = context;
    }

    public ProcessContainer(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager
    )
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // Alle Vorgänge lesen
    public async Task<List<Process>> GetProcessesAsync()
    {
        List<Process> processList = await _context
            .Processes.Include(p => p.WorkerOfReference)
            .Include(p => p.Supervisor)
            .Include(p => p.ContractOfRefWorker)
            .Include(p => p.DepartmentOfRefWorker)
            .Include(p => p.Assignments)
            .ToListAsync();
        return processList;
    }

    public async Task<List<Process>> GetActiveProcessesAsync()
    {
        List<Process> processList = await GetProcessesAsync();
        List<Process> activeProcesses = new List<Process> { };

        foreach (var process in processList)
        {
            if (!process.IsArchived)
            {
                activeProcesses.Add(process);
            }
        }

        return activeProcesses;
    }

    public async Task<List<Process>> GetArchivedProcessesAsync()
    {
        List<Process> processList = await GetProcessesAsync();
        List<Process> archivedProcesses = new List<Process> { };

        foreach (var process in processList)
        {
            if (process.IsArchived)
            {
                archivedProcesses.Add(process);
            }
        }

        return archivedProcesses;
    }

    // Vorgang per Id lesen
    public async Task<Process> GetProcessByIdAsync(int id)
    {
        Process process =
            _context
                .Processes.Include(p => p.WorkerOfReference)
                .Include(p => p.Supervisor)
                .Include(p => p.ContractOfRefWorker)
                .Include(p => p.DepartmentOfRefWorker)
                .Include(p => p.Assignments)
                .ThenInclude(a => a.Assignee)
                .Include(p => p.Assignments)
                .ThenInclude(a => a.AssignedRole)
                .Include(p => p.Assignments)
                .ThenInclude(a => a.Assignee)
                .Include(p => p.IsArchived)
                .ToList()
                .Find(p => p.Id == id)
            ?? throw new InvalidOperationException($"No Process found with Id {id}");
        return process;
    }

    // Alle Vorgänge die mit den Benutzer (userId) in Beziehung stehen
    public async Task<List<Process>> GetProcessesOfUserAsync(string userId)
    {
        List<Process> processList = new List<Process> { };

        List<Process> allProcesses = await _context
            .Processes.Include(p => p.Supervisor)
            .Include(p => p.WorkerOfReference)
            .Include(p => p.ContractOfRefWorker)
            .Include(p => p.DepartmentOfRefWorker)
            .Include(p => p.Assignments)
            .ToListAsync();

        ApplicationUser user = await _userManager.FindByIdAsync(userId);
        List<string> userRoles = new List<string>(await _userManager.GetRolesAsync(user));

        foreach (Process p in allProcesses)
        {
            if (p.Supervisor.Id == userId)
            {
                processList.Add(p);
                continue;
            }

            if (p.WorkerOfReference.Id == userId)
            {
                processList.Add(p);
                continue;
            }

            foreach (Assignment a in p.Assignments)
            {
                if (a.AssignedRole != null && userRoles.Find(r => r == a.AssignedRole.Name) != null)
                {
                    processList.Add(p);
                    continue;
                }

                // Nach dem Merge wieder hinzufügen
                /*
                if (a.Assignee != null && a.Assignee.Id == userId)
                {
                    processList.Add(p);
                    continue;
                }
                */
            }
        }

        return processList;
    }

    public async Task<List<Process>> GetActiveProcessesOfUserAsync(string userId)
    {
        List<Process> processList = await GetProcessesOfUserAsync(userId);
        List<Process> activeProcesses = new List<Process> { };

        foreach (Process p in processList)
        {
            if (!p.IsArchived)
            {
                activeProcesses.Add(p);
            }
        }

        return activeProcesses;
    }

    public async Task<List<Process>> GetArchivedProcessesOfUserAsync(string userId)
    {
        List<Process> processList = await GetProcessesOfUserAsync(userId);
        List<Process> archivedProcesses = new List<Process> { };

        foreach (Process p in processList)
        {
            if (p.IsArchived)
            {
                archivedProcesses.Add(p);
            }
        }

        return archivedProcesses;
    }

    // Alle Vorgänge die mit der Rolle (roleId) in Beziehung stehen
    public async Task<List<Process>> GetProcessesOfRoleAsync(string roleId)
    {
        List<Process> processList = new List<Process> { };

        List<Process> allProcesses = await _context
            .Processes.Include(p => p.Assignments)
            .ToListAsync();

        foreach (Process p in allProcesses)
        {
            foreach (Assignment a in p.Assignments)
            {
                if (a.AssignedRole != null && roleId == a.AssignedRole.Id)
                {
                    processList.Add(p);
                    continue;
                }
            }
        }

        return processList;
    }

    // Neue Vorgang hinzufügen
    public async Task<Process?> AddProcessAsync(Process processToAdd)
    {
        // Zuerst Assignments dem Context hinzufügen
        foreach (var assignment in processToAdd.Assignments)
        {
            ApplicationUser? assignee = await _userManager.FindByIdAsync(
                processToAdd.Supervisor.Id
            );
            ;
            if (assignment.Assignee != null)
            {
                assignee = await _userManager.FindByIdAsync(assignment.Assignee.Id);
            }
            else
            {
                /* später implementieren
                ApplicationRole role = assignment.AssignedRole ?? throw new NullReferenceException("Role cannot be null if there is no assignee");
                assignment.AssignedRole = await _roleManager.FindByIdAsync(role.Id);
                */
            }

            if (assignment.AssignedRole != null)
            {
                ApplicationRole assignedRole = await _roleManager.FindByIdAsync(
                    assignment.AssignedRole.Id
                );
                assignment.AssignedRole = assignedRole;
            }

            assignment.Assignee = assignee;

            _context.Assignments.Add(assignment);
        }

        var process = _context.Processes.Add(processToAdd);
        await _context.SaveChangesAsync();

        return process.Entity;
    }

    // Vorgang Bearbeiten
    public async Task UpdateProcessAsync(
        int id,
        string title,
        string description,
        List<Assignment> assignments,
        ApplicationUser supervisor,
        ApplicationUser workerOfRef,
        Contract contractOfRefWorker,
        Department departmentOfRefWorker
    )
    {
        Process processToUpdate =
            await _context
                .Processes.Include(p => p.Assignments)
                .FirstOrDefaultAsync(x => x.Id.Equals(id))
            ?? throw new InvalidOperationException($"No Process found with Id {id}");

        // Aufgaben die nicht mehr in der Liste enthalten sind finden und anschließend löschen
        List<string> newAssignmentTitles = assignments.Select(n => n.Title).ToList();
        List<Assignment> assignmentsToRemove = processToUpdate
            .Assignments.Where(a => !newAssignmentTitles.Contains(a.Title))
            .ToList();

        // Zuerst Assignments dem Context hinzufügen
        foreach (var assignment in assignments)
        {
            ApplicationUser? assignee = await _userManager.FindByIdAsync(supervisor.Id);
            ;
            if (assignment.Assignee != null)
            {
                assignee = await _userManager.FindByIdAsync(assignment.Assignee.Id);
            }
            else
            {
                /* später implementieren
                ApplicationRole role = assignment.AssignedRole ?? throw new NullReferenceException("Role cannot be null if there is no assignee");
                assignment.AssignedRole = await _roleManager.FindByIdAsync(role.Id);
                */
            }

            var assignmentExists = await _context.Assignments.FindAsync(assignment.Id);
            if (assignmentExists == null)
            {
                assignment.Assignee = assignee;
                _context.Assignments.Add(assignment);
            }
        }

        _context.Assignments.RemoveRange(assignmentsToRemove);

        processToUpdate.Title = title;
        processToUpdate.Description = description;
        processToUpdate.Assignments = assignments;
        processToUpdate.Supervisor = supervisor;
        processToUpdate.WorkerOfReference = workerOfRef;
        processToUpdate.ContractOfRefWorker = contractOfRefWorker;
        processToUpdate.DepartmentOfRefWorker = departmentOfRefWorker;

        // Änderungen speicher
        await _context.SaveChangesAsync();
    }

    // Vorgang anhalten und archivieren
    public async Task StopProcess(int id)
    {
        Process processToStop =
            await _context.Processes.FirstOrDefaultAsync(x => x.Id.Equals(id))
            ?? throw new InvalidOperationException($"No Process found with Id {id}");

        // Vorgang als Archiviert markieren
        processToStop.IsArchived = true;

        // Änderungen speicher
        await _context.SaveChangesAsync();
    }
}
