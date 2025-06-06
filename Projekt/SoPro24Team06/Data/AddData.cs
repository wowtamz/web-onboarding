using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;

//-------------------------
// Author: Michael Adolf
//-------------------------

namespace SoPro24Team06.Data
{
    public static class AddData
    {
        public static async Task Initialize(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            string jsonString
        )
        {
            JsonElement importData;

            try
            {
                importData = JsonSerializer.Deserialize<JsonElement>(jsonString);
                if (importData.ValueKind != JsonValueKind.Object)
                {
                    throw new Exception(
                        "Deserialization failed. Import data is not a valid JSON object."
                    );
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to deserialize JSON string.", ex);
            }

            // Seed roles
            context.Database.EnsureDeleted();
            context.Database.Migrate();
            var keyRingPath = Path.Combine(Directory.GetCurrentDirectory(), "keys");

            // Vorhandene Schlüssel löschen und Ordner neu erstellen
            if (Directory.Exists(keyRingPath))
            {
                Directory.Delete(keyRingPath, true);
            }
            Directory.CreateDirectory(keyRingPath);
            try
            {
                if (
                    importData.TryGetProperty("Roles", out var rolesElement)
                    && rolesElement.ValueKind == JsonValueKind.Array
                )
                {
                    foreach (var roleElement in rolesElement.EnumerateArray())
                    {
                        var roleName = roleElement.GetString();
                        if (!string.IsNullOrEmpty(roleName))
                        {
                            var roleExist = await roleManager.RoleExistsAsync(roleName);
                            if (!roleExist)
                            {
                                await roleManager.CreateAsync(new ApplicationRole(roleName));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to seed roles.", ex);
            }

            // Seed users
            try
            {
                if (
                    importData.TryGetProperty("Users", out var usersElement)
                    && usersElement.ValueKind == JsonValueKind.Array
                )
                {
                    foreach (var userElement in usersElement.EnumerateArray())
                    {
                        var fullName = userElement.GetProperty("FullName").GetString();
                        var email = userElement.GetProperty("Email").GetString();
                        var password = userElement.GetProperty("Password").GetString();
                        var roles = userElement
                            .GetProperty("Roles")
                            .EnumerateArray()
                            .Select(r => r.GetString())
                            .ToList();
                        var locked = userElement.GetProperty("Locked").GetBoolean();

                        var user = await userManager.FindByEmailAsync(email);
                        if (user == null)
                        {
                            user = new ApplicationUser
                            {
                                UserName = email,
                                FullName = fullName,
                                Email = email,
                                LockoutEnabled = locked,
                                LockoutEnd = locked
                                    ? DateTimeOffset.MaxValue
                                    : (DateTimeOffset?)null
                            };
                            var result = await userManager.CreateAsync(user, password);
                            if (result.Succeeded)
                            {
                                foreach (var role in roles)
                                {
                                    if (role != null)
                                    {
                                        await userManager.AddToRoleAsync(user, role);
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception(
                                    $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to seed users.", ex);
            }

            // Seed Departments
            if (
                importData.TryGetProperty("Departments", out var departmentElements)
                && departmentElements.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var departmentElement in departmentElements.EnumerateArray())
                {
                    var department = new Department
                    {
                        Id = departmentElement.GetProperty("Id").GetInt32(),
                        Name = departmentElement.GetProperty("Name").GetString(),
                    };
                    context.Departments.Add(department);
                }
                await context.SaveChangesAsync();
            }

            // Seed Contracts
            if (
                importData.TryGetProperty("Contracts", out var contractElements)
                && contractElements.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var contractElement in contractElements.EnumerateArray())
                {
                    var contract = new Contract
                    {
                        Id = contractElement.GetProperty("Id").GetInt32(),
                        Label = contractElement.GetProperty("Label").GetString(),
                    };
                    context.Contracts.Add(contract);
                }
                await context.SaveChangesAsync();
            }

            // Seed due times

            if (
                importData.TryGetProperty("DueTimes", out var dueTimesElement)
                && dueTimesElement.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var dueTimeElement in dueTimesElement.EnumerateArray())
                {
                    var dueTime = new DueTime
                    {
                        Id = dueTimeElement.GetProperty("Id").GetInt32(),
                        Label = dueTimeElement.GetProperty("Label").GetString(),
                        Days = dueTimeElement.GetProperty("Days").GetInt32(),
                        Weeks = dueTimeElement.GetProperty("Weeks").GetInt32(),
                        Months = dueTimeElement.GetProperty("Months").GetInt32()
                    };
                    context.DueTimes.Add(dueTime);
                }
                await context.SaveChangesAsync();
            }

            try
            {
                if (
                    importData.TryGetProperty("ProcessTemplates", out var processTemplatesElement)
                    && processTemplatesElement.ValueKind == JsonValueKind.Array
                )
                {
                    Console.WriteLine("Importing process templates...");
                    foreach (var templateElement in processTemplatesElement.EnumerateArray())
                    {
                        var rolesWithAccess = templateElement
                            .GetProperty("RolesWithAccess")
                            .EnumerateArray()
                            .Select(roleName =>
                                roleManager.FindByNameAsync(roleName.GetString()).Result
                            )
                            .Where(role => role != null)
                            .ToList();

                        var processTemplate = new ProcessTemplate
                        {
                            Title = templateElement.GetProperty("Title").GetString(),
                            Description = templateElement.GetProperty("Description").GetString(),
                            RolesWithAccess = rolesWithAccess,
                            ContractOfRefWorker = context.Contracts.FirstOrDefault(x =>
                                x.Id
                                == templateElement.GetProperty("ContractOfRefWorkerId").GetInt32()
                            ),
                            DepartmentOfRefWorker = context.Departments.FirstOrDefault(x =>
                                x.Id
                                == templateElement.GetProperty("DepartmentOfRefWorkerId").GetInt32()
                            ),
                            AssignmentTemplates = templateElement
                                .GetProperty("AssignmentTemplates")
                                .EnumerateArray()
                                .Select(a => new AssignmentTemplate
                                {
                                    Title = a.GetProperty("Title").GetString(),
                                    Instructions = a.GetProperty("Instructions").GetString(),
                                    AssigneeType = (AssigneeType)
                                        a.GetProperty("AssigneeType").GetInt32(),
                                    DueIn = context.DueTimes.FirstOrDefault(d =>
                                        d.Id == a.GetProperty("DueInId").GetInt32()
                                    ),
                                    AssignedRole = roleManager.Roles.FirstOrDefault(r =>
                                        r.Name == a.GetProperty("AssignedRole").GetString()
                                    ),
                                    ForContractsList = a.TryGetProperty(
                                        "Contracts",
                                        out var contractsElement
                                    )
                                        ? contractsElement
                                            .EnumerateArray()
                                            .Select(c =>
                                                context.Contracts.FirstOrDefault(con =>
                                                    con.Id == c.GetProperty("ContractId").GetInt32()
                                                )
                                            )
                                            .ToList()
                                        : new List<Contract>(),
                                    ForDepartmentsList = a.TryGetProperty(
                                        "Departments",
                                        out var departmentsElement
                                    )
                                        ? departmentsElement
                                            .EnumerateArray()
                                            .Select(d =>
                                                context.Departments.FirstOrDefault(dep =>
                                                    dep.Id
                                                    == d.GetProperty("DepartmentId").GetInt32()
                                                )
                                            )
                                            .ToList()
                                        : new List<Department>()
                                })
                                .ToList()
                        };

                        foreach (var at in processTemplate.AssignmentTemplates)
                        {
                            context.AssignmentTemplates.Add(at);
                        }

                        context.ProcessTemplates.Add(processTemplate);
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to seed process templates.", ex);
            }

            //seed processes
            try
            {
                if (
                    importData.TryGetProperty("Processes", out var processElement)
                    && processElement.ValueKind == JsonValueKind.Array
                )
                    Console.WriteLine("Importing process...");
                {
                    foreach (var p in processElement.EnumerateArray())
                    {
                        string title = p.GetProperty("Title").GetString();
                        string description = p.GetProperty("Description").GetString();
                        string supervisorEmail = p.GetProperty("Supervisor").GetString();
                        string workerOfReferenceEmail = p.GetProperty("WorkerOfReference")
                            .GetString();
                        int DepartmentOfRefWorkerId = p.GetProperty("Department").GetInt32();
                        int contractOfRefWorkerId = p.GetProperty("ContractOfRefWorker").GetInt32();
                        DateTime startDate = DateTime.ParseExact(
                            p.GetProperty("StartDate").GetString(),
                            "dd-MM-yyyy",
                            null
                        );
                        DateTime dueDate = DateTime.ParseExact(
                            p.GetProperty("DueDate").GetString(),
                            "dd-MM-yyyy",
                            null
                        );

                        bool isArchived = p.GetProperty("isArchived").GetBoolean();
                        var workerOfReference = await userManager.FindByEmailAsync(
                            workerOfReferenceEmail
                        );
                        var supervisor = await userManager.FindByEmailAsync(supervisorEmail);
                        List<Assignment> assignments = new List<Assignment>();
                        foreach (var a in p.GetProperty("Assignments").EnumerateArray())
                        {
                            string assignmentTitle = a.GetProperty("Title").GetString();
                            string instructions = a.GetProperty("Instructions").GetString();
                            AssigneeType assigneeType = (AssigneeType)
                                a.GetProperty("AssigneeType").GetInt32();
                            int dueInId = a.GetProperty("DueInId").GetInt32();
                            string assignedRoleString = a.GetProperty("AssignedRole").GetString();
                            AssignmentStatus status = (AssignmentStatus)
                                a.GetProperty("Status").GetInt32();
                            ApplicationUser? assignee = null;
                            ApplicationRole? assignedRole = null;

                            var contractOfRefWorker = context.Contracts.FirstOrDefault(c =>
                                c.Id == contractOfRefWorkerId
                            );
                            var departmentOfRefWorker = context.Departments.FirstOrDefault(d =>
                                d.Id == DepartmentOfRefWorkerId
                            );

                            switch (assigneeType)
                            {
                                case AssigneeType.ROLES:
                                    assignedRole = roleManager.Roles.FirstOrDefault(r =>
                                        r.Name == assignedRoleString
                                    );
                                    break;
                                case AssigneeType.SUPERVISOR:
                                    assignee = supervisor;
                                    assigneeType = AssigneeType.USER;
                                    break;
                                case AssigneeType.WORKER_OF_REF:
                                    assignee = workerOfReference;
                                    assigneeType = AssigneeType.USER;
                                    break;
                            }
                            var dueIn = context.DueTimes.FirstOrDefault(d => d.Id == dueInId);
                            if (dueIn == null)
                            {
                                throw new Exception($"DueIn with Id {dueInId} not found.");
                            }
                            DateTime assignmentDueDate = dueDate
                                .AddMonths(dueIn.Months)
                                .AddDays(dueIn.Weeks * 7 + dueIn.Days);
                            if (dueIn.Label == "ASAP")
                            {
                                assignmentDueDate = DateTime.Today;
                            }
                            var forContractsList = a.TryGetProperty(
                                "Contracts",
                                out var contractsElement
                            )
                                ? contractsElement
                                    .EnumerateArray()
                                    .Select(c =>
                                        context.Contracts.FirstOrDefault(con =>
                                            con.Id == c.GetProperty("ContractId").GetInt32()
                                        )
                                    )
                                    .ToList()
                                : new List<Contract>();
                            var forDepartmentsList = a.TryGetProperty(
                                "Departments",
                                out var departmentsElement
                            )
                                ? departmentsElement
                                    .EnumerateArray()
                                    .Select(d =>
                                        context.Departments.FirstOrDefault(dep =>
                                            dep.Id == d.GetProperty("DepartmentId").GetInt32()
                                        )
                                    )
                                    .ToList()
                                : new List<Department>();
                            if (
                                (
                                    forContractsList.Count() == 0
                                    || forContractsList.Contains(contractOfRefWorker)
                                )
                                && (
                                    forDepartmentsList.Count() == 0
                                    || forDepartmentsList.Contains(departmentOfRefWorker)
                                )
                            )
                            {
                                assignments.Add(
                                    new Assignment
                                    {
                                        Title = a.GetProperty("Title").GetString(),
                                        Instructions = a.GetProperty("Instructions").GetString(),
                                        DueDate = assignmentDueDate,
                                        AssigneeType = assigneeType,
                                        Assignee = assignee,
                                        AssignedRole = assignedRole,
                                        Status = (AssignmentStatus)
                                            a.GetProperty("Status").GetInt32(),
                                        ForDepartmentsList = new List<Department>(),
                                        ForContractsList = new List<Contract>()
                                    }
                                );
                            }
                        }
                        var process = new Process
                        {
                            Title = title,
                            Description = description,
                            StartDate = startDate,
                            DueDate = dueDate,
                            WorkerOfReference = workerOfReference,
                            Supervisor = supervisor,
                            Assignments = assignments,
                            ContractOfRefWorker = context.Contracts.FirstOrDefault(c =>
                                c.Id == contractOfRefWorkerId
                            ),
                            DepartmentOfRefWorker = context.Departments.FirstOrDefault(d =>
                                d.Id == DepartmentOfRefWorkerId
                            ),
                            IsArchived = p.GetProperty("isArchived").GetBoolean()
                        };

                        foreach (Assignment a in process.Assignments)
                        {
                            context.Assignments.Add(a);
                        }

                        context.Processes.Add(process);
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to seed processes:" + ex.Message);
            }

            try
            {
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to ensure database creation.", ex);
            }
        }
    }
}
