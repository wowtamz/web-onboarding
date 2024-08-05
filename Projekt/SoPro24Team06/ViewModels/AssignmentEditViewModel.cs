//beginn codeownership Jan Pfluger
using Microsoft.AspNetCore.Mvc.Rendering;
using SoPro24Team06.Enums;
using SoPro24Team06.Helpers;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels
{
    public class AssignmentEditViewModel
    {
        /// <summary>
        /// Assignment wich will be edited
        /// </summary>
        public Assignment Assignment { get; set; }

        /// <summary>
        /// Id of the in View Selected User
        /// </summary>
        public string? SelectedUserId { get; set; }

        /// <summary>
        /// Id of the in View Selected Role
        /// </summary>
        public string? SelectedRoleId { get; set; }

        /// <summary>
        /// in View Selected Due Date
        /// </summary>
        public string? SelectedDate { get; set; }

        /// <summary>
        /// SelectList of Users wich can be Selected
        /// </summary>
        public SelectList UserList { get; set; }

        /// <summary>
        /// SelectList of Roles wich can be Selected
        /// </summary>
        public SelectList RoleList { get; set; }

        /// <summary>
        /// Title of Process to wich the Assignment belongs
        /// </summary>
        public string ProcessTitle { get; set; }

        /// <summary>
        /// IEnumerable of possible Assignment Stauses ()
        /// </summary>
        public IEnumerable<SelectListItem> AssignmentStatusList { get; set; }

        /// <summary>
        /// IEnumerable of possible AssigneeTypes ()
        /// </summary>
        public IEnumerable<SelectListItem> AssigneeTypeList { get; set; }

        /// <summary>
        /// Constructs AssignmentEditViewModel
        /// </summary>
        /// <param name="assignment">Assignment wich will be edited</param>
        /// <param name="userList">List of users wich will be selectable</param>
        /// <param name="roleList">List of roles wich will be selectable</param>
        /// <param name="process">Process of the Assignment</param>
        public AssignmentEditViewModel(
            Assignment assignment,
            List<ApplicationUser> userList,
            List<ApplicationRole> roleList,
            Process? process
        )
        {
            this.Assignment = assignment;
            if (assignment.AssignedRole != null)
                SelectedRoleId = assignment.AssignedRole.Id;
            if (assignment.Assignee != null)
                SelectedUserId = assignment.Assignee.Id;
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

        /// <summary>
        /// Creates Empty AssignmentEditViewModel
        /// Do not use in Codemaually
        /// Only meant to be used by the ViewBindings
        /// Ititalises Lists with empty selectLists
        /// </summary>
        public AssignmentEditViewModel()
        {
            UserList = new SelectList(new List<SelectListItem>());
            RoleList = new SelectList(new List<SelectListItem>());
            AssignmentStatusList = new List<SelectListItem>();
            AssigneeTypeList = new List<SelectListItem>();
        }

        /// <summary>
        /// used to reinitialise the selectList for this viewModel
        /// </summary>
        /// <param name="userList">List of users wich will be selectable</param>
        /// <param name="roleList">List of roles wich will be selectable</param>
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
//end codeownership Jan Pfluger
