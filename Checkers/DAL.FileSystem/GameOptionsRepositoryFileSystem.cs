﻿using System.Text.Json;
using Domain;

namespace DAL.FileSystem;

public class GameOptionsRepositoryFileSystem : IGameOptionsRepository
{
    private const string FileExtension = "json";
    private readonly string _optionsDirectory = "." + System.IO.Path.DirectorySeparatorChar + "options";

    public string Name { get; set; } = "FS";

    public void SaveChanges()
    {
        throw new NotImplementedException("File system is updated immediately!");
    }

    public List<string> GetGameOptionsList()
    {
        CheckOrCreateDirectory();

        var res = new List<string>();

        foreach (var fileName in Directory.GetFileSystemEntries(_optionsDirectory, "*." + FileExtension))
        {
            Console.WriteLine(fileName);
            res.Add(Path.GetFileNameWithoutExtension(fileName));
        }

        return res;
    }

    public CheckersOption GetGameOptions(string id)
    {
        var fileContent = File.ReadAllText(GetFileName(id));
        var options = JsonSerializer.Deserialize<CheckersOption>(fileContent);
        if (options == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }

        return options;
    }

    public void SaveGameOptions(string id, CheckersOption option)
    {
        CheckOrCreateDirectory();

        var fileContent = JsonSerializer.Serialize(option);
        File.WriteAllText(GetFileName(id), fileContent);
    }

    public void DeleteGameOptions(string id)
    {
        File.Delete(GetFileName(id));
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