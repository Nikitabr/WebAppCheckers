using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class GameOptionsRepositoryDb : BaseRepository, IGameOptionsRepository
{
    public GameOptionsRepositoryDb(AppDbContext ctx) : base(ctx)
    {
    }

    public List<string> GetGameOptionsList()
    {
        var res = Ctx
            .CheckersOptions
            .Include(o => o.CheckersGames)
            .OrderBy(o => o.Name)
            .ToList();

        return res.Select(o => o.Name)
            .ToList();
    }

    public CheckersOption GetGameOptions(string id) => Ctx.CheckersOptions.First(p => p.Name == id);

    public void SaveGameOptions(string id, CheckersOption option)
    {
        var optionsFromDb = Ctx.CheckersOptions.FirstOrDefault(p => p.Name == id);
        if (optionsFromDb == null)
        {
            Ctx.CheckersOptions.Add(option);
            Ctx.SaveChanges();
            return;
        }

        optionsFromDb.Name = option.Name;
        optionsFromDb.Width = option.Width;
        optionsFromDb.Height = option.Height;
        optionsFromDb.RandomMoves = option.RandomMoves;
        optionsFromDb.WhiteStarts = option.WhiteStarts;

        Ctx.SaveChanges();
    }

    public void DeleteGameOptions(string id)
    {
        var optionsFromDb = GetGameOptions(id);
        Ctx.CheckersOptions.Remove(optionsFromDb);
        Ctx.SaveChanges();
    }
}