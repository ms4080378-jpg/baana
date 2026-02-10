namespace elbanna.Models.ViewModels
{
    namespace elbanna.Models.ViewModels
    {
        public class ChequeVM
        {
            public int id { get; set; }
            public long chequeNo { get; set; }
            public int qty { get; set; }

            public int? dealerId { get; set; }
            public string dealer { get; set; }

            public string? notes { get; set; }
            public string? respons { get; set; }
        }
    }

}
