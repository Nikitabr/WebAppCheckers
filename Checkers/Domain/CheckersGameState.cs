namespace Domain;

public class CheckersGameState
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string SerializedGameState { get; set; } = default!;

    public int CheckersGameId { get; set; }
    public CheckersGame? CheckersGame { get; set; }
}