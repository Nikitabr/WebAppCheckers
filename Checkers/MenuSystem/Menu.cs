using System.Text.Json;
using ConsoleUI;
using Domain;
using GameBrain;

namespace MenuSystem;

public class Menu
{
    private readonly EMenuLevel _level;
    
    
    private const string ShortcutExit = "Exit";
    private const string ShortcutGoBack = "Back";
    private const string ShortcutGoToMain = "Main menu";
    private const string ShortcutGoToGameMenu = "Game menu";
    private const string ShortcutGoToGameOver = "Game Over";

    private string Title { get; set; }
    private readonly Dictionary<string, MenuItem> _menuItems = new Dictionary<string, MenuItem>();

    private CheckersBrain _brain = null!;
    private CheckersGame _game = null!;

    private readonly MenuItem _menuItemExit = new MenuItem("Exit", null);
    private readonly MenuItem _menuItemGoBack = new MenuItem("Back", null);
    private readonly MenuItem _menuItemGoToMain = new MenuItem("Main menu", null);

    public Menu(EMenuLevel level, string title, List<MenuItem> menuItems)
    {
        _level = level;
        Title = title;
        foreach (var menuItem in menuItems)
        {
            _menuItems.Add(menuItem.Title, menuItem);
        }

        if (_level == EMenuLevel.Without) {}
        else if (_level != EMenuLevel.Main)
            _menuItems.Add(_menuItemGoBack.Title, _menuItemGoBack);
        if (_level == EMenuLevel.Other)
            _menuItems.Add(_menuItemGoToMain.Title, _menuItemGoToMain);

        if (_level != EMenuLevel.Without)
        {
            _menuItems.Add(_menuItemExit.Title, _menuItemExit);
        }
    }

    public string RunMenu()
    {
        var menuDone = false;
        var userChoice = "";
        var activeLine = 0;

        do
        {
            if (_brain != null && _brain.NextMoveByBlack() && _game.Player2Type == EPlayerType.AI)
            {
                Game();
                Thread.Sleep(3000);
                _brain.MakeAMoveByAi();
                Console.Clear();
            }
            
            if (_brain != null)
            {
                if (Title is ">  Possible Moves  <" or ">  Move to  <" && _menuItems.Keys.ElementAt(activeLine) != ShortcutGoBack
                                                                       && _menuItems.Keys.ElementAt(activeLine) != ShortcutExit)
                {
                    Game(_menuItems.Keys.ElementAt(activeLine));
                }
                else
                {
                    Game();
                }
            }

            

            NormalBackground();
            
            Console.WriteLine(Title);
            Console.WriteLine("===================");
            for (int i = 0; i < _menuItems.Values.Count; i++)
            {
                if (i == activeLine)
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    NormalBackground();
                }

                if (_game is {GameWonByPlayer: { }} && _menuItems.Keys.ToList()[i] == "Make a move")
                {
                    continue;
                }
                Console.WriteLine(_menuItems.Values.ToList()[i]);
            }

            NormalBackground();

            Console.WriteLine("-------------------");
            
            
            var key = Console.ReadKey();

            switch (key.Key)
            {
                case ConsoleKey.UpArrow or ConsoleKey.W:
                {
                    activeLine--;
                    if (activeLine < 0) activeLine = _menuItems.Values.Count - 1;
                    break;
                }
                case ConsoleKey.DownArrow or ConsoleKey.S:
                {
                    activeLine++;
                    if (activeLine >= _menuItems.Values.Count) activeLine = 0;
                    break;
                }
                case ConsoleKey.Enter or ConsoleKey.Spacebar:
                    userChoice = _menuItems.Values.ToList()[activeLine].Title;
                    break;
            }


            Console.BackgroundColor = ConsoleColor.Black;
            
            Console.Clear();

            if (_menuItems.ContainsKey(userChoice))
            {
                string? runReturnValue = null;
                if (_menuItems[userChoice].MethodToRun != null)
                {
                    if (_menuItems.Keys.ToList()[activeLine].Length == 2 && _brain != null
                        && Title == ">  Possible Moves  <")
                    {
                        _brain.MovePiece = _menuItems.Keys.ToList()[activeLine];
                    }
                    else if (_menuItems.Keys.ToList()[activeLine].Length == 2 && _brain != null
                                                                              && Title == ">  Move to  <")
                    {
                        _brain.MoveTo = _menuItems.Keys.ToList()[activeLine];
                    }
                    runReturnValue = _menuItems[userChoice].MethodToRun!();
                    userChoice = "";
                }
                else
                {
                    menuDone = true;
                }

                if (userChoice == ShortcutGoBack)
                {
                    menuDone = true;
                }

                if (runReturnValue == ShortcutExit || userChoice == ShortcutExit)
                {
                    userChoice = runReturnValue ?? userChoice;
                    menuDone = true;
                }

                if ((userChoice == ShortcutGoToMain || runReturnValue == ShortcutGoToMain) && _level != EMenuLevel.Main)
                {
                    userChoice = runReturnValue ?? userChoice;
                    menuDone = true;
                }

                if ((userChoice == ShortcutGoToGameMenu || runReturnValue == ShortcutGoToGameMenu ||
                    userChoice == ShortcutGoToGameOver || runReturnValue == ShortcutGoToGameOver)
                    && _level != EMenuLevel.Game)
                {
                    userChoice = runReturnValue ?? userChoice;
                    menuDone = true;
                }
            }

        } while (menuDone == false);

        return userChoice;
    }

    private void NormalBackground()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;
    }


    public string RunGameMenu(CheckersBrain brain, CheckersGame game)
    {
        _brain = brain;
        _game = game;
        return RunMenu();
    }

    public void Game(string piece = default!)
    {
        UI.DrawGameBoard(_brain.GetBoard(), piece);
        NormalBackground();
        CheckersState? state = JsonSerializer.Deserialize<CheckersState>(_brain.GetSerializedGameState());
        if (_game.GameWonByPlayer != null)
        {
            Console.WriteLine("GAME OVER");
            var player = _brain.NextMoveByBlack() ? _game.Player1Name : _game.Player2Name;
            Console.WriteLine($"GAME WON BY {player.ToUpper()}");
        }
        else
        {
            Console.WriteLine($"{(state is {NextMoveByBlack: true} ? $"{_game.Player2Name}" : $"{_game.Player1Name}")} turn!");
        }
    }
}