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

    /// <summary>
    ///  Codeownership Jan Pfluger
    ///  Method used to get list of all enums of a sepcific type
    ///  Returns List
    /// </summary>
    /// <typeparam name="T">Type of enum for wich the list should be generated</typeparam>
    /// <returns></returns>
    public static List<T> GetEnumList<T>()
        where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }
}
