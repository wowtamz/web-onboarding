using Microsoft.AspNetCore.Mvc.Rendering;
using SoPro24Team06.Enums;
using SoPro24Team06.Helpers;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels
{
    public class AssignmentEditViewModel
    {
        public Assignment Assignment { get; set; }
        public int SelectedProcessId { get; set; }
        public List<Process> ProcessList { get; set; }
        public SelectList UserList { get; set; }
        public SelectList RoleList { get; set; }
        public int AssingeeType { get; set; }
        public SelectList AssignmentStatusList { get; set; }

        public AssignmentEditViewModel(
            Assignment assignment,
            int selectedProcessId,
            List<Process> processList,
            List<ApplicationUser> userList,
            List<ApplicationRole> roleList
        )
        {
            this.Assignment = assignment;
            this.SelectedProcessId = selectedProcessId;
            this.ProcessList = processList;

            if (this.Assignment.Assignee != null)
            {
                this.UserList = new SelectList(userList, "Id", "Name", this.Assignment.Assignee.Id);
            }
            else
            {
                this.UserList = new SelectList(userList, "Id", "Name");
            }

            if (this.Assignment.AssignedRole != null)
            {
                this.RoleList = new SelectList(
                    roleList,
                    "Id",
                    "Name",
                    this.Assignment.AssignedRole.Id
                );
            }
            else
            {
                this.RoleList = new SelectList(roleList, "Id", "Name");
            }
            this.AssignmentStatusList = new SelectList(
                EnumHelper.GetEnumList<AssignmentStatus>(),
                "Value",
                "Text"
            );
        }
    }
}
