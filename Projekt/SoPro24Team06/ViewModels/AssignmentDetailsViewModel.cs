//beginn codeownership Jan Pfluger
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SoPro24Team06.Enums;
using SoPro24Team06.Helpers;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels
{
    public class AssignmentDetailsViewModel
    {
        public Assignment Assignment { get; set; }
        public IEnumerable<SelectListItem> UserList { get; set; }
        public IEnumerable<SelectListItem> RoleList { get; set; }
        public string ProcessTitle { get; set; }
        public IEnumerable<SelectListItem> AssignmentStatusList { get; set; }

		public IEnumerable<SelectListItem> AssigneeTypeList {get; set;}

        public AssignmentDetailsViewModel(
            Assignment assignment,
            List<ApplicationUser> userList,
            List<ApplicationRole> roleList,
            Process? process
        )
        {
            this.Assignment = assignment;
			if(process != null)
			{
				this.ProcessTitle = process.Title;
			}
			else 
			{
				this.ProcessTitle = "es konnte kein zugehöriger Vorgang gefunden werden";
			}
			InitialiseSelectList(userList, roleList);
        }

        public AssignmentDetailsViewModel() { }

		public void InitialiseSelectList (
			List<ApplicationUser> userList,
            List<ApplicationRole> roleList
		)
		{
			if (
                this.Assignment.AssigneeType == AssigneeType.USER
                && this.Assignment.Assignee != null
            )
            {
                this.UserList = new SelectList(
                    userList,
                    "Id",
                    "FullName",
                    this.Assignment.Assignee
                );
            }
            else
            {
                this.UserList = new SelectList(userList, "Id", "FullName");
            }

            if (
                this.Assignment.AssigneeType == AssigneeType.ROLES
                && this.Assignment.AssignedRole != null
            )
            {
                this.RoleList = new SelectList(
                    roleList,
                    "Id",
                    "Name",
                    this.Assignment.AssignedRole
                );
            }
            else
            {
                this.RoleList = new SelectList(roleList, "Id", "Name");
            }

            this.AssignmentStatusList = new SelectList(
                EnumHelper.GetEnumList<AssignmentStatus>(),
                "Value",
                "Text",
                Assignment.Status
            );
			
			List<AssigneeType> assigneeTypeList = new List<AssigneeType> () {
				AssigneeType.ROLES,
				AssigneeType.USER,
			};
			AssigneeTypeList = assigneeTypeList
            .Select(type => new SelectListItem
            {
                Value = type.ToString(),
                Text = EnumHelper.GetDisplayName(type),
                Selected = type == Assignment.AssigneeType
            });
		}
    }
}
//end codeownership Jan Pfluger
