using elbanna.Data;
using elbanna.Models;
using YourProject.Models;

namespace YourProject.Data
{
    public class ItemPurchaseRepository
    {
        private readonly AppDbContext _db;

        public ItemPurchaseRepository(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<pr_itempurchase> Query()
        {
            return _db.ItemPurchases.AsQueryable();
        }

        public IQueryable<hr_user> Users()
        {
            return _db.hr_user.AsQueryable();
        }

        public List<acc_CostCenter> GetCostCenters()
        {
            return _db.CostCenters.OrderBy(x => x.costCenter).ToList();
        }

        public List<Dealer> GetDealers()
        {
            return _db.Dealers
                .Where(x => x.isStopped != true)
                .OrderBy(x => x.dealer)
                .ToList();
        }

        public List<IC_ItemStore> GetItems()
        {
            return _db.ItemStores
                .Where(x => x.isVendorItem == true)
                .OrderBy(x => x.item)
                .ToList();
        }

        public List<pr_Inspect> GetInspects(int costCenterId, int itemId)
        {
            return _db.Inspects
                .Where(x => x.costcenterId == costCenterId && x.itemId == itemId)
                .ToList();
        }

        public void Add(pr_itempurchase entity)
        {
            _db.ItemPurchases.Add(entity);
            _db.SaveChanges();
        }

        public void Update(pr_itempurchase entity)
        {
            _db.ItemPurchases.Update(entity);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _db.ItemPurchases.Find(id);
            if (entity == null) return;

            _db.ItemPurchases.Remove(entity);
            _db.SaveChanges();
        }




    }
}