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
    /// <summary>
    /// Gets the display name of the passed enum
    /// </summary>
    /// <param name="value"></param>
    /// <returns>String display name of enum</returns>
    public static string GetDisplayName(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var displayAttribute = field
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault();
        return displayAttribute?.Name ?? value.ToString();
    }

    /// <summary>
    ///  Codeownership Jan Pfluger
    ///  Method used to get list of all enums of a sepcific type
    ///  Returns List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<T> GetEnumList<T>()
        where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }
}
