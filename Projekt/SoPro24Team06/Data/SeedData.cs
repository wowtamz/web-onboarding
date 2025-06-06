using Microsoft.AspNetCore.Identity;
using SoPro24Team06.Enums;
using SoPro24Team06.Models;
using SQLitePCL;

namespace SoPro24Team06.Data
{
    public static class SeedData
    {
        public static async Task Initialize(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context
        )
        {
            if (roleManager.Roles.Count() <= 0)
            {
                string[] roleNames =
                {
                    "Administrator",
                    "IT",
                    "Backoffice",
                    "Geschäftsleitung",
                    "Personal"
                };
                IdentityResult roleResult;

                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        roleResult = await roleManager.CreateAsync(new ApplicationRole(roleName));
                    }
                }
            }

            if (userManager.Users.Count() <= 0)
            {
                var users = new List<(
                    string UserName,
                    string FullName,
                    string Email,
                    string Password,
                    string[] Roles
                )>
                {
                    ("admin@gmx.de", "Admin", "admin@gmx.de", "Admin1!", new[] { "Administrator" }),
                    (
                        "koenig@gmx.de",
                        "König v. Augsburg",
                        "koenig@gmx.de",
                        "GHLutz13!",
                        new[] { "Geschäftsleitung" }
                    ),
                    (
                        "gerhard@gmx.de",
                        "Gerhard",
                        "gerhard@gmx.de",
                        "Gerhard1!",
                        new[] { "Backoffice" }
                    ),
                    (
                        "bill@gmx.de",
                        "Bill Yard",
                        "bill@gmx.de",
                        "B1llard!",
                        new[] { "Personal", "Backoffice" }
                    ),
                    ("karl@gmx.de", "Karl Toffel", "karl@gmx.de", "Erdapfel1!", new[] { "IT" }),
                    ("wilma@gmx.de", "Wilma Ruhe", "wilma@gmx.de", "Pssssst1!", new string[] { })
                };

                foreach (var (userName, fullName, email, password, roles) in users)
                {
                    var user = await userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        if (email == null)
                            continue;
                        user = new ApplicationUser
                        {
                            UserName = userName,
                            FullName = fullName,
                            Email = email
                        };
                        var result = await userManager.CreateAsync(user, password);
                        if (result.Succeeded)
                        {
                            foreach (var role in roles)
                            {
                                await userManager.AddToRoleAsync(user, role);
                            }
                        }
                        else
                        {
                            throw new Exception(
                                $"Failed to create user {userName}: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                            );
                        }
                    }
                }
            }
            context.Database.EnsureCreated();

            List<AssignmentTemplate> assignmentTemplateList = new();
            List<ProcessTemplate> processTemplateList = new();

            // Prüfen ob Einträge existieren

            if (!context.Contracts.Any())
            {
                List<Contract> contracts =
                    new()
                    {
                        new Contract() { Id = 1, Label = "Festanstellung", },
                        new Contract() { Id = 2, Label = "Trainee", },
                        new Contract() { Id = 3, Label = "Praktikant", },
                        new Contract() { Id = 4, Label = "Werkstudent", },
                    };

                foreach (Contract t in contracts)
                {
                    context.Contracts.Add(t);
                }
                context.SaveChanges();
            }

            if (!context.Departments.Any())
            {
                List<Department> departments =
                    new()
                    {
                        new Department { Name = "Entwicklung" },
                        new Department { Name = "Operations" },
                        new Department { Name = "UI/UX" },
                        new Department { Name = "Projektmanagement" },
                        new Department { Name = "Backoffice" },
                        new Department { Name = "Sales" },
                        new Department { Name = "People & Culture" },
                    };

                foreach (Department t in departments)
                {
                    context.Departments.Add(t);
                }
                context.SaveChanges();
            }

            if (!context.DueTimes.Any())
            {
                List<DueTime> dueTimes =
                    new()
                    {
                        new DueTime
                        {
                            Id = 1,
                            Label = "ASAP",
                            Days = 0,
                            Weeks = 0,
                            Months = 0
                        },
                        new DueTime
                        {
                            Id = 2,
                            Label = "2 Wochen vor Start",
                            Days = -14,
                            Weeks = 0,
                            Months = 0
                        },
                        new DueTime
                        {
                            Id = 3,
                            Label = "3 Wochen nach Arbeitsbeginn",
                            Days = 21,
                            Weeks = 0,
                            Months = 0
                        },
                        new DueTime
                        {
                            Id = 4,
                            Label = "3 Monate nach Arbeitsbeginn",
                            Days = 0,
                            Weeks = 0,
                            Months = 3
                        },
                        new DueTime
                        {
                            Id = 5,
                            Label = "6 Monate nach Arbeitsbeginn",
                            Days = 0,
                            Weeks = 0,
                            Months = 6
                        }
                    };

                foreach (DueTime t in dueTimes)
                {
                    context.DueTimes.Add(t);
                }
                context.SaveChanges();
            }

            if (!context.ProcessTemplates.Any())
            {
                List<ProcessTemplate> processTemplates =
                    new()
                    {
                        new ProcessTemplate
                        {
                            Id = 1,
                            Title = "Onboarding",
                            Description = "Onboarding neuer Mitarbeiter",
                            RolesWithAccess = new List<ApplicationRole>
                            {
                                await roleManager.FindByNameAsync("Administrator"),
                                await roleManager.FindByNameAsync("Geschäftsleitung"),
                                await roleManager.FindByNameAsync("Personal")
                            },
                            ContractOfRefWorker = context.Contracts.FirstOrDefault(x =>
                                x.Label == "Festanstellung"
                            ),
                            DepartmentOfRefWorker = context.Departments.FirstOrDefault(x =>
                                x.Name == "People & Culture"
                            ),
                            AssignmentTemplates = new List<AssignmentTemplate>
                            {
                                new AssignmentTemplate
                                {
                                    Id = 1,
                                    Title = "GSuite Account erstellen",
                                    Instructions =
                                        "Bitte erstellen Sie einen GSuite Account für den neuen Mitarbeiter.",
                                    AssigneeType = AssigneeType.ROLES,
                                    DueIn = context.DueTimes.FirstOrDefault(x => x.Label == "ASAP"),
                                    ProcessTemplateId = 1
                                },
                                new AssignmentTemplate
                                {
                                    Id = 2,
                                    Title = "Hardware",
                                    Instructions = "",
                                    AssigneeType = AssigneeType.WORKER_OF_REF,
                                    DueIn = context.DueTimes.FirstOrDefault(x => x.Label == "ASAP"),
                                    AssignedRole = await roleManager.FindByNameAsync("IT"),
                                    ProcessTemplateId = 1
                                },
                                new AssignmentTemplate
                                {
                                    Id = 3,
                                    Title = "Grundlagen der Büro-IT-Sicherheit",
                                    Instructions = "",
                                    AssigneeType = AssigneeType.WORKER_OF_REF,
                                    DueIn = context.DueTimes.FirstOrDefault(x => x.Label == "ASAP"),
                                    AssignedRole = await roleManager.FindByNameAsync("IT"),
                                    ForContractsList = new List<Contract>
                                    {
                                        context.Contracts.FirstOrDefault(x =>
                                            x.Label == "Werkstudent"
                                        ),
                                        context.Contracts.FirstOrDefault(x => x.Label == "Trainee"),
                                        context.Contracts.FirstOrDefault(x =>
                                            x.Label == "Praktikant"
                                        ),
                                    },
                                    ProcessTemplateId = 1
                                },
                                new AssignmentTemplate
                                {
                                    Id = 4,
                                    Title = "Anmeldedaten einrichten",
                                    Instructions = "",
                                    AssigneeType = AssigneeType.WORKER_OF_REF,
                                    DueIn = context.DueTimes.FirstOrDefault(x => x.Label == "ASAP"),
                                    ForContractsList = new List<Contract>
                                    {
                                        context.Contracts.FirstOrDefault(x =>
                                            x.Label == "Festanstellung"
                                        )
                                    },
                                    ProcessTemplateId = 1
                                },
                                new AssignmentTemplate
                                {
                                    Id = 5,
                                    Title = "Erste Schritte mit Gitlab'",
                                    Instructions = "",
                                    AssigneeType = AssigneeType.WORKER_OF_REF,
                                    DueIn = context.DueTimes.FirstOrDefault(x => x.Label == "ASAP"),
                                    ForContractsList = new List<Contract>
                                    {
                                        context.Contracts.FirstOrDefault(x =>
                                            x.Label == "Werkstudent"
                                        ),
                                        context.Contracts.FirstOrDefault(x => x.Label == "Trainee"),
                                        context.Contracts.FirstOrDefault(x =>
                                            x.Label == "Praktikant"
                                        ),
                                    },
                                    ProcessTemplateId = 1
                                },
                            }
                        }
                    };

                foreach (ProcessTemplate t in processTemplates)
                {
                    context.ProcessTemplates.Add(t);
                    processTemplateList.Add(t);
                }
                context.SaveChanges();
            }

            if (!context.Processes.Any())
            {
                List<Process> processes = new List<Process>
                {
                    new Process
                    {
                        Id = 1,
                        Title = "Neuzugang HR",
                        Description = "",
                        ContractOfRefWorker = context.Contracts.FirstOrDefault(x =>
                            x.Label == "Trainee"
                        ),
                        DepartmentOfRefWorker = context.Departments.FirstOrDefault(x =>
                            x.Name == "People & Culture"
                        ),
                        Supervisor = await userManager.FindByNameAsync("koenig@gmx.de"),
                        WorkerOfReference = await userManager.FindByNameAsync("bill@gmx.de"),
                        DueDate = new DateTime(2024, 9, 13),
                        IsArchived = false,
                        Assignments = new List<Assignment> { }
                    },
                    new Process
                    {
                        Id = 2,
                        Title = "Neuzugang IT",
                        Description = "",
                        ContractOfRefWorker = context.Contracts.FirstOrDefault(x =>
                            x.Label == "Festanstellung"
                        ),
                        DepartmentOfRefWorker = context.Departments.FirstOrDefault(x =>
                            x.Name == "Entwicklung"
                        ),
                        Supervisor = await userManager.FindByNameAsync("gerhard@gmx.de"),
                        WorkerOfReference = await userManager.FindByNameAsync("karl@gmx.de"),
                        IsArchived = false,
                        DueDate = new DateTime(2024, 7, 27),
                        Assignments = new List<Assignment>
                        {
                            new Assignment
                            {
                                Id = 1,
                                Title = "GSuite Account erstellen",
                                Instructions =
                                    "Bitte erstellen Sie einen GSuite Account für den neuen Mitarbeiter.",
                                AssigneeType = AssigneeType.ROLES,
                                DueDate = new DateTime(2024, 7, 27),
                                Status = AssignmentStatus.DONE,
                            },
                            new Assignment
                            {
                                Id = 2,
                                Title = "Hardware",
                                Instructions = "",
                                AssigneeType = AssigneeType.WORKER_OF_REF,
                                AssignedRole = await roleManager.FindByNameAsync("IT"),
                                DueDate = new DateTime(2024, 7, 27),
                                Status = AssignmentStatus.DONE,
                            },
                            new Assignment
                            {
                                Id = 3,
                                Title = "Grundlagen der Büro-IT-Sicherheit",
                                Instructions = "",
                                AssigneeType = AssigneeType.WORKER_OF_REF,
                                AssignedRole = await roleManager.FindByNameAsync("IT"),
                                ForContractsList = new List<Contract>
                                {
                                    context.Contracts.FirstOrDefault(x => x.Label == "Werkstudent"),
                                    context.Contracts.FirstOrDefault(x => x.Label == "Trainee"),
                                    context.Contracts.FirstOrDefault(x => x.Label == "Praktikant"),
                                },
                                DueDate = new DateTime(2024, 7, 27),
                                Status = AssignmentStatus.DONE,
                            },
                            new Assignment
                            {
                                Id = 4,
                                Title = "Anmeldedaten einrichten",
                                Instructions = "",
                                AssigneeType = AssigneeType.WORKER_OF_REF,
                                ForContractsList = new List<Contract>
                                {
                                    context.Contracts.FirstOrDefault(x =>
                                        x.Label == "Festanstellung"
                                    )
                                },
                                DueDate = new DateTime(2024, 7, 27),
                                Status = AssignmentStatus.DONE,
                            },
                            new Assignment
                            {
                                Id = 5,
                                Title = "Erste Schritte mit Gitlab'",
                                Instructions = "",
                                AssigneeType = AssigneeType.WORKER_OF_REF,
                                ForContractsList = new List<Contract>
                                {
                                    context.Contracts.FirstOrDefault(x => x.Label == "Werkstudent"),
                                    context.Contracts.FirstOrDefault(x => x.Label == "Trainee"),
                                    context.Contracts.FirstOrDefault(x => x.Label == "Praktikant"),
                                },
                                DueDate = new DateTime(2024, 7, 27),
                                Status = AssignmentStatus.DONE,
                            },
                        }
                    },
                    new Process
                    {
                        Id = 3,
                        Title = "Neuzugang IT/Trainee",
                        Description = "",
                        ContractOfRefWorker = context.Contracts.FirstOrDefault(x =>
                            x.Label == "Trainee"
                        ),
                        DepartmentOfRefWorker = context.Departments.FirstOrDefault(x =>
                            x.Name == "Entwicklung"
                        ),
                        Supervisor = await userManager.FindByNameAsync("bill@gmx.de"),
                        WorkerOfReference = await userManager.FindByNameAsync("wilma@gmx.de"),
                        IsArchived = false,
                        DueDate = new DateTime(2024, 10, 13),
                        Assignments = new List<Assignment>
                        {
                            new Assignment
                            {
                                Id = 6,
                                Title = "GSuite Account erstellen",
                                Instructions =
                                    "Bitte erstellen Sie einen GSuite Account für den neuen Mitarbeiter.",
                                AssigneeType = AssigneeType.ROLES,
                                DueDate = new DateTime(2024, 10, 13),
                                Status = AssignmentStatus.NOT_STARTED,
                            },
                            new Assignment
                            {
                                Id = 7,
                                Title = "Hardware",
                                Instructions = "",
                                AssigneeType = AssigneeType.WORKER_OF_REF,
                                AssignedRole = await roleManager.FindByNameAsync("IT"),
                                DueDate = new DateTime(2024, 10, 13),
                                Status = AssignmentStatus.DONE,
                            },
                            new Assignment
                            {
                                Id = 8,
                                Title = "Grundlagen der Büro-IT-Sicherheit",
                                Instructions = "",
                                AssigneeType = AssigneeType.WORKER_OF_REF,
                                AssignedRole = await roleManager.FindByNameAsync("IT"),
                                ForContractsList = new List<Contract>
                                {
                                    context.Contracts.FirstOrDefault(x => x.Label == "Werkstudent"),
                                    context.Contracts.FirstOrDefault(x => x.Label == "Trainee"),
                                    context.Contracts.FirstOrDefault(x => x.Label == "Praktikant"),
                                },
                                DueDate = new DateTime(2024, 10, 13),
                                Status = AssignmentStatus.DONE,
                            },
                            new Assignment
                            {
                                Id = 9,
                                Title = "Anmeldedaten einrichten",
                                Instructions = "",
                                AssigneeType = AssigneeType.WORKER_OF_REF,
                                ForContractsList = new List<Contract>
                                {
                                    context.Contracts.FirstOrDefault(x =>
                                        x.Label == "Festanstellung"
                                    )
                                },
                                DueDate = new DateTime(2024, 10, 13),
                                Status = AssignmentStatus.DONE,
                            },
                            new Assignment
                            {
                                Id = 10,
                                Title = "Erste Schritte mit Gitlab'",
                                Instructions = "",
                                AssigneeType = AssigneeType.WORKER_OF_REF,
                                ForContractsList = new List<Contract>
                                {
                                    context.Contracts.FirstOrDefault(x => x.Label == "Werkstudent"),
                                    context.Contracts.FirstOrDefault(x => x.Label == "Trainee"),
                                    context.Contracts.FirstOrDefault(x => x.Label == "Praktikant"),
                                },
                                DueDate = new DateTime(2024, 10, 13),
                                Status = AssignmentStatus.DONE,
                            },
                        }
                    },
                };

                foreach (Process t in processes)
                {
                    context.Processes.Add(t);
                }
                context.SaveChanges();
            }
        }
    }
}
