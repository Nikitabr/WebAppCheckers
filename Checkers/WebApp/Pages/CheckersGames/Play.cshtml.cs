using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.CheckersGames;

public class Play : PageModel
{

    private readonly IGameRepository _repo;


    public bool StartMove { get; set; } = true;
    public int? XStart { get; set; }
    public int? YStart { get; set; }
    
    public Play(IGameRepository repo)
    {
        _repo = repo;
    }

    public CheckersBrain Brain { get; set; } = default!;

    public CheckersGame CheckersGame { get; set; } = default!;

    public List<(int, int)> PossibleMoves { get; set; } = new List<(int, int)>();

    public int PlayerNo { get; set; }

    public async Task<IActionResult> OnGet(int? id, int? playerNo, int? xStart, int? yStart, int? xEnd, int? yEnd, bool? checkAi)
    {
        
        if (id == null)
        {
            return RedirectToPage("/Index", new {error = "No game id!"});
        }

        if (playerNo == null || playerNo.Value < 0 || playerNo.Value > 1)
        {
            return RedirectToPage("/Index", new {error = "No player no, or wrong no!"});
        }

        PlayerNo = playerNo.Value;
        // playerNo 0 - first player. first player is always white.
        // playerNo 1 - second player. second player is always black.



        XStart = xStart;
        YStart = yStart;
        
        var game = _repo.GetGame(id);

        if (game == null || game.CheckersOption == null)
        {
            return NotFound();
        }

        CheckersGame = game;

        Brain = new CheckersBrain(game.CheckersOption, game.CheckersGameStates!.LastOrDefault());

        if (xStart.HasValue && yStart.HasValue && Brain.GetBoard()[yStart.Value][xStart.Value] != EGamePiece.Empty)
        {
            StartMove = false;

            PossibleMoves = Brain.GetAvailablePositionsToMove(xStart.Value, yStart.Value);


            var a = (2,3);
            if (PossibleMoves.Contains(a))
            {
                Console.WriteLine("HUI");
            }
            
            if (xEnd.HasValue && yEnd.HasValue)
            {
                Brain.MakeAMoveInDirection(XStart!.Value, YStart!.Value, xEnd.Value - XStart.Value, yEnd.Value - YStart.Value);
                game.CheckersGameStates!.Add(
                    new CheckersGameState
                    {
                        SerializedGameState = Brain.GetSerializedGameState()
                    });
                _repo.SaveChanges();
                PossibleMoves = new List<(int, int)>();
                StartMove = true;
                if (Brain.IsGameOver())
                {
                    game.GameWonByPlayer = Brain.NextMoveByBlack() ? game.Player1Name : game.Player2Name;
                    game.GameOverAt = DateTime.Now;
                    _repo.SaveChanges();
                }
            }
        } else if (checkAi.HasValue && (playerNo == 0 && CheckersGame.Player2Type == EPlayerType.AI ||
                                        playerNo == 1 && CheckersGame.Player1Type == EPlayerType.AI))
        {
            Brain.MakeAMoveByAi();
            game.CheckersGameStates!.Add(new CheckersGameState()
            {
                SerializedGameState = Brain.GetSerializedGameState()
            });
            _repo.SaveChanges();
        }

        return Page();
    }
}