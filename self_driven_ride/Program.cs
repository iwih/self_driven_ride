using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace self_driven_ride
{
    public class Program
    {
        internal static List<Ride> RidesBooked;
        internal static List<Car> CarsAvailable;
        internal static Board CityBoard;
        internal static int T_Time;
        internal static string CaseFileName;

        public static void Main()
        {
            var threadStart = new ThreadStart(StartOperations);
            var thread = new Thread(threadStart);

            thread.Start();
        }

        public static void StartOperations()
        {
            LoadCaseFile();

            Console.ReadLine();
        }

        private static void LoadCaseFile()
        {
            var path = string.Empty;
            path = ".\\input\\a_example.in";
            //path = ".\\input\\b_should_be_easy.in";
            //path = ".\\input\\c_no_hurry.in";
            //path = ".\\input\\d_metropolis.in";
            //path = ".\\input\\e_high_bonus.in";

            if (!File.Exists(path))
            {
                Console.WriteLine($"File doesn't exists! {path}");
                return;
            }

            CaseFileName = Path.GetFileName(path);
            Console.WriteLine($"Case file {CaseFileName}");

            var tokensMain = File.ReadAllLines(path, Encoding.ASCII);

            //reading case header info
            var spaceSplitter = new[] {" "};
            var headerTokens = tokensMain[0].Split(spaceSplitter, StringSplitOptions.RemoveEmptyEntries);

            var rows = int.Parse(headerTokens[0]);
            var cols = int.Parse(headerTokens[1]);
            CityBoard = new Board(cols, rows);
            Console.WriteLine($"City {CityBoard.Rows}x{CityBoard.Columns}");

            var cars = int.Parse(headerTokens[2]);
            CarsAvailable = new List<Car>(cars);
            Console.WriteLine($"Cars #{cars}");

            var rides = int.Parse(headerTokens[3]);
            RidesBooked = new List<Ride>(rides);
            Console.WriteLine($"Rides #{rides}");

            var bonus = int.Parse(headerTokens[4]);
            Console.WriteLine($"Bouns is {bonus} Yay!");

            var steps = int.Parse(headerTokens[5]);
            T_Time = steps;
            Console.WriteLine($"T = {T_Time}");

            for (var i = 1; i < tokensMain.Length; i++)
            {
                var rideTokens = tokensMain[i].Split(spaceSplitter, StringSplitOptions.None);

                var startPoint = new Point(int.Parse(rideTokens[0]), int.Parse(rideTokens[1]));
                var destiPoint = new Point(int.Parse(rideTokens[2]), int.Parse(rideTokens[3]));

                var earlit = int.Parse(rideTokens[4]);
                var latest = int.Parse(rideTokens[5]);

                var ride = new Ride(startPoint, destiPoint, earlit, latest);

                RidesBooked.Add(ride);
            }

            Console.WriteLine("Case file loaded successfully...");
        }


        internal class Board
        {
            internal int Columns { get; }
            internal int Rows { get; }
            internal int C_higest { get; }
            internal int R_higest { get; }

            public Board(int rows, int columns)
            {
                Columns = columns;
                Rows = rows;
                C_higest = Columns - 1;
                R_higest = Rows - 1;
            }
        }

        internal class Point
        {
            internal int C { get; }
            internal int R { get; }

            public Point(int r, int c)
            {
                C = c;
                R = r;
            }
        }

        internal class Ride
        {
            internal Point StartPoint { get; }
            internal Point DestiPoint { get; }

            internal int EarlitTime { get; }
            internal int LatestTime { get; }

            internal int Distance { get; }

            public Ride(Point startPoint, Point destiPoint, int earlitTime, int latestTime)
            {
                StartPoint = startPoint;
                DestiPoint = destiPoint;
                EarlitTime = earlitTime;
                LatestTime = latestTime;

                Distance = (DestiPoint.R - StartPoint.R) + (DestiPoint.C - DestiPoint.C);
            }
        }

        internal class Car
        {
            internal Ride RideCurrent { set; get; } = null;
            internal List<Ride> SuccessfulRides { get; }
            internal Point LocationCurrent { set; get; }

            public Car(Point locationCurrent)
            {
                LocationCurrent = locationCurrent;

                SuccessfulRides = new List<Ride>();
            }
        }
    }
}