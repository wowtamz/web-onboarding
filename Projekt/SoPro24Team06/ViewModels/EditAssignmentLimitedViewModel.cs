//beginn codeownership Jan Pfluger
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SoPro24Team06.Enums;
using SoPro24Team06.Helpers;
using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels
{
    public class EditAssignmentLimitedViewModel
    {
        /// <summary>
        /// Assignment to be Editd
        /// </summary>
        public Assignment Assignment { get; set; }

        /// <summary>
        /// Id of User wich was / will be selected in the View
        /// </summary>
        public string? SelectedUserId { get; set; }

        /// <summary>
        /// Id of Role wich was / will be selected in the view
        /// </summary>

        public string? SelectedRoleId { get; set; }

        /// <summary>
        /// list of choosable user for AssignedUser
        /// </summary>
        public SelectList UserList { get; set; }

        /// <summary>
        /// list of choosable roles for AssignedRole
        /// </summary>
        public SelectList RoleList { get; set; }

        /// <summary>
        /// Title of Process wich contains the Assignment
        /// </summary>
        public string ProcessTitle { get; set; }

        /// <summary>
        /// List of selectable Statuses
        /// </summary>
        public IEnumerable<SelectListItem> AssignmentStatusList { get; set; }

        /// <summary>
        /// list of selectable AssigneeTypes
        /// </summary>
        public IEnumerable<SelectListItem> AssigneeTypeList { get; set; }

        /// <summary>
        /// Construktor for the View Model
        /// Sets required Parameters
        /// </summary>
        /// <param name="assignment">assingment to be editet</param>
        /// <param name="userList">list of users wich will be selectable</param>
        /// <param name="roleList">list of roles wich will be selectable</param>
        /// <param name="process">Process wich contains the assignment</param>
        public EditAssignmentLimitedViewModel(
            Assignment assignment,
            List<ApplicationUser> userList,
            List<ApplicationRole> roleList,
            Process? process
        )
        {
            //set properties
            this.Assignment = assignment;
            //preselect SelectedRole and SelectedID
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
            //initialise the SelectList Properties
            InitialiseSelectList(userList, roleList);
        }

        /// <summary>
        /// Empty Constructor neccecary for Postrequest
        /// Should not be used by the developer
        /// </summary>
        public EditAssignmentLimitedViewModel()
        {
            UserList = new SelectList(new List<SelectListItem>());
            RoleList = new SelectList(new List<SelectListItem>());
            AssignmentStatusList = new List<SelectListItem>();
            AssigneeTypeList = new List<SelectListItem>();
        }

        /// <summary>
        /// Initialises Select List parameters of this obejkt with the content of the lists provided
        /// </summary>
        /// <param name="userList">list of users wich will be selectable</param>
        /// <param name="roleList">list of useres wich will be selectable</param>
        public void InitialiseSelectList(
            List<ApplicationUser> userList,
            List<ApplicationRole> roleList
        )
        {
            //check if and wich user has to be preselected
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

            //check if and wich role has to be preselected
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

            //set to possible status options and preselect current
            List<AssignmentStatus> assignmentStatusList =
                EnumHelper.GetEnumList<AssignmentStatus>();
            this.AssignmentStatusList = assignmentStatusList.Select(status => new SelectListItem
            {
                Value = status.ToString(),
                Text = EnumHelper.GetDisplayName(status),
                Selected = status == Assignment.Status
            });

            //set assigneeType optons and preselect currentstatus
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
//end codeownership Jan  Pfluger
