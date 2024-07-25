using Microsoft.AspNetCore.Mvc.Rendering;
using SoPro24Team06.Enums;
using SoPro24Team06.Helpers;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels
{
    public class AssignmentEditViewModel
    {
        public Assignment Assignment { get; set; }
        public string? SelectedUserId { get; set; }
        public string? SelectedRoleId { get; set; }

        public SelectList UserList { get; set; }
        public SelectList RoleList { get; set; }
        public string ProcessTitle { get; set; }
        public IEnumerable<SelectListItem> AssignmentStatusList { get; set; }
        public IEnumerable<SelectListItem> AssigneeTypeList { get; set; }

        public AssignmentEditViewModel(
            Assignment assignment,
            List<ApplicationUser> userList,
            List<ApplicationRole> roleList,
            Process? process
        )
        {
            this.Assignment = assignment;
            if (process != null)
            {
                this.ProcessTitle = process.Title;
            }
            else
            {
                this.ProcessTitle = "es konnte kein zugehöriger Vorgang gefunden werden";
            }
            InitialiseSelectLists(userList, roleList);
        }

        public AssignmentEditViewModel()
        {
            UserList = new SelectList(new List<SelectListItem>());
            RoleList = new SelectList(new List<SelectListItem>());
            AssignmentStatusList = new List<SelectListItem>();
            AssigneeTypeList = new List<SelectListItem>();
        }

        public void InitialiseSelectLists(
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

            List<AssignmentStatus> assignmentStatusList =
                EnumHelper.GetEnumList<AssignmentStatus>();
            this.AssignmentStatusList = assignmentStatusList.Select(status => new SelectListItem
            {
                Value = status.ToString(),
                Text = EnumHelper.GetDisplayName(status),
                Selected = status == Assignment.Status
            });

            List<AssigneeType> assigneeTypeList = new List<AssigneeType>()
            {
                AssigneeType.ROLES,
                AssigneeType.USER,
            };
            AssigneeTypeList = assigneeTypeList.Select(type => new SelectListItem
            {
                Value = type.ToString(),
                Text = EnumHelper.GetDisplayName(type),
                Selected = type == Assignment.AssigneeType
            });
        }
    }
}
