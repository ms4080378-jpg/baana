using elbanna.ViewModels;

namespace elbanna.Models.ViewModels
{
    public class ProjectStatusPageVM
    {
        public List<string> Floors { get; set; } = new();
        public List<ProjectStatusRowVM> Rows { get; set; } = new();
    }
}
