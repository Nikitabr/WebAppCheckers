// See https://aka.ms/new-console-template for more information


using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ConsoleApp;
using ConsoleUI;
using DAL;
using DAL.Db;
using DAL.FileSystem;
using Domain;
using GameBrain;
using MenuSystem;
using Microsoft.EntityFrameworkCore;

var gameOptions = new CheckersOption();
gameOptions.Name = "Standard Game";
var gameBrain = new CheckersBrain(gameOptions, null);
const string shortcutExit = "Exit";
const string shortcutGoBack = "Back";
const string shortcutGoToMain = "Main menu";
const string shortcutGoToGameMenu = "Game menu";

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseLoggerFactory(Helpers.MyLoggerFactory)
    .UseSqlite(@"Data Source=C:\Users\nikit\RiderProjects\icd0008-2022f\Checkers\DAL.Db\app.db")
    .Options;

var ctx = new AppDbContext(options);
// apply any non-applied migrations
// ctx.Database.EnsureDeleted();
ctx.Database.Migrate();
// seed some data to db - if and when needed

IGameOptionsRepository repoFs = new GameOptionsRepositoryFileSystem();
IGameRepository gameRepoFs = new GameRepositoryFileSystem();
IGameStateRepository gameStateRepoFs = new GameStateRepositoryFileSystem();
IGameOptionsRepository repoDb = new GameOptionsRepositoryDb(ctx);
IGameRepository gameRepoDb = new GameRepositoryDb(ctx);
IGameStateRepository gameStateRepoDb = new GameStateRepositoryDb(ctx);


IGameOptionsRepository repo = repoDb;
IGameRepository gameRepo = gameRepoDb;
IGameStateRepository gameStateRepo = gameStateRepoDb;

CheckersGame checkersGame = null!;

repo.SaveGameOptions(gameOptions.Name, gameOptions);



var gameStarted = new Menu(
    EMenuLevel.Game,
    ">  Game  <",
    new List<MenuItem>()
    {
        new MenuItem("Make a move", MakeAMove),
        new MenuItem("Save the game", SaveTheGame),
    }
);

var chooseYourSide = new Menu(EMenuLevel.Without,
    ">  Who starts  <",
    new List<MenuItem>()
    {
        new MenuItem("WHITE", null),
        new MenuItem("BLACK", null),
    }
);


var chooseEnemy = new Menu(EMenuLevel.Without,
    ">  Choose Your SIDE  <",
    new List<MenuItem>()
    {
        new MenuItem("1 Player", null),
        new MenuItem("2 Players", null),
    }
);

var optionsMenu = new Menu(
    EMenuLevel.Second, ">  Checkers Options  <",
    new List<MenuItem>()
    {
         new MenuItem( "Create options", CreateGameOptions),
         new MenuItem( "List saved options", ListGameOptions),
         new MenuItem( "Delete options", DeleteGameOptions),
         new MenuItem( "Save current options", SaveGameOptions),
         new MenuItem( "Persistence method swap", SwapPersistenceEngine)
    }
);

var newGameMenu = new Menu(
    EMenuLevel.Second,
    ">  New Game  <",
    new List<MenuItem>()
    {
        new MenuItem("Standard", StandardNewGame),
        new MenuItem("New options", CustomNewGame),
        new MenuItem("Load options", LoadGameOptions),
    }
);

var mainMenu = new Menu(
    EMenuLevel.Main,
    ">  Checkers  <",
    new List<MenuItem>()
    {
        new MenuItem( "New Game", newGameMenu.RunMenu),
        new MenuItem( "Load Game", LoadGame),
        new MenuItem( "Delete Game", DeleteGame),
        new MenuItem( "Options", optionsMenu.RunMenu)
    }
);



var choice = mainMenu.RunMenu();

string StandardNewGame()
{
    var gameOptions = new CheckersOption();
    gameOptions.Name = "Standard Game";
    gameBrain = new CheckersBrain(gameOptions, null);
    return DoNewGame();
}

string CustomNewGame()
{
    CreateGameOptions();
    return DoNewGame();
}

string LoadGameOptions()
{
    var gameOptionsList = repo.GetGameOptionsList();
    var count = 1;

    List<MenuItem> menuList = new List<MenuItem>();

    foreach (var option in gameOptionsList)
    {
        menuList.Add(new MenuItem(count + " " + option, null));
        count++;
    }

    var opMenu = GetMenu(EMenuLevel.Other,
        ">  Options names  <",
        menuList);

    var op = opMenu.RunMenu();

    if (op is not (shortcutExit or shortcutGoBack or shortcutGoToMain))
    {
        gameOptions = repo.GetGameOptions(op.Substring(2));
        return DoNewGame();
    }

    return op;
}

string DoNewGame()
{
    checkersGame = new CheckersGame();
    
    checkersGame.Player1Type = EPlayerType.Human;
    checkersGame.Player2Type = chooseEnemy.RunMenu() == "1 Player" ? EPlayerType.AI : EPlayerType.Human;
    
    Console.WriteLine("First player name: ");
    var player1Name = Console.ReadLine();
    var player2Name = "AI";
    if (checkersGame.Player2Type == EPlayerType.Human)
    {
        Console.WriteLine("Second player name: ");
        player2Name = Console.ReadLine();
    }
    
    checkersGame.Player1Name = player1Name;
    checkersGame.Player2Name = player2Name;

    
    checkersGame.StartedAt = DateTime.Now;
    checkersGame.CheckersOptionId = gameOptions.Id;
    checkersGame.CheckersOption = gameOptions;

    var checkersGameState = new CheckersGameState()
    {
        CheckersGame = checkersGame,
        CheckersGameId = checkersGame.Id,
        CreatedAt = DateTime.Now,
        SerializedGameState = gameBrain.GetSerializedGameState()
    };

    checkersGame.CheckersOption.Name = checkersGame.CheckersOption.Width +
                                       "x" +
                                       checkersGame.CheckersOption.Height +
                                       "W:" +
                                       checkersGame.CheckersOption.WhiteStarts;

    checkersGame.CheckersGameStates = new List<CheckersGameState>();
    checkersGame.CheckersGameStates.Add(checkersGameState);

    return gameStarted.RunGameMenu(gameBrain, checkersGame);
}


string DeleteGame()
{
    var gamesList = gameRepo.GetAll();
    List<MenuItem> menuList = new List<MenuItem>();
    var count = 1;

    foreach (var gameName in gamesList)
    {
        menuList.Add(new MenuItem(count + " " + gameName.Player1Name + " VS " + gameName.Player2Name, null));
        count++;
    }

    var options = GetMenu(EMenuLevel.Second,
        ">  Games names  <",
        menuList);

    var optionToDelete = options.RunMenu();


    var gameToDelete = gamesList.Find(g => g.Player1Name + " VS " + g.Player2Name == optionToDelete.Substring(2));

    if (optionToDelete is not (shortcutExit or shortcutGoBack or shortcutGoToMain))
    {
        gameRepo.DeleteGame(gameToDelete);
    }
    
    return optionToDelete;
}

string SaveTheGame()
{
    gameRepo.AddGame(checkersGame);

    return "";
}

string LoadGame()
{
    var count = 1;
    var gamesList = gameRepo.GetAll();

    List<MenuItem> menuList = new List<MenuItem>();

    foreach (var gameName in gamesList)
    {
        menuList.Add(new MenuItem( count + " " + gameName.Player1Name + " VS " + gameName.Player2Name, null));
        count++;
    }

    var games = GetMenu(EMenuLevel.Other,
        ">  Games names  <",
        menuList);

    var gameChoice = games.RunMenu();


    if (gameChoice is not (shortcutExit or shortcutGoBack or shortcutGoToMain))
    {
        CheckersGame game = gamesList.Find(g => g.Player1Name + " VS " + g.Player2Name == gameChoice.Substring(2))!;
        gameOptions = game.CheckersOption;
        gameOptions!.Name = gameOptions.Name;
        gameBrain = new CheckersBrain(gameOptions, game.CheckersGameStates.Last());
        checkersGame = game;
        return gameStarted.RunGameMenu(gameBrain, checkersGame);
    }

    return gameChoice;
}

string MakeAMove()
{
    var moves = gameBrain.MarkAllPossibleMoves();

    char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    int[] numbers = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26};

    var height = gameBrain.GetBoard().Length - 1;

    List<MenuItem> menuList = new List<MenuItem>();
    
    foreach (var move in moves)
    {
        menuList.Add(new MenuItem(alpha[move.Item1] + numbers[height - move.Item2].ToString(), ChooseMove));
    }
    
    var possibleMoves = GetMenu(EMenuLevel.Second,
        ">  Possible Moves  <",
        menuList);
    
    

    return possibleMoves.RunGameMenu(gameBrain, checkersGame);
}

string ChooseMove()
{

    var boardHeight = gameBrain.GetBoard().Length;
    var activePiece = gameBrain.MovePiece;
    string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    int[] numbers = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26};
    int[] piece = new[]
    {
        boardHeight - int.Parse(activePiece.Substring(1, activePiece.Length - 1)),
        alphabet.IndexOf(activePiece.Substring(0, 1), StringComparison.Ordinal)
    };


    var posToMove = gameBrain.GetAvailablePositionsToMove(piece[1], piece[0]);
    
    List<MenuItem> menuList = new List<MenuItem>();
    
    foreach (var move in posToMove)
    {
        menuList.Add(new MenuItem(alphabet[move.Item1] + numbers[boardHeight - 1 - move.Item2].ToString(), Move));
    }
    
    var possibleMoves = GetMenu(EMenuLevel.Second,
        ">  Move to  <",
        menuList);
    
    
    
    return possibleMoves.RunGameMenu(gameBrain, checkersGame);
}


string Move()
{
    var boardHeight = gameBrain.GetBoard().Length;
    var activePiece = gameBrain.MovePiece;
    string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    int[] piece = new[]
    {
        boardHeight - int.Parse(activePiece.Substring(1, activePiece.Length - 1)),
        alphabet.IndexOf(activePiece.Substring(0, 1), StringComparison.Ordinal)
    };

    activePiece = gameBrain.MoveTo;
    int[] pieceMoveTo = new[]
    {
        boardHeight - int.Parse(activePiece.Substring(1, activePiece.Length - 1)),
        alphabet.IndexOf(activePiece.Substring(0, 1), StringComparison.Ordinal)
    };

    var yStep = pieceMoveTo[0] - piece[0];
    var xStep = pieceMoveTo[1] - piece[1];
    
    gameBrain.MakeAMoveInDirection(piece[1], piece[0], xStep, yStep);
    AddCheckersGameState();
    

    if (!gameBrain.IsGameOver()) return shortcutGoToGameMenu;
    // SaveTheGame();
    checkersGame.GameOverAt = DateTime.Now;
    checkersGame.GameWonByPlayer =
        gameBrain.NextMoveByBlack() ? checkersGame.Player1Name : checkersGame.Player2Name;
    return "Game Over";
}


string CreateGameOptions()
{
    var bWidth = 0;
    var bHeight = 0;
    do
    {
        Console.Write("Board Width(6-26)→→→: ");
        var keyW = Console.ReadLine();
        if (Int32.TryParse(keyW, out bWidth))
        {
            if (bWidth is >= 6 and <= 26) continue;
            Console.WriteLine("The value is too small or too big!");
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("What the hell man?");
            Console.WriteLine();
        }
    } while (bWidth is < 6 or > 26);

    do
    {
        Console.Write("Board Height(6-26)↑↑↑: ");
        var keyH = Console.ReadLine();
        if (int.TryParse(keyH, out bHeight))
        {
            if (bHeight is >= 6 and <= 26) continue;
            Console.WriteLine("The value is too small or too big!");
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("What the hell man?");
            Console.WriteLine();
        }
    } while (bHeight is < 6 or > 26);

    gameOptions.Height = bHeight;
    gameOptions.Width = bWidth;
    gameOptions.WhiteStarts = chooseYourSide.RunMenu() == "WHITE";
    gameBrain = new CheckersBrain(gameOptions, null);
    
    return "";
}

string SwapPersistenceEngine()
{
    repo = repo == repoDb ? repoFs : repoDb;
    gameRepo = gameRepo == gameRepoDb ? gameRepoFs : gameRepoDb;
    gameStateRepo = gameStateRepo == gameStateRepoDb ? gameStateRepoFs : gameStateRepoDb;

    Console.WriteLine("Persistence engine: " + repo.Name);

    return repo.Name;
}


string ListGameOptions()
{
    foreach (var name in repo.GetGameOptionsList())
    {
        Console.WriteLine(name);
    }

    return "";
}


string SaveGameOptions()
{
    Console.WriteLine("Options name:");
    var optionsName = Console.ReadLine();
    gameOptions.Name = optionsName;
    repo.SaveGameOptions(optionsName, gameOptions);
    return "";
}


string DeleteGameOptions()
{
    var gameOptions = repo.GetGameOptionsList();
    var count = 1;

    List<MenuItem> menuList = new List<MenuItem>();

    foreach (var option in gameOptions)
    {
        menuList.Add(new MenuItem(count + " " + option, null));
        count++;
    }

    var options = GetMenu(EMenuLevel.Other,
        ">  Options names  <",
        menuList);

    var optionToDelete = options.RunMenu();

    if (optionToDelete is not (shortcutExit or shortcutGoBack or shortcutGoToMain))
    {
        repo.DeleteGameOptions(optionToDelete.Substring(2));
    }
    
    return optionToDelete;
}


Menu GetMenu(EMenuLevel level, string name, List<MenuItem> menuItems)
{
    return new Menu(level, name, menuItems);
}


void AddCheckersGameState()
{

    var checkersGameState = new CheckersGameState()
    {
        CheckersGame = checkersGame,
        CheckersGameId = checkersGame.Id,
        CreatedAt = DateTime.Now,
        SerializedGameState = gameBrain!.GetSerializedGameState()
    };
    checkersGame.CheckersGameStates!.Add(checkersGameState);
}