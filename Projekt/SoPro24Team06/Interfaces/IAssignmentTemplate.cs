//-------------------------
// Author: Vincent Steiner 
//-------------------------

using SoPro24Team06.Models;
using SoPro24Team06.Enums;


namespace SoPro24Team06.Interfaces{
    public interface IAssignmentTemplate{
            public AssignmentTemplate AddAssignmentTemplate(
            string title,
            string? instructions,
            DueTime dueIn,
            List<Department>? forDepartmentsList,
            List<Contract>? forContractsList,
            AssigneeType assigneeType,
            ApplicationRole? assignedRole,
            int? processTemplateId
        );
            public void DeleteAssignmentTemplate(int id);
            public void EditAssignmentTemplates(
            int id,
            string title,
            string? instructions,
            DueTime dueIn,
            List<Department>? forDepartmentsList,
            List<Contract>? forContractsList,
            AssigneeType assigneeType,
            ApplicationRole? assignedRole
        );
        public AssignmentTemplate GetAssignmentTemplate(int id);
        public AssignmentTemplate? GetAssignmentTemplateWithDepartmentsAsync(int id);
        public List<AssignmentTemplate> GetAllAssignmentTemplates();
    }
}