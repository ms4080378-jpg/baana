using elbanna.ViewModels;

namespace YourProject.ViewModels
{
    public class ItemPurchasePageVM
    {
        public ItemPurchaseFilterVM Filter { get; set; } = new();
        public ItemPurchaseFormVM Form { get; set; } = new();

        public List<ItemPurchaseGridRowVM> Grid { get; set; } = new();
    }
}