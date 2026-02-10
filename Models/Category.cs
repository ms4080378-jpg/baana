public class Category
{
    public int Id { get; set; }

    public string Name { get; set; }     // ❌ العمود ده اسمه item في SQL
    public bool IsStopped { get; set; }  // ❌ العمود ده مش موجود
}
