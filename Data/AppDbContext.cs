using elbanna.Models;

using Microsoft.EntityFrameworkCore;
using YourProject.Models;

namespace elbanna.Data
{
    public class AppDbContext : DbContext
    {


        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
        public DbSet<acc_Daily> acc_Daily { get; set; }
        public DbSet<acc_CostCenter> acc_CostCenter { get; set; }
        public DbSet<acc_CreditAccount> acc_CreditAccounts { get; set; }
        public DbSet<st_screen> st_screen { get; set; }
        public DbSet<st_userPermission> st_userPermission { get; set; }
        public DbSet<st_UserCCPermission> st_UserCCPermission { get; set; }
        public DbSet<acc_BankTransfer> acc_BankTransfers { get; set; }
        public DbSet<IC_StockTransfer> IC_StockTransfers { get; set; }
        public DbSet<IC_ItemStore> IC_ItemStore { get; set; }
        public DbSet<acc_incomecash> acc_incomecash { get; set; }
        public DbSet<pr_Inspect> pr_Inspect { get; set; }
        public DbSet<pr_intrn> pr_intrns { get; set; }
        public DbSet<pr_itempurchase> pr_itempurchases { get; set; }
        public DbSet<pr_Inspect> pr_Inspects { get; set; }
        public DbSet<st_job> st_job { get; set; }

        public DbSet<Dealer> acc_Dealer { get; set; }
        public DbSet<acc_CreditAccount> acc_CreditAccount { get; set; }

        public DbSet<acc_invoiceCode> acc_invoiceCode { get; set; }
        public DbSet<ConInvoiceDetail> ConInvoiceDetail { get; set; }

        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<pr_outtrns> pr_outtrns { get; set; }

        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<hr_user> hr_user { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<acc_Daily> acc_Dailies { get; set; }
        public DbSet<st_Cheque> st_Cheques { get; set; }

        public DbSet<ConPayFor> ConPayFors { get; set; }

        public DbSet<ConInvoice> ConInvoices { get; set; }

        public DbSet<acc_ChequeRec> acc_ChequeRecs { get; set; }
        public DbSet<con_payInvoice> con_payInvoices { get; set; }

        public DbSet<acc_custody> acc_custody { get; set; }
        public DbSet<Dealer> acc_Dealers { get; set; }
        public DbSet<acc_CostCenter> acc_CostCenters { get; set; }
        public object ConPayInvoices { get; internal set; }
        public DbSet<st_Cheque> acc_Cheques { get; set; }
        public dynamic Banks { get; internal set; }

        public DbSet<pr_itempurchase> ItemPurchases { get; set; }
        public DbSet<IC_ItemStore> ItemStores { get; set; }
        public DbSet<pr_Inspect> Inspects { get; set; }
        public DbSet<acc_CostCenter> CostCenters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<pr_itempurchase>().ToTable("pr_itempurchase");
            modelBuilder.Entity<IC_ItemStore>().ToTable("IC_ItemStore");
            modelBuilder.Entity<pr_Inspect>().ToTable("pr_Inspect");
            modelBuilder.Entity<acc_CostCenter>().ToTable("acc_CostCenter");

            base.OnModelCreating(modelBuilder);
        }



    }
}
