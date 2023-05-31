namespace DAL;

public interface IBaseRepository
{
    public string Name { get; set; }
    void SaveChanges();
}