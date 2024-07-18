//-------------------------
// Author: Tamas Varadi
//-------------------------

using Microsoft.AspNetCore.Identity;

namespace SoPro24Team06.Models;

public class ApplicationRole : IdentityRole
{
    public List<ProcessTemplate>? ProcessTemplates { get; set; }

    //public List<AssignmentTemplate> AssignmentTemplates { get; set; }
    //public List<Assignment> Assignments { get; set; }

    public ApplicationRole() { }

    public ApplicationRole(string roleName)
        : base(roleName) { }
}
