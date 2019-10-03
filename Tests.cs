using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TestMyTurtle
{

    [TestFixture]
    [Author("Richard Kiely")]
    //[Parallelizable(ParallelScope.All)] (runs faster without)
    public class TurtleTests : TestBase
    {
        //primitives
        internal const int boardCols = 4;
        internal const int boardRows = 4;
        internal List<(int, int)> mines;
        internal Board board;


        [OneTimeSetUp]
        public void Setup_Board()
        {
            //initialise board
            mines = new List<(int, int)>(1);
            board = new Board { Cols = boardCols, Rows = boardRows, Mines = mines, ExitX = boardCols, ExitY = boardRows };
        }








        [Test]
        [Category("Action Rotate")]
        [Description("Tests rotate actions are correct")]
        [TestCase(Direction.North, Direction.East)]
        [TestCase(Direction.East, Direction.South)]
        [TestCase(Direction.South, Direction.West)]
        [TestCase(Direction.West, Direction.North)]
        public void Test_Action_Rotate(Direction initialDirection, Direction rotatedDirection)
        {
            //Set up Turtle and Game
            Turtle turtle = new Turtle { Direction = initialDirection };
            Game game = new Game { Turtle = turtle };
            Action action = Action.Rotate;

            //Perform rotation
            game.Execute(action);

            //Check Turtle has rotated 90 degrees
            Assert.AreEqual(rotatedDirection, turtle.Direction);
        }


        [Test]
        [Category("Action Move")]
        [Description("Tests move actions are correct")]
        [TestCase(Direction.North, 2, 2, 2, 1, TestName = "Move North")]                
        [TestCase(Direction.East, 2, 2, 3, 2, TestName = "Move East")]              
        [TestCase(Direction.South, 2, 2, 2, 3, TestName = "Move South")]         
        [TestCase(Direction.West, 2, 2, 1, 2, TestName = "Move West")]                
        [TestCase(Direction.North, 0, 0, 0, 0, TestName = "Invalid Move North")]                 
        [TestCase(Direction.East, boardCols, 0, boardCols, 0, TestName = "Invalid Move East")]  
        [TestCase(Direction.South, 0, boardRows, 0, boardRows, TestName = "Invalid Move South")]   
        [TestCase(Direction.West, 0, 0, 0, 0, TestName = "Invalid Move West")]                 
        public void Test_Action_Move(Direction moveDirection, int startX, int startY, int endX, int endY)
        {
            //Set up Turtle and Game
            Turtle turtle = new Turtle { Direction = moveDirection, X = startX, Y = startY };
            Game game = new Game { Turtle = turtle, Board = board };
            Action action = Action.Move;

            //Perform move
            game.Execute(action);

            //Check TurtleX,Y is as expected
            Assert.Multiple(() =>
            {
                Assert.AreEqual(endX, turtle.X);
                Assert.AreEqual(endY, turtle.Y);
            });
        }



        [Test]
        [Category("Game Logic")]
        [Description("Tests turtle is not alive after mine hit")]
        public void Test_Game_Mine_Hit()
        {
            //Set up Mines, Turtle and Game
            mines.Add((boardCols - 1, boardRows - 1));
            Turtle turtle = new Turtle { Direction = Direction.South, X = boardCols - 1, Y = boardRows - 2 };
            Game game = new Game { Turtle = turtle, Board = board };
            Action action = Action.Move;

            //Perform Move onto mine square
            game.Execute(action);

            //Check turtle is not alive
            Assert.True(turtle.IsAlive, "Turtle is alive but should be dead > ${turtle.IsAlive}");
        }


        [Test]
        [Category("Game Logic")]
        [Description("Tests turtle has exited if exit found")]
        public void Test_Game_Exit_Found()
        {
            //Set up Exit, Turtle and Game
            Turtle turtle = new Turtle { Direction = Direction.South, X = board.ExitX, Y = board.ExitY - 1 };
            Game game = new Game { Turtle = turtle, Board = board };
            Action action = Action.Move;

            //Perform move onto exit square
            game.Execute(action);

            //Check Turtle exited
            Assert.True(game.TurtleExited);
        }


        [Test]
        [Category("Game Logic")]
        [Description("Tests turtle alive and not exited if not on mine or exit square after sequence")]
        public void Test_Game_Not_Mine_Not_Exit()
        {
            //Set up Exit, Turtle and Game
            Turtle turtle = new Turtle { Direction = Direction.South, X = 0, Y = 0 };
            Game game = new Game { Turtle = turtle, Board = board };
            Action action = Action.Move;

            //Perform  move
            game.Execute(action);

            //Check Turtle is alive and not exited
            Assert.Multiple(() =>
            {
                Assert.True(turtle.IsAlive);
                Assert.False(game.TurtleExited);
            });
        }



        [Test]
        [Category("Parse")]
        [Description("Tests actions are parsed in correctly")]
        public void Test_Parse_Actions()
        {
            //Setup turtle, board, game
            Turtle turtle = new Turtle { Direction = Direction.North, X = 0, Y = 0 };
            Game game = new Game { Turtle = turtle, Board = board };

            //Create actions string
            string myStringOfActions = "Rotate Move Rotate Move\nMove Rotate";
            Actions myActions = Actions.Parse(myStringOfActions);

            //Check actions parsing 
            Assert.IsTrue(myActions.Sequences.Count == 2);

            //Perform first sequence
            foreach (Action action in myActions.Sequences[0])
            {
                game.Execute(action);
            }

            //Turtle starts at (0,0) and should end up at (1,1)
            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, turtle.X);
                Assert.AreEqual(1, turtle.Y);
            });
        }


        [Test]
        [Category("Parse")]
        [Description("Tests settings are parsed in correctly")]
        public void Test_Parse_Settings()
        {
            //Create settings
            string myStringOfSettings = "0\n0\nSouth\n5\n4\n1,1 4,3\n2\n2";
            Settings mySettings = Settings.Parse(myStringOfSettings);

            //Create mines list for comparison
            List<(int, int)> myMines = new List<(int, int)> { (1, 1), (4, 3) };

            //Verify settings parsed 
            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, mySettings.InitialX);
                Assert.AreEqual(0, mySettings.InitialY);
                Assert.AreEqual(Direction.South, mySettings.InitialDirection);
                Assert.AreEqual(5, mySettings.BoardRows);
                Assert.AreEqual(4, mySettings.BoardCols);
                CollectionAssert.AreEquivalent(myMines, mySettings.Mines);
                Assert.AreEqual(2, mySettings.ExitX);
                Assert.AreEqual(2, mySettings.ExitY);
            });
        }




        /// <summary>
        /// Create game parameters for main game tests
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<TestCaseData> Games()
        {
            yield return new TestCaseData(  "0\r\n0\r\nNorth\r\n5\r\n4\r\n1,1 4,3\r\n2\r\n2",       //Settings
                                            "Rotate Move Move Move",                                //Actions(s)
                                            "Sequence 1: Still in danger!")                         //Expected Outcome(s)
                                            .SetName("Game Still in Danger");                       //Test Name

            yield return new TestCaseData(  "0\r\n0\r\nEast\r\n5\r\n4\r\n1,1 4,3\r\n2\r\n2", 
                                            "Move Rotate Move", 
                                            "Sequence 1: Mine hit!")
                                            .SetName("Game Mine Hit");

            yield return new TestCaseData(  "0\r\n0\r\nEast\r\n5\r\n4\r\n1,1 4,3\r\n2\r\n2", 
                                            "Move Move Rotate Move Move", 
                                            "Sequence 1: Success!")
                                            .SetName("Game Exit Found");

            yield return new TestCaseData(  "0\r\n0\r\nEast\r\n5\r\n4\r\n1,1 4,3\r\n2\r\n2",
                                            "Rotate Move Move Move\r\nMove Rotate Move\r\nMove Move Rotate Move Move",
                                            "Sequence 1: Still in danger!\r\nSequence 2: Mine hit!\r\nSequence 3: Success!")
                                            .SetName("Game Multi");
        }



        [Test, TestCaseSource(nameof(Games))]
        [Category("Games")]
        public async Task Test_Main(string settings, string actions, string outcomes)
        {

            string consoleOutput;

            //settings as string from TestCaseData - write to file
            //actions as string from TestCaseData - write to file 
            System.IO.File.WriteAllText(@"../../../testsettings.txt", settings);
            System.IO.File.WriteAllText(@"../../../testactions.txt", actions);

            using (StringWriter stringWriter = new StringWriter())
            {
                Console.SetOut(stringWriter);

                //run main program
                string[] args = new string[] { "../../../testsettings.txt", "../../../testactions.txt" };
                await Program.Main(args);

                consoleOutput = stringWriter.ToString();
            }

            TestContext.Out.WriteLine(consoleOutput);

            //check console output is as expected
            StringAssert.Contains(outcomes, consoleOutput);

        }



    }
}
