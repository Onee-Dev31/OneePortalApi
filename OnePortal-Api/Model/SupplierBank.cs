using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePortal_Api.Model
{
    public class SupplierBank
    {
        [Key]
        public int SupbankId { get; set; }
        public int SupplierId { get; set; }
        public  string? NameBank { get; set; }
        public  string? Branch { get; set; }
        public  string? AccountNum { get; set; }
        public  string? AccountName { get; set; }
        public  string? SupplierGroup { get; set; }
        public string? Company { get; set; }
    }
}
