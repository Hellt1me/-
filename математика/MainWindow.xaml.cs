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
            // Инициализация колоды карт
            for (int i = 1; i <= 13; i++)
            {
                deck.AddRange(new int[] { i, i, i, i });
            }
            ShuffleDeck();
        }

        // Перемешивание колоды
        private void ShuffleDeck()
        {
            Random random = new Random();
            deck.Sort((a, b) => random.Next(-1, 2));
        }

        // Выбор случайной карты
        public int DrawCard()
        {
            if (deck.Count == 0)
                throw new InvalidOperationException("Колода пуста!");
            int card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        // Проверка и подсчет очков
        public int CalculateScore(int[,] board)
        {
            int score = 0;

            // Проверка всех строк, столбцов и диагоналей
            for (int i = 0; i < 5; i++)
            {
                score += CheckLine(board, i, 0, 1, 0); // Строка
                score += CheckLine(board, 0, i, 0, 1); // Столбец
            }
            score += CheckLine(board, 0, 0, 1, 1); // Главная диагональ
            score += CheckLine(board, 0, 4, 1, -1); // Второстепенная диагональ

            return score;
        }

        // Проверка комбинаций в линии
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

            if (count < 3) return 0; // Минимальное количество чисел для комбинации - 3

            Array.Sort(numbers, 0, count);

            // Проверяем комбинации
            if (numbers[0] == 1 && numbers[1] == 1 && numbers[2] == 1 && numbers[3] == 1)
                return 200; // 4 единицы
            if (numbers[0] == 1 && numbers[1] == 1 && numbers[2] == 13 && numbers[3] == 13)
                return 100; // 3 раза по 1 и 2 раза по 13
            if (numbers[0] == 10 && numbers[1] == 11 && numbers[2] == 12 && numbers[3] == 13 && numbers[4] == 1)
                return 150; // 1, 13, 12, 11, 10
            if (IsConsecutive(numbers, count))
                return 50; // 5 последовательных чисел

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
                    return 160; // 4 одинаковых числа
            }

            if (tripletCount == 1 && pairCount == 1)
                return 80; // 3 одинаковых числа и 2 других одинаковых
            if (tripletCount == 1)
                return 40; // 3 одинаковых числа
            if (pairCount == 2)
                return 20; // 2 пары
            if (pairCount == 1)
                return 10; // 2 одинаковых числа

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

        // Установка числа на доску
        public void SetNumber(int x, int y, int number)
        {
            if (board[x, y] != 0)
                throw new InvalidOperationException("Клетка уже занята!");
            board[x, y] = number;
        }

        // Получение текущей доски
        public int[,] GetBoard() => board;
    }
    public partial class MainWindow : Window
    {
        private GameLogic game = new GameLogic();
        private int currentNumber;
        private bool isPlayerTurn = true; // Флаг для контроля очередности ходов

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
            return null; // Если все клетки заняты
        }

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                currentNumber = game.DrawCard();
                CurrentNumberText.Text = currentNumber.ToString();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Колода пуста! Игра завершена.");
                Close();
            }
        }

        private void PlayerTurn_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlayerTurn) return; // Если сейчас не ход игрока, ничего не делаем

            // Игрок выбирает клетку через клик на кнопку
            isPlayerTurn = false; // Передаем ход компьютеру
            ComputerTurn_Click(null, null); // Вызываем ход компьютера
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button cell = sender as Button;
            if (cell == null || string.IsNullOrEmpty(cell.Name))
            {
                MessageBox.Show("Ошибка: Неверная клетка.");
                return;
            }

            string name = cell.Name;
            int row = int.Parse(name.Substring(4, 1));
            int col = int.Parse(name.Substring(5, 1));

            try
            {
                if (game.GetBoard()[row, col] != 0)
                {
                    MessageBox.Show("Эта клетка уже занята!");
                    return;
                }

                game.SetNumber(row, col, currentNumber);
                cell.Content = currentNumber.ToString();

                // Обновляем текущее число из колоды
                currentNumber = game.DrawCard();
                CurrentNumberText.Text = currentNumber.ToString();

                // Проверяем окончание игры
                if (IsGameFinished())
                {
                    MessageBox.Show($"Игра окончена! Ваш счет: {game.CalculateScore(game.GetBoard())}");
                    return;
                }

                isPlayerTurn = false; // Передаем ход компьютеру
                ComputerTurn_Click(null, null); // Вызываем ход компьютера
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool IsGameFinished()
        {
            int[,] board = game.GetBoard();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (board[i, j] == 0)
                        return false;
                }
            }
            return true;
        }

        private void ComputerTurn_Click(object sender, RoutedEventArgs e)
        {
            if (isPlayerTurn) return; // Если сейчас ход игрока, ничего не делаем

            if (IsGameFinished())
            {
                MessageBox.Show($"Игра окончена! Ваш счет: {game.CalculateScore(game.GetBoard())}");
                return;
            }

            var position = ChooseOptimalPosition(game);
            if (position != null)
            {
                int row = position.Item1;
                int col = position.Item2;

                try
                {
                    game.SetNumber(row, col, currentNumber);

                    string cellName = $"Cell{row}{col}";
                    Button cell = FindName(cellName) as Button;
                    if (cell != null)
                    {
                        cell.Content = currentNumber.ToString();
                    }

                    try
                    {
                        currentNumber = game.DrawCard();
                        CurrentNumberText.Text = currentNumber.ToString();
                    }
                    catch (InvalidOperationException ex)
                    {
                        MessageBox.Show("Колода пуста! Игра завершена.");
                        MessageBox.Show($"Итоговый счет: {game.CalculateScore(game.GetBoard())}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Нет свободных клеток!");
            }

            isPlayerTurn = true; // Возвращаем ход игроку
        }
    }
}
