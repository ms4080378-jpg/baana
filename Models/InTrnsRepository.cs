using elbanna.Models;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Data
{
    public class InTrnsRepository
    {
        private readonly AppDbContext _context;

        public InTrnsRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<pr_intrn> Query()
            => _context.pr_intrns.AsNoTracking();

        public void Add(pr_intrn entity)
        {
            _context.pr_intrns.Add(entity);
        }

        public void Update(pr_intrn entity)
        {
            _context.pr_intrns.Update(entity);
        }

        public void Delete(pr_intrn entity)
        {
            _context.pr_intrns.Remove(entity);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public List<acc_CostCenter> GetCostCenters()
        {
            return _context.acc_CostCenters
                .OrderBy(x => x.costCenter)
                .ToList();
        }
    }
}
