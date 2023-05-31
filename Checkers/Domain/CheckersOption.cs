namespace Domain;

public class CheckersOption
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int Width { get; set; } = 8;
    public int Height { get; set; } = 8;
    public int RandomMoves { get; set; } = 0;
    public bool WhiteStarts { get; set; } = true;

    public ICollection<CheckersGame>? CheckersGames { get; set; }
    
    public override string ToString()
    {
        return $"Board: {Width}x{Height} Random: {RandomMoves} WhiteStarts:{WhiteStarts}";
    }
}