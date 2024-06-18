using SoPro24Team06.Models;
using SoPro24Team06.Data;
using System.Collections.Generic;

namespace SoPro24Team06.Containers;
public class AssigmentTemplateContainer
{
    private Context db = new();

    public void AddAssigmentTemplate(string title, string? instructions, DueTime dueIn, List<Department>? forDepartmentsList, List<Contract>? forContractsList, AssigneType assigneType, List<Role>? assignedRoles)
    {
        db.Add(new AssigmentTemplate{Title = title, Instructions = instructions, DueIn = dueIn, ForDepartmentList = forDepartmentList, ForContractList = forContractList, AssigneType = assigneType, AssignedRoles = assignedRoles});
        db.SaveChanges();
    }
    public void DeleteAssigmentTemplate(int id)
    {
        if(db.AssigmentTemplates != null){
            AssigmentTemplate? assigmentTemplate= db.AssigmentTemplates.FirstOrDefault(assigmentTemplate => assigmentTemplate.id == id);
            if(assigmentTemplate != null){
                db.Remove(assigmentTemplate!);
                db.SaveChanges();
            }
        }
    }
    public void EditAssigmentTemplate(string title, string? instructions, DueTime dueIn, List<Department>? forDepartmentsList, List<Contract>? forContractsList, AssigneeType assigneeType, List<Role>? assignedRoles)
    {
        if(db.AssigmentTemplates != null){
            AssigmentTemplate? assigmentTemplate = db.AssigmentTemplates.FirstOrDefault(assigmentTemplate => assigmentTemplate.id == id);
            if(assigmentTemplate != null){
                assigmentTemplate.Title = title;
                assigmentTemplate.Instructions = instructions;
                assigmentTemplate.DueIn = dueIn;
                assigmentTemplate.ForDepartmentsList = forDepartmentsList;
                assigmentTemplate.ForContractsList = forContractsList;
                assigmentTemplate.AssigneeType = assigneeType;
                assigmentTemplate.AssignedRoles = assignedRoles;
                db.Update(assigmentTemplate);
                db.SaveChanges();
            }
        }
    }
    public AssigmentTemplate GetAssigmentTemplate(int id)
    {
         if(db.AssigmentTemplates != null){
            AssigmentTemplate? assigmentTemplate = db.AssigmentTemplates.FirstOrDefault(assigmentTemplate => assigmentTemplate.id == id);
            if(assigmentTemplate != null){
                return assigmentTemplate!;
            }
        }
        return null;
    }

     public List<AssigmentTemplate> GetAllAssigmentTemplate()
    {
        List<AssigmentTemplate> assigmentTemplateList = new List<AssigmentTemplate>();
        if(db.AssigmentTemplates != null){
            lessons = db.AssigmentTemplates.ToList();
        }
        return assigmentTemplateList;
    }
}