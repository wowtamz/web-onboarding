//beginn codeownership Jan Pfluger
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels
{
    public class AssignmentDetailsViewModel
    {
        public Assignment Assignment { get; set; }
        public string? ProcessTitle { get; set; }

        /// <summary>
        /// Constructs AssignmentDetails Viewmodel
        /// if process = null ProcessTitle = "es konnte kein zugehöriger Vorgang gefunden werden"
        /// </summary>
        /// <param name="assignment">Assignment wich should be Displayed</param>
        /// <param name="process">process to wich the Assignment belongs can be null</param>
        public AssignmentDetailsViewModel(Assignment assignment, Process? process)
        {
            if (process != null)
            {
                ProcessTitle = process.Title;
            }
            else
            {
                ProcessTitle = "es konnte kein zugehöriger Vorgang gefunden werden";
            }
            Assignment = assignment;
        }
    }
}
//end codeownership Jan Pfluger
