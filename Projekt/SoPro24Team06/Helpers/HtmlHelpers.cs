using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

//-------------------------
// Author: Michael Adolf
//-------------------------

namespace SoPro24Team06.Helpers
{
    public static class HtmlHelpers
    {
        /// <summary>
        /// Creates a SelectList for the given roles
        /// </summary>
        public static SelectList GetRolesSelectList(
            IEnumerable<string> allRoles,
            IEnumerable<string> selectedRoles
        )
        {
            return new SelectList(
                allRoles
                    .Select(role => new SelectListItem
                    {
                        Value = role,
                        Text = role,
                        Selected = selectedRoles.Contains(role)
                    })
                    .ToList(),
                "Value",
                "Text"
            );
        }
    }
}
