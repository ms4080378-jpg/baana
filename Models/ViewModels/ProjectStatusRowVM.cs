namespace elbanna.ViewModels
{
    public class ProjectStatusRowVM
    {
        public string BuildingNo { get; set; }
        public string Item { get; set; }
        public string Dealer { get; set; }

        public Dictionary<string, decimal> Floors { get; set; } = new();

        public decimal Total { get; set; }
        public decimal Required { get; set; }
    }
}
