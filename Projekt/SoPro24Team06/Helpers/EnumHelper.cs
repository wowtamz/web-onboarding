//-------------------------
// Author: Tamas Varadi
//-------------------------

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SoPro24Team06.Helpers;

public static class EnumHelper
{
    public static string GetDisplayName(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var displayAttribute = field
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault();
        return displayAttribute?.Name ?? value.ToString();
    }

    public static SelectListItem GetSelectListItem<T>(Enum value)
    {
        return new SelectListItem
        {
            Text = EnumHelper.GetDisplayName(value),
            Value = value.ToString()
        };
    }

    public static List<SelectListItem> GetEnumList<T>()
        where T : Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => new SelectListItem
            {
                Text = EnumHelper.GetDisplayName(e),
                Value = e.ToString(),
            })
            .ToList();
    }
}
