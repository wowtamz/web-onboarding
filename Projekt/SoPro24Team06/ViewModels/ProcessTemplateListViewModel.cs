using SoPro24Team06.Models;

namespace SoPro24Team06.ViewModels;

public class ProcessTemplateListViewModel
{
    public List<ProcessTemplate> ProcessTemplateList { get; set; }

    public ProcessTemplateListViewModel(List<ProcessTemplate> processTemplateList)
    {
        this.ProcessTemplateList = processTemplateList;
    }

    public ProcessTemplateListViewModel()
    {
        this.ProcessTemplateList = new List<ProcessTemplate>();
    }
}
