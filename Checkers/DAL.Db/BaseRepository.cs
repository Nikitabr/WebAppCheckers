namespace DAL.Db;

public class BaseRepository : IBaseRepository
{
    protected readonly AppDbContext Ctx;

    public BaseRepository(AppDbContext ctx)
    {
        Ctx = ctx;
    }

    public string Name { get; set; } = "DB";
    public void SaveChanges()
    {
        Ctx.SaveChanges();
    }
}