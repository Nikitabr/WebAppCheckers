using System.Text.Json;
using System.Text.RegularExpressions;
using Domain;

namespace GameBrain;

public class CheckersBrain
{
    private readonly CheckersState _state;
    public string MovePiece = default!;
    public string MoveTo = default!;
    private List<(int, int)> _availablePosToMove = new List<(int, int)>();
    private List<(int, int)> _availableMoveToEat = new List<(int, int)>();
    private bool _searchingForAvailablePos = false;

    public CheckersBrain(CheckersOption option, CheckersGameState? state)
    {

        if (state == null)
        {
            _state = new CheckersState();

            InitializeNewGame(option);
        }
        else
        {
            _state = System.Text.Json.JsonSerializer.Deserialize<CheckersState>(state.SerializedGameState)!;
        }

    }

    public string GetSerializedGameState()
    {
        return System.Text.Json.JsonSerializer.Serialize(_state);
    }

    public void InitializeNewGame(CheckersOption option)
    {
        var boardWidth = option.Width;
        var boardHeight = option.Height;

        if (boardWidth < 4 || boardHeight < 4)
        {
            throw new ArgumentException("Board size too small");
        }

        _state.GameBoard = new EGamePiece?[boardHeight][];
        for (int i = 0; i < boardHeight; i++)
        {
            _state.GameBoard[i] = new EGamePiece?[boardWidth];
        }

        _state.NextMoveByBlack = option.WhiteStarts != true;


        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                _state.GameBoard[i][j] =
                    i % 2 == 0
                        ?
                        j % 2 == 1
                            ? i < boardHeight / 2 - 1 ? EGamePiece.Black :
                            i > Math.Round((double) boardHeight / 2) ? EGamePiece.White : EGamePiece.Empty
                            : EGamePiece.Blank
                        : j % 2 == 0
                            ? i < boardHeight / 2 - 1 ? EGamePiece.Black :
                            i > (boardHeight + 1) / 2 ? EGamePiece.White : EGamePiece.Empty
                            : EGamePiece.Blank
                    ;
            }
        }
    }

    public EGamePiece?[][] GetBoard()
    {
        var jsonStr = JsonSerializer.Serialize(_state.GameBoard);
        return JsonSerializer.Deserialize<EGamePiece?[][]>(jsonStr)!;
    }

    public List<(int, int)> MarkAllPossibleMoves(bool aiMove = false)
    {
        _availablePosToMove = new List<(int, int)>();
        _availableMoveToEat = new List<(int, int)>();

        for (int i = 0; i < _state.GameBoard.Length; i++)
        {
            for (int j = 0; j < _state.GameBoard[0].Length; j++)
            {
                if (IsMovePossible(j, i, aiMove))
                {
                    _availablePosToMove.Add((j, i));
                }
                
            }
        }

        return _availableMoveToEat.Any() ? _availableMoveToEat : _availablePosToMove;
    }
    
    public bool IsMovePossible(int x, int y, bool aiMove = false)
    {
        if (_state.GameBoard[y][x] == EGamePiece.Empty) return false;
        if (_state.NextMoveByBlack && _state.GameBoard[y][x] is EGamePiece.White or EGamePiece.WhiteQ) return false;
        if (!_state.NextMoveByBlack && _state.GameBoard[y][x] is EGamePiece.Black or EGamePiece.BlackQ) return false;

        int moveByYDown;
        int moveByYUp;

        bool res = false;

        var gamePiece = _state.GameBoard[y][x];
        
        if (gamePiece == EGamePiece.Black)
        {
            moveByYDown = 1;
            moveByYUp = 1;
        }
        else if (gamePiece == EGamePiece.White)
        {
            moveByYDown = -1;
            moveByYUp = -1;
        } else
        {
            moveByYDown = -1;
            moveByYUp = 1;
        }
        
        for (int xStep = -1; xStep <= 1; xStep+=2)
        {
            for (int yStep = moveByYDown; yStep <= moveByYUp; yStep+=2)
            {
                if (IsMovePossibleInDirection(x, y, xStep, yStep, aiMove))
                {
                    res = true;
                }
            }
        }

        return res;
    }

    private bool IsMovePossibleInDirection(int x, int y, int xStep, int yStep, bool aiMove)
    {
        
        var boardHeight = _state.GameBoard.Length;
        var boardWidth = _state.GameBoard[0].Length;

        var currentX = x;
        var currentY = y;

        var black = new List<EGamePiece>() {EGamePiece.Black, EGamePiece.BlackQ};
        var white = new List<EGamePiece>() {EGamePiece.White, EGamePiece.WhiteQ};

        var actPiece = _state.GameBoard[y][x];
        
        var enemyPiece = (actPiece == white[0] || actPiece == white[1]) ? black : white;


        currentX += xStep;
        currentY += yStep;

        // are we over the edge? if so - we are done, move is not possible
        if (currentX < 0 || currentY < 0 || currentX == boardWidth || currentY == boardHeight)
        {
            return false;
        }

        var curPiece = _state.GameBoard[currentY][currentX];
        
        
        
        if(curPiece == enemyPiece[0] || curPiece == enemyPiece[1])
        {
            var nextY = currentY + yStep;
            var nextX = currentX + xStep;
            
            if (nextX < 0 || nextY < 0 || nextX == boardWidth || nextY == boardHeight)
            {
                return false;
            }
            
            var check = _state.GameBoard[nextY][nextX];
            
            
            
            if (check == EGamePiece.Empty)
            {
                if (aiMove && _searchingForAvailablePos)
                {
                    _availableMoveToEat.Add((nextX, nextY));
                }
                else if (aiMove)
                {
                    _availableMoveToEat.Add((x, y));
                }
                else if (_searchingForAvailablePos)
                {
                    _availablePosToMove.Add((nextX, nextY));
                }


                return true;
            }
        }


        if (curPiece != EGamePiece.Empty) return false;
        if (_searchingForAvailablePos)
        {
            _availablePosToMove.Add((currentX, currentY));
        }
        return true;

    }

    public bool NextMoveByBlack() => _state.NextMoveByBlack;

    public void MakeAMoveInDirection(int x, int y, int xStep, int yStep)
    {
        var currentX = x;
        var currentY = y;

        var curPiece = _state.GameBoard[y][x];
    
        currentX += xStep;
        currentY += yStep;
        
        
        if (Math.Abs(xStep) == 2)
        {
            _state.GameBoard[y + yStep / 2][x + xStep / 2] = EGamePiece.Empty;
        }

        _state.GameBoard[currentY][currentX] = _state.NextMoveByBlack ? 
            currentY == _state.GameBoard.Length - 1 ? EGamePiece.BlackQ : curPiece :
            currentY == 0 ? EGamePiece.WhiteQ : curPiece;
        _state.GameBoard[y][x] = EGamePiece.Empty;
        _state.NextMoveByBlack = !_state.NextMoveByBlack;

        if (!IsGameOver() && !MarkAllPossibleMoves().Any())
        {
            _state.NextMoveByBlack = !_state.NextMoveByBlack;
        }
    }

    public void MakeAMoveByAi()
    {
        var rnd = new Random();
        var possibleMoves = MarkAllPossibleMoves(true);

        if (possibleMoves.Count > 0)
        {
            var (x, y) = possibleMoves[rnd.Next(0, possibleMoves.Count)];
            var possiblePieceMoves = GetAvailablePositionsToMove(x, y, true);
            var (xEnd, yEnd) = possiblePieceMoves[rnd.Next(0, possiblePieceMoves.Count)];
            MakeAMoveInDirection(x, y, xEnd - x, yEnd - y);
        }
    }

    public List<(int, int)> GetAvailablePositionsToMove(int x, int y, bool aiMove = false)
    {
        _searchingForAvailablePos = true;
        _availablePosToMove = new List<(int, int)>();
        _availableMoveToEat = new List<(int, int)>();

        IsMovePossible(x, y, aiMove);

        _searchingForAvailablePos = false;
        return _availableMoveToEat.Any() ? _availableMoveToEat : _availablePosToMove;
    }

    public bool IsGameOver()
    {
        bool white = true;
        bool black = true;

        foreach (var row in _state.GameBoard)
        {
            foreach (var piece in row)
            {
                if (piece is (EGamePiece.White or EGamePiece.WhiteQ))
                {
                    white = false;
                }

                if (piece is (EGamePiece.Black or EGamePiece.BlackQ))
                {
                    black = false;
                }
            }
        }

        if(white || black)
        {
            return true;
        }

        return false;

    }

    public int GetPiecesCount(bool getWhite = true)
    {
        var white = 0;
        var black = 0;
        
        foreach (var row in _state.GameBoard)
        {
            foreach (var piece in row)
            {
                if (piece is (EGamePiece.White or EGamePiece.WhiteQ))
                {
                    white++;
                }

                if (piece is (EGamePiece.Black or EGamePiece.BlackQ))
                {
                    black++;
                }
            }
        }

        return getWhite ? white : black;
    }
}