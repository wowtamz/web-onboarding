//-------------------------
// Author: Kevin Tornquist
//-------------------------

using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Data;
using SoPro24Team06.Models;

namespace SoPro24Team06.Containers;

public class ProcessTemplateContainer
{
    private readonly ApplicationDbContext _context;

    public ProcessTemplateContainer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProcessTemplate>> GetProcessTemplatesAsync()
    {
        return await GetProcessTemplatesWithIncludes().ToListAsync();
    }

    /// <summary>
    /// Retrieves a list of ProcessTemplates based on the access rights of the user.
    /// </summary>
    /// <param name="userName">The username of the user.</param>
    /// <returns>A list of ProcessTemplates accessible by the user.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the user or their role is not found.</exception>
    public async Task<List<ProcessTemplate>> GetProcessListByAccessRights(string userName)
    {
        // --- Start: Get the User and their Roles ---
        var user = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();

        if (user == null)
        {
            throw new InvalidOperationException($"No User found with UserName {userName}");
        }

        var userRoles = await _context
            .UserRoles.AsNoTracking()
            .Where(ur => ur.UserId == user.Id)
            .Select(x => x.RoleId)
            .ToListAsync();

        var roles = await _context
            .Roles.AsNoTracking()
            .Where(r => userRoles.Contains(r.Id))
            .Select(x => x.Name)
            .ToListAsync();

        // --- End: Get the User and their Roles ---

        if (roles.Count == 0)
        {
            return null;
        }

        if (roles.Contains("Administrator"))
        {
            return await GetProcessTemplatesAsync();
        }
        else
        {
            List<ProcessTemplate> pt = await GetProcessTemplatesWithIncludes().ToListAsync();

            return pt.Aggregate(
                new List<ProcessTemplate>(),
                (acc, x) =>
                {
                    if (x.RolesWithAccess.Any(y => roles.Contains(y.Name)))
                    {
                        acc.Add(x);
                    }
                    return acc;
                }
            );
        }
    }

    /// <summary>
    /// Retrieves a ProcessTemplate by its ID.
    /// </summary>
    /// <param name="id">The ID of the ProcessTemplate.</param>
    /// <returns>The ProcessTemplate with the specified ID.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no ProcessTemplate is found with the given ID.</exception>
    public async Task<ProcessTemplate> GetProcessTemplateByIdAsync(int id)
    {
        ProcessTemplate processTemplate =
            await GetProcessTemplatesWithIncludes().FirstOrDefaultAsync(pt => pt.Id == id)
            ?? throw new InvalidOperationException($"No Process found with Id {id}");
        return processTemplate;
    }

    /// <summary>
    /// Adds a new ProcessTemplate to the database.
    /// </summary>
    /// <param name="processTemplateToAdd">The ProcessTemplate to add.</param>
    /// <returns>The added ProcessTemplate.</returns>
    public async Task<ProcessTemplate> AddProcessTemplateAsync(ProcessTemplate processTemplateToAdd)
    {
        var pt = _context.ProcessTemplates.Add(processTemplateToAdd);

        await _context.SaveChangesAsync();

        return pt.Entity;
    }

    /// <summary>
    /// Updates an existing ProcessTemplate in the database.
    /// </summary>
    /// <param name="processTemplate">The ProcessTemplate to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no ProcessTemplate is found with the given ID.</exception>
    public async Task UpdateProcessTemplateAsync(ProcessTemplate processTemplate)
    {
        var processTemplateToUpdate =
            await _context
                .ProcessTemplates.Include(pt => pt.AssignmentTemplates)
                .Include(pt => pt.RolesWithAccess)
                .FirstOrDefaultAsync(x => x.Id == processTemplate.Id)
            ?? throw new InvalidOperationException(
                $"No Process found with Id {processTemplate.Id}"
            );

        processTemplateToUpdate.Title = processTemplate.Title;
        processTemplateToUpdate.Description = processTemplate.Description;
        processTemplateToUpdate.DepartmentOfRefWorker = processTemplate.DepartmentOfRefWorker;
        processTemplateToUpdate.ContractOfRefWorker = processTemplate.ContractOfRefWorker;
        processTemplateToUpdate.RolesWithAccess = processTemplate.RolesWithAccess;
        processTemplateToUpdate.AssignmentTemplates = processTemplate.AssignmentTemplates;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a ProcessTemplate from the database.
    /// </summary>
    /// <param name="id">The ID of the ProcessTemplate to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no ProcessTemplate is found with the given ID.</exception>
    public async Task DeleteProcessTemplateAsync(int id)
    {
        ProcessTemplate processTemplate =
            await _context.ProcessTemplates.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException($"No Process found with Id {id}");
        _context.ProcessTemplates.Remove(processTemplate);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a queryable collection of ProcessTemplates with related data included.
    /// </summary>
    /// <returns>An IQueryable of ProcessTemplates with related data.</returns>
    private IQueryable<ProcessTemplate> GetProcessTemplatesWithIncludes()
    {
        return _context
            .ProcessTemplates.Include(pt => pt.DepartmentOfRefWorker)
            .Include(pt => pt.AssignmentTemplates)
            .ThenInclude(a => a.DueIn)
            .Include(pt => pt.AssignmentTemplates)
            .ThenInclude(a => a.ForDepartmentsList)
            .Include(pt => pt.AssignmentTemplates)
            .ThenInclude(a => a.ForContractsList)
            .Include(pt => pt.AssignmentTemplates)
            .ThenInclude(a => a.AssignedRole)
            .Include(pt => pt.ContractOfRefWorker)
            .Include(pt => pt.RolesWithAccess);
    }
}
