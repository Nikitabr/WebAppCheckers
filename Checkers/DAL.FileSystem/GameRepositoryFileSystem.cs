using System.Text.Json;
using System.Text.Json.Serialization;
using Domain;

namespace DAL.FileSystem;

public class GameRepositoryFileSystem : IGameRepository
{
    private const string FileExtension = "json";
    private readonly string _optionsDirectory = "." + System.IO.Path.DirectorySeparatorChar + "games";
    
    private readonly JsonSerializerOptions _optionsJson = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };
    
    public string Name { get; set; } = default!;

    public void SaveChanges()
    {
        throw new NotImplementedException();
    }

    public List<CheckersGame> GetAll()
    {
        CheckOrCreateDirectory();

        var res = new List<CheckersGame>();

        foreach (var fileName in Directory.GetFileSystemEntries(_optionsDirectory, "*." + FileExtension))
        {
            var fileContent = File.ReadAllText(GetFileName(Path.GetFileNameWithoutExtension(fileName)));
            res.Add(JsonSerializer.Deserialize<CheckersGame>(fileContent, _optionsJson)!);
        }

        return res;
    }

    public CheckersGame? GetGame(int? id)
    {
        var fileContent = File.ReadAllText(GetFileName(id.ToString()!));
        var game = JsonSerializer.Deserialize<CheckersGame>(fileContent, _optionsJson);
        if (game == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }

        return game;
    }

    public CheckersGame AddGame(CheckersGame game)
    {
        CheckOrCreateDirectory();
        Console.WriteLine(game.CheckersGameStates);

        var fileContent = JsonSerializer.Serialize(game, _optionsJson);
        File.WriteAllText(GetFileName(game.Player1Name + " VS " + game.Player2Name), fileContent);
        
        return game;
    }

    public void DeleteGame(CheckersGame game)
    {
        File.Delete(GetFileName(game.Player1Name + " VS " + game.Player2Name));
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