//beginn codeownership Jan Pfluger
using Microsoft.AspNetCore.Mvc.Rendering;
using SoPro24Team06.Enums;
using SoPro24Team06.Helpers;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels
{
    public class AssignmentDetailsViewModel
    {
        public Assignment Assignment { get; set; }
        public SelectList UserList { get; set; }
        public SelectList RoleList { get; set; }
        public Process? Process { get; set; }
        public SelectList AssignmentStatusList { get; set; }

		public SelectList AssigneeTypeList {get; set;}

        public AssignmentDetailsViewModel(
            Assignment assignment,
            List<ApplicationUser> userList,
            List<ApplicationRole> roleList,
            Process? process
        )
        {
            this.Assignment = assignment;
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
            this.Process = process;
            this.AssignmentStatusList = new SelectList(
                EnumHelper.GetEnumList<AssignmentStatus>(),
                "Value",
                "Text",
                Assignment.Status
            );
			
			List<AssigneeType> assingeeTypeListTemp = new List<AssigneeType> {AssigneeType.ROLES, AssigneeType.USER};
			AssigneeTypeList = new SelectList(assingeeTypeListTemp.Select(e => new {Value = e, Text = EnumHelper.GetDisplayName(e)}), "Value", "Text", Assignment.AssigneeType);
				
        }

        public AssignmentDetailsViewModel() { }
    }
}
//end codeownership Jan Pfluger
