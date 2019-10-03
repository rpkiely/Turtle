using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestMyTurtle
{
    public enum Direction
    {
        North, South, East, West
    }

    public enum Action
    {
        Move, Rotate
    }

    public class Settings
    {
        public int InitialY { get; set; }
        public int InitialX { get; set; }
        public Direction InitialDirection { get; set; }
        public int BoardRows { get; set; }
        public int BoardCols { get; set; }
        public List<(int, int)> Mines { get; set; }
        public int ExitY { get; set; }
        public int ExitX { get; set; }

        public static Settings Parse(string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return new Settings
            {
                InitialY = int.Parse(lines[0]),
                InitialX = int.Parse(lines[1]),
                InitialDirection = Enum.Parse<Direction>(lines[2]),
                BoardRows = int.Parse(lines[3]),
                BoardCols = int.Parse(lines[4]),
                Mines = lines[5].Split(' ')
                    .Select(x =>
                    {
                        var coordsText = x.Split(',');
                        return (int.Parse(coordsText[0]), int.Parse(coordsText[1]));
                    })
                    .ToList(),
                ExitY = int.Parse(lines[6]),
                ExitX = int.Parse(lines[7])
            };
        }
    }

    public class Actions
    {
        public List<List<Action>> Sequences { get; set; }

        public static Actions Parse(string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            return new Actions
            {
                Sequences = lines.Select(line => line.Split(' ').Select(Enum.Parse<Action>).ToList()).ToList()
            };

        }
    }

    public class Board
    {
        public List<(int, int)> Mines { get; set; }
        public int ExitY { get; set; }
        public int ExitX { get; set; }
        public int Cols { get; set; }
        public int Rows { get; set; }
    }

    public class Turtle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Direction Direction { get; set; }
        public bool IsAlive { get; set; } = true;
    }

    public class Game
    {
        public Board Board { get; set; }
        public Turtle Turtle { get; set; }
        public List<Action> Sequence { get; set; }
        public bool TurtleExited { get; set; } = false;

        public void Move()
        {
            //Code change by RK - added check for moving off board. Move is ignored if it moves off board.
            switch (Turtle.Direction)
            {
                case Direction.East:
                    if (Turtle.X + 1 <= Board.Cols)
                    {
                        Turtle.X++;
                    }
                    break;
                case Direction.South:
                    if (Turtle.Y + 1 <= Board.Rows)
                    {
                        Turtle.Y++;
                    }
                    break;
                case Direction.West:
                    if (Turtle.X - 1 >= 0)
                    {
                        Turtle.X--;
                    }
                    break;
                case Direction.North:
                    if (Turtle.Y - 1 >= 0)
                    {
                        Turtle.Y--;         //Code fix RK - "Turtle.X-- >> Turtle.Y--"
                    }
                    break;
            }
        }

        public void Rotate()
        {
            switch (Turtle.Direction)
            {
                case Direction.East:
                    Turtle.Direction = Direction.South;
                    break;
                case Direction.South:
                    Turtle.Direction = Direction.West;
                    break;
                case Direction.West:
                    Turtle.Direction = Direction.North;
                    break;
                case Direction.North:
                    Turtle.Direction = Direction.East;
                    break;
            }
        }

        public void Execute(Action action)
        {
            if (!Turtle.IsAlive || TurtleExited) return;

            if (action == Action.Move)
            {
                Move();
                if (Board.Mines.Contains((Turtle.X, Turtle.Y)))
                {
                    Turtle.IsAlive = false;
                }
                if (Board.ExitX == Turtle.X && Board.ExitY == Turtle.Y)
                {
                    TurtleExited = true;
                }
            }
            else
            {
                Rotate();
            }
        }
    }

    class Program
    {
        public static async Task Main(string[] args)                                            //Code Change by RK - set access to public for testing
        {
            var settings = Settings.Parse(await File.ReadAllTextAsync(args[0]));
            var actions = Actions.Parse(await File.ReadAllTextAsync(args[1]));

            var board = new Board { Cols = settings.BoardCols, Rows = settings.BoardRows, Mines = settings.Mines, ExitX = settings.ExitX, ExitY = settings.ExitY };
            foreach (var (sequence, index) in actions.Sequences.Select((a, i) => (a, i + 1)))   //Code Fix by RK - Removed ".Skip(1)" as skipped first sequence
            {
                var turtle = new Turtle { Direction = settings.InitialDirection, X = settings.InitialX, Y = settings.InitialY };
                var game = new Game { Turtle = turtle, Sequence = sequence, Board = board };
                foreach (var action in sequence)
                {
                    game.Execute(action);
                }
                if (!turtle.IsAlive)
                {
                    Console.WriteLine($"Sequence {index}: Mine hit!");
                }
                else if (game.TurtleExited)
                {
                    Console.WriteLine($"Sequence {index}: Success!");
                }
                else
                {
                    Console.WriteLine($"Sequence {index}: Still in danger!");
                }
            }
        }
    }
}
