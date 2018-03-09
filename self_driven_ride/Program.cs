using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;


namespace self_driven_ride
{
    public class Program
    {
        internal static List<Ride> RidesBooked;
        internal static List<Car> CarsFleet;
        internal static Board CityBoard;
        internal static int T_Time;
        public static int Bonus;

        internal static string CaseFileName;
        private static string _outputFilePath;
        private static string _outputDirectory;


        public static void Main()
        {
            StartOperations();
        }

        private static string CasePath()
        {
            string path;
            //path = ".\\input\\a_example.in";
            //path = ".\\input\\b_should_be_easy.in";
            //path = ".\\input\\c_no_hurry.in";
            //path = ".\\input\\d_metropolis.in";
            path = ".\\input\\e_high_bonus.in";

            CaseFileName = Path.GetFileName(path);
            Console.WriteLine($"Case file {CaseFileName}");
            _outputDirectory = Directory.CreateDirectory($"./{CaseFileName}/").FullName;
            _outputFilePath = $"./{CaseFileName}/{CaseFileName}__.out";

            return path;
        }

        private static void LoadCaseFile()
        {
            var path = CasePath();

            if (!File.Exists(path))
            {
                Console.WriteLine($"File doesn't exists! {path}");
                return;
            }


            var tokensMain = File.ReadAllLines(path, Encoding.ASCII);

            //reading case header info
            var spaceSplitter = new[] {" "};
            var headerTokens = tokensMain[0].Split(spaceSplitter, StringSplitOptions.RemoveEmptyEntries);

            var rows = int.Parse(headerTokens[0]);
            var cols = int.Parse(headerTokens[1]);
            CityBoard = new Board(cols, rows);
            Console.WriteLine($"City {CityBoard.Rows}x{CityBoard.Columns}");

            var cars = int.Parse(headerTokens[2]);
            CarsFleet = new List<Car>(cars);
            Console.WriteLine($"Cars #{cars}");
            for (var i = 0; i < cars; i++)
            {
                CarsFleet.Add(new Car());
            }

            var rides = int.Parse(headerTokens[3]);
            RidesBooked = new List<Ride>(rides);
            Console.WriteLine($"Rides #{rides}");

            var bonus = int.Parse(headerTokens[4]);
            Bonus = bonus;
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

                var distance = Ride.DistanceGet(startPoint, destiPoint);

                //validating ride
                var strtTimeShortest = Ride.DistanceGet(new Point(0, 0), startPoint);
                var endTimeLongest = strtTimeShortest + distance;

                var isAssignableRide =
                    (endTimeLongest <=
                     T_Time); //the total time to start and finish the ride reasonable with the total time of the case
                if (!isAssignableRide)
                    continue; //this ride can't be taken anyway -> remove it totally -> check the next one

                var ride = new Ride(i - 1, startPoint, destiPoint, earlit, latest, distance);

                RidesBooked.Add(ride);
            }

            Console.WriteLine("Case file loaded successfully...");
        }

        public static void StartOperations()
        {
            LoadCaseFile();

            WeighedScoreSelection();

            Console.WriteLine();

            GenerateOuputFile();

            Console.WriteLine("+~-+-+-+-+ Finished +-+-+-+-~+");
            Console.ReadLine();

            Process.Start(@"C:\Windows\Explorer.exe", _outputDirectory);
        }

        private static void WeighedScoreSelection()
        {
            Console.WriteLine("Distributing rides amoung cars by weight..");
            var rides = RidesBooked.ToList();
            var keepLooking = true;
            while (keepLooking)
            {
                keepLooking = false;
                foreach (var car in CarsFleet)
                {
                    var validRide = new SortedDictionary<float, int>();
                    foreach (var ride in rides)
                    {
                        var isPickupableRide = Ride.IsRidePickupable(ride, car);
                        if (isPickupableRide)
                            try
                            {
                                validRide.Add(ride.Weight, ride.Index);
                            }
                            catch
                            {
                            }
                    }

                    if (validRide.Any())
                    {
                        var rideIndex = validRide.Reverse().First().Value;
                        var ride = rides.Find(x => x.Index == rideIndex);
                        car.AddRide(ride.Clone());
                        rides.Remove(ride);

                        keepLooking = true;
                    }
                }
            }

            Console.WriteLine($"Finished distributing rides, {rides.Count} left only!");
            ConsoleLineEnter();
        }

        private static void ConsoleLineEnter()
        {
            Console.WriteLine();
        }

        private static void GenerateOuputFile()
        {
            var score = 0;
            File.WriteAllText(_outputFilePath, string.Empty);
            foreach (var car in CarsFleet)
            {
                var ridesLine = string.Empty;
                foreach (var t in car.SuccessfulRides)
                {
                    ridesLine += " " + t.Index;
                    score += t.Score;
                }

                var carLine = (car.SuccessfulRides.Count) + " " + ridesLine + Environment.NewLine;
                ;
                File.AppendAllText(_outputFilePath, carLine);
            }

            Console.WriteLine($"Case score = {score}");
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
            internal int C { set; get; }
            internal int R { set; get; }

            public Point(int r, int c)
            {
                C = c;
                R = r;
            }
        }

        internal class Ride
        {
            internal int Index { get; }

            internal Point StartPoint { get; }
            internal Point DestiPoint { get; }

            internal int EarlitTime { get; }
            internal int LatestTime { get; }

            internal int Distance { get; }

            internal int Score { set; get; }
            internal int Cost { set; get; }
            internal float Weight { set; get; }

            public Ride(
                int index,
                Point startPoint,
                Point destiPoint,
                int earlitTime,
                int latestTime,
                int distance) : this(index)
            {
                StartPoint = startPoint;
                DestiPoint = destiPoint;
                EarlitTime = earlitTime;
                LatestTime = latestTime;

                Distance = distance;
            }

            public Ride(int index)
            {
                Index = index;
            }

            internal Ride Clone()
            {
                return new Ride(Index, StartPoint, DestiPoint, EarlitTime, LatestTime, Distance)
                {
                    Score = Score,
                    Cost = Cost,
                    Weight = Weight
                };
            }

            internal static (int, int, float) CostScoreWeightRide(Ride ride, Car car)
            {
                var distanceToRide = DistanceGet(car.NextFreeLocation, ride.StartPoint);
                var waitingSteps = ride.EarlitTime - car.NextFreeStep + distanceToRide;
                if (waitingSteps < 0) waitingSteps = 0;

                var cost = distanceToRide + waitingSteps + ride.Distance;

                var score = (ride.EarlitTime >= (car.NextFreeStep + distanceToRide))
                    ? ride.Distance + Bonus
                    : ride.Distance;

                var weight = ((float) score) / cost * 100;

                return (cost, score, weight);
            }

            internal static bool IsRidePickupable(Ride ride, Car car)
            {
                (var cost, var score, var weight) = CostScoreWeightRide(ride, car);
                var isPickupable = (car.NextFreeStep + cost) <= ride.LatestTime;

                if (isPickupable)
                    (ride.Cost, ride.Score, ride.Weight) = (cost, score, weight);

                return isPickupable;
            }

            internal static int DistanceGet(Point startPoint, Point destiPoint)
            {
                return Math.Abs(destiPoint.R - startPoint.R) + Math.Abs(destiPoint.C - startPoint.C);
            }
        }

        internal class Car
        {
            internal List<Ride> SuccessfulRides { set; get; }

            internal Point NextFreeLocation { set; get; }

            internal int NextFreeStep { private set; get; } = 0;

            public Car()
            {
                NextFreeLocation = new Point(0, 0);

                SuccessfulRides = new List<Ride>();
            }

            internal void AddRide(Ride ride)
            {
                SuccessfulRides.Add(ride);
                NextFreeStep += ride.Cost;
                NextFreeLocation = ride.DestiPoint;
            }
        }
    }
}