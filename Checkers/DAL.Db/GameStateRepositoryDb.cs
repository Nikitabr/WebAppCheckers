using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class GameStateRepositoryDb : BaseRepository ,IGameStateRepository 
{
    
    public GameStateRepositoryDb(AppDbContext ctx) : base(ctx)
    {
    }


    public CheckersGameState AddState(CheckersGameState state)
    {
        Ctx.CheckersGameStates.Add(state);
        Ctx.SaveChanges();
        return state;
    }

    public CheckersGameState? GetState(int id, int gameId)
    {
        return Ctx.CheckersGameStates
            .Include(g => g.SerializedGameState)
            .Include(g => g.CheckersGame)
            .FirstOrDefault(s => s.Id == id && s.CheckersGameId == gameId);
    }

    public CheckersGameState? GetLatestStateForGame(int gameId)
    {
        return Ctx.CheckersGameStates
            .Include(s => s.CheckersGame)
            .Include(s => s.SerializedGameState)
            .FirstOrDefault(s => s.CheckersGameId == gameId);
    }
}