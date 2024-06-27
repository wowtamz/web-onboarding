using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace SoPro24Team06.Helpers
{
    public static class HtmlHelpers
    {
        public static SelectList GetRolesSelectList(IEnumerable<string> allRoles, IEnumerable<string> selectedRoles)
        {
            return new SelectList(allRoles.Select(role => new SelectListItem
            {
                Value = role,
                Text = role,
                Selected = selectedRoles.Contains(role)
            }).ToList(), "Value", "Text");
        }
    }
}
