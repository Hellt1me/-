using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace математика
{
    public class GameLogic
    {
        private List<int> deck = new List<int>();
        private int[,] board = new int[5, 5];

        public GameLogic()
        {
            for (int i = 1; i <= 13; i++)
            {
                deck.AddRange(new int[] { i, i, i, i });
            }
            ShuffleDeck();
        }

        private void ShuffleDeck()
        {
            Random random = new Random();
            deck.Sort((a, b) => random.Next(-1, 2));
        }

        public int DrawCard()
        {
            if (deck.Count == 0)
                throw new InvalidOperationException("колода пуста");
            int card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        public int CalculateScore(int[,] board)
        {
            int score = 0;

            for (int i = 0; i < 5; i++)
            {
                score += CheckLine(board, i, 0, 1, 0); 
                score += CheckLine(board, 0, i, 0, 1); 
            }
            score += CheckLine(board, 0, 0, 1, 1); 
            score += CheckLine(board, 0, 4, 1, -1); 

            return score;
        }

        private int CheckLine(int[,] board, int startX, int startY, int dx, int dy)
        {
            int[] numbers = new int[5];
            int count = 0;

            for (int i = 0; i < 5; i++)
            {
                int x = startX + i * dx;
                int y = startY + i * dy;

                if (x < 0 || x >= 5 || y < 0 || y >= 5 || board[x, y] == 0)
                    break;

                numbers[count++] = board[x, y];
            }

            if (count < 3) return 0; 

            Array.Sort(numbers, 0, count);


            if (numbers[0] == 1 && numbers[1] == 1 && numbers[2] == 1 && numbers[3] == 1)
                return 200; 
            if (numbers[0] == 1 && numbers[1] == 1 && numbers[2] == 13 && numbers[3] == 13)
                return 100; 
            if (numbers[0] == 10 && numbers[1] == 11 && numbers[2] == 12 && numbers[3] == 13 && numbers[4] == 1)
                return 150; 
            if (IsConsecutive(numbers, count))
                return 50; 

            Dictionary<int, int> counts = new Dictionary<int, int>();
            for (int i = 0; i < count; i++)
            {
                if (!counts.ContainsKey(numbers[i]))
                    counts[numbers[i]] = 0;
                counts[numbers[i]]++;
            }

            int pairCount = 0;
            int tripletCount = 0;
            foreach (var countValue in counts.Values)
            {
                if (countValue == 2)
                    pairCount++;
                else if (countValue == 3)
                    tripletCount++;
                else if (countValue == 4)
                    return 160; 
            }

            if (tripletCount == 1 && pairCount == 1)
                return 80; 
            if (tripletCount == 1)
                return 40; 
            if (pairCount == 2)
                return 20; 
            if (pairCount == 1)
                return 10; 

            return 0;
        }

        private bool IsConsecutive(int[] numbers, int count)
        {
            for (int i = 1; i < count; i++)
            {
                if (numbers[i] - numbers[i - 1] != 1)
                    return false;
            }
            return true;
        }

        public void SetNumber(int x, int y, int number)
        {
            if (board[x, y] != 0)
                throw new InvalidOperationException("клетка занята");
            board[x, y] = number;
        }

        public int[,] GetBoard() => board;
    }
    public partial class MainWindow : Window
    {
        private GameLogic game = new GameLogic();
        private int currentNumber;
        private bool isPlayerTurn = true; 

        public Tuple<int, int> ChooseOptimalPosition(GameLogic game)
        {
            int[,] board = game.GetBoard();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (board[i, j] == 0)
                        return Tuple.Create(i, j);
                }
            }
            return null; 
        }

        private void PlayerTurn_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlayerTurn) return; 

            isPlayerTurn = false; 
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button cell = sender as Button;
            if (cell == null || string.IsNullOrEmpty(cell.Name))
            {
                MessageBox.Show("ошибка");
                return;
            }

            string name = cell.Name;
            int row = int.Parse(name.Substring(4, 1));
            int col = int.Parse(name.Substring(5, 1));

            try
            {
                if (game.GetBoard()[row, col] != 0)
                {
                    MessageBox.Show("клетка занята");
                    return;
                }

                game.SetNumber(row, col, currentNumber);
                cell.Content = currentNumber.ToString();

                currentNumber = game.DrawCard();
                CurrentNumberText.Text = currentNumber.ToString();

                isPlayerTurn = false; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
