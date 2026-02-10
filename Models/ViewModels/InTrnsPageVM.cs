namespace elbanna.ViewModels
{
    public class InTrnsPageVM
    {
        public InTrnsFilterVM Filter { get; set; } = new();
        public InTrnsFormVM Form { get; set; } = new();

        public List<InTrnsGridRowVM> Grid { get; set; } = new();
    }
}
