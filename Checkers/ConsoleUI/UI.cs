using Domain;

namespace ConsoleUI;

public class UI
{
    public static void DrawGameBoard(EGamePiece?[][] board, string activePiece)
    {
       
        var rows = board.GetLength(0);
        var cols = board[0].GetLength(0);
        int[] piece = new[] {-1, -1};
        
        if (activePiece != null)
        {
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int[] numbers = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26};
            int subNum = rows;
            int subSubNum = int.Parse(activePiece.Substring(1, activePiece.Length - 1));
            
            int num = numbers.ElementAt(subNum - subSubNum) - 1;
            int numAl = alphabet.IndexOf(activePiece.Substring(0, 1), StringComparison.Ordinal);

                piece = new[]
            {
                num,
                numAl
            };
        }

         
        for (int i = 0; i < rows; i++)
        {
            if (i == 0)
            {
                
                char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
                Console.Write("    ");
                for (int k = 0; k < cols; k++)
                {
                    Console.Write($"  {alpha[k]}  ");
                }
                Console.WriteLine();
            }
            
            for (int j = 0; j < cols; j++)
            {
                var pieceStr =
                    board[i][ j] == EGamePiece.Blank ? "     " :
                    board[i][ j] == EGamePiece.Empty ? "     " :
                    board[i][ j] == EGamePiece.Black || board[i][ j] == EGamePiece.BlackQ ? "  X  " : "  O  ";

                
                if (j == 0)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    string num = rows - i < 10 ? rows - i + "   " : rows - i + "  ";
                    Console.Write(num);
                }

                if (piece[0] == i && piece [1] == j)
                {
                    Console.BackgroundColor = ConsoleColor.Magenta;
                }
                else
                {
                    Console.BackgroundColor = board[i][ j] == EGamePiece.Blank ? ConsoleColor.DarkBlue : ConsoleColor.Black;
                }
                

                Console.Write(pieceStr);
            }

            Console.WriteLine();
        }
    }
}