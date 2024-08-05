//-------------------------
// Author: Vincent Steiner 
//-------------------------

using SoPro24Team06.Models;
using SoPro24Team06.Enums;


namespace SoPro24Team06.Interfaces{
    public interface IAssignmentTemplate{
            public  Task<AssignmentTemplate> AddAssignmentTemplate(
            string title,
            string? instructions,
            DueTime dueIn,
            List<Department>? forDepartmentsList,
            List<Contract>? forContractsList,
            AssigneeType assigneeType,
            ApplicationRole? assignedRole,
            int? processTemplateId
        );
            public Task DeleteAssignmentTemplate(int id);
            public Task EditAssignmentTemplates(
            int id,
            string title,
            string? instructions,
            DueTime dueIn,
            List<Department>? forDepartmentsList,
            List<Contract>? forContractsList,
            AssigneeType assigneeType,
            ApplicationRole? assignedRole
        );
        public Task<AssignmentTemplate> GetAssignmentTemplate(int id);
        public Task<AssignmentTemplate?> GetAssignmentTemplateWithDepartmentsAsync(int id);
        public Task<List<AssignmentTemplate>> GetAllAssignmentTemplates();
    }
}