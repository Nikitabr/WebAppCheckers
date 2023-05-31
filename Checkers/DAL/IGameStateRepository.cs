using Domain;

namespace DAL;

public interface IGameStateRepository : IBaseRepository
{
    CheckersGameState AddState(CheckersGameState state);
    CheckersGameState? GetState(int id, int gameId);
    CheckersGameState? GetLatestStateForGame(int gameId);
}
