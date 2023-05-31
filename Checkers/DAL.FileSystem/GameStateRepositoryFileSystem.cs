using System.Text.Json;
using Domain;

namespace DAL.FileSystem;

public class GameStateRepositoryFileSystem : IGameStateRepository
{
    
    private const string FileExtension = "json";
    private readonly string _optionsDirectory = "." + System.IO.Path.DirectorySeparatorChar + "states";


    public string Name { get; set; } = default!;
    public void SaveChanges()
    {
        throw new NotImplementedException();
    }

    public CheckersGameState AddState(CheckersGameState state)
    {
        CheckOrCreateDirectory();

        var fileContent = JsonSerializer.Serialize(state);
        File.WriteAllText(GetFileName(state.CheckersGameId + ";" + state.Id), fileContent);
        
        return state;
    }

    public CheckersGameState? GetState(int id, int gameId)
    {
        var fileContent = File.ReadAllText(GetFileName(gameId + ";" + id));
        var game = JsonSerializer.Deserialize<CheckersGameState>(fileContent);
        if (game == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }

        return game;
    }

    public CheckersGameState? GetLatestStateForGame(int gameId)
    {
        CheckOrCreateDirectory();

        var res = new List<CheckersGameState>();

        foreach (var fileName in Directory.GetFileSystemEntries(_optionsDirectory, $"{gameId}*." + FileExtension))
        {
            var fileContent = File.ReadAllText(GetFileName(Path.GetFileNameWithoutExtension(fileName)));
            res.Add(JsonSerializer.Deserialize<CheckersGameState>(fileContent)!);
        }

        return res[-1];
    }
    
    private string GetFileName(string id)
    {
        return _optionsDirectory +
               Path.DirectorySeparatorChar +
               id + "." + FileExtension;
    }
    
    private void CheckOrCreateDirectory()
    {
        if (!Directory.Exists(_optionsDirectory))
        {
            Directory.CreateDirectory(_optionsDirectory);
        }
    }
}