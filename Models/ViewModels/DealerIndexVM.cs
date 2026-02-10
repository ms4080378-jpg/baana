using System.Collections.Generic;
using elbanna.Models;

namespace elbanna.ViewModels
{
    public class DealerIndexVM
    {
        public Dealer Dealer { get; set; } = new Dealer();
        public List<Dealer> Dealers { get; set; } = new();
    }
}
