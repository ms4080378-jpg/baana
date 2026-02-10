using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("ic_item")]
public class Item
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("item")]
    public string Name { get; set; }
}
