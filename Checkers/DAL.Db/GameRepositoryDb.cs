using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class GameRepositoryDb : BaseRepository, IGameRepository
{
    public GameRepositoryDb(AppDbContext ctx) : base(ctx)
    {
    }

    public List<CheckersGame> GetAll()
    {
        return Ctx.CheckersGames
            .Include(o => o.CheckersOption)
            .Include(o => o.CheckersGameStates)
            .OrderBy(o => o.StartedAt)
            .ToList();
    }
    
    public CheckersGame? GetGame(int? id)
    {
        return Ctx.CheckersGames
            .Include(g => g.CheckersOption)
            .Include(g => g.CheckersGameStates)
            .FirstOrDefault(g => g.Id == id);
    }

    public CheckersGame AddGame(CheckersGame game)
    {
        if (Ctx.CheckersGames.Contains(game))
        {
            Ctx.Update(game);
        }
        else
        {
            Ctx.CheckersGames.Add(game);
        }
        Ctx.SaveChanges();
        return game;
    }

    public void DeleteGame(CheckersGame game)
    {
        Ctx.CheckersGames.Remove(game);
        Ctx.SaveChanges();
    }
}