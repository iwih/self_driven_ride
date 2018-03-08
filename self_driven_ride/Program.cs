using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private static bool _loggedCars = false;


        public static void Main()
        {
            StartOperations();
        }

        private static string CasePath()
        {
            string path;
            //path = ".\\input\\a_example.in";
            //path = ".\\input\\b_should_be_easy.in";
            path = ".\\input\\c_no_hurry.in";
            //path = ".\\input\\d_metropolis.in";
            //path = ".\\input\\e_high_bonus.in";

            CaseFileName = Path.GetFileName(path);
            Console.WriteLine($"Case file {CaseFileName}");
            _outputDirectory = Directory.CreateDirectory($"./{CaseFileName}/").FullName;
            _outputFilePath = $"./{CaseFileName}/{CaseFileName}__.out";

            return path;
        }

        private static void LoadCaseFile()
        {
            var path = string.Empty;
            path = CasePath();

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
                CarsFleet.Add(new Car(i));
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

                var endTimeShortest = strtTimeShortest + distance;
                var isBonusable = (endTimeShortest <= T_Time);

                var ride = new Ride(i - 1, startPoint, destiPoint, earlit, latest, distance, strtTimeShortest,
                    endTimeShortest, endTimeLongest, isBonusable);

                RidesBooked.Add(ride);
            }

            Console.WriteLine("Case file loaded successfully...");
        }

        public static void StartOperations()
        {
            LoadCaseFile();

            ScoreDependantSelection();

            Console.WriteLine();

            GenerateOuputFile();

            Console.WriteLine("+~-+-+-+-+ Finished +-+-+-+-~+");
            Console.ReadLine();

            Process.Start(@"C:\Windows\Explorer.exe", _outputDirectory);
        }

        private static void ScoreDependantSelection()
        {
            //sorting rides by their total score (score + bonus)
            var sortedRides = RidesBooked.OrderByDescending(ride => ride.TotalScore()).ToList();
            Console.WriteLine("Rides sorted descending");

            ConsoleLineEnter();

            Console.WriteLine("Assiging highest bonus-rides to cars..");
            //this check is at the start of time (big-bang), all the cars are at the origin point (0, 0)
            var indexFreeCar = 0;
            while (indexFreeCar < CarsFleet.Count && sortedRides.Count > 0)
            {
                sortedRides[0].BonusWillBeScored = true;
                CarsFleet[indexFreeCar].SuccessfulRides.Add(sortedRides[0]);
                sortedRides.RemoveAt(0);

                indexFreeCar++;
            }

            //now, there are the following probablities:
            //1) All cars have a ride and there is/are left ride(s)
            //   a) Some or all left rides can be fit before some or all cars' ride
            //   b) Some or all left rides can be fit after some or all cars' ride
            //   c) Mix of (a) & (b)
            //   e) Some or all left rides can't be fit neither before nor after some or all cars' ride
            //      i)  the unfittable rides are not a bigger deal -> ok
            //      ii) the unfittable rides are a bigger deal!    -> f#*k
            //
            //2) All cars have a ride and no rides left -> perfect -> done
            //3) Some of cars have rides and there is no left rides -> perfect -> done

            // this is (2 & 3) possiblities
            if (sortedRides.Count == 0) return;

            // trying 1 - a & b:: with bonus
            FitRidesWithBonus(sortedRides, true);

            //trying 1 - a & b:: without bonus
            if (sortedRides.Any())
                FitRidesWithBonus(sortedRides, false);
            Console.Read();
        }

        private static void FitRidesWithBonus(List<Ride> sortedRides, bool checkBonus)
        {
            var ridesCounter = 0;
            while (ridesCounter < sortedRides.Count)
            {
                var insertedRide = sortedRides[ridesCounter];

                var carCounter = 0;

                while (carCounter < CarsFleet.Count)
                {
                    var car = CarsFleet[carCounter];

                    var toFinishThisRideTotalTime = 0;
                    var isRideUnfittableToCar = true;
                    for (var ridesCounterInCar = 0; ridesCounterInCar < car.SuccessfulRides.Count; ridesCounterInCar++)
                    {
                        var rideCurrentCar = car.SuccessfulRides[ridesCounterInCar];
                        var isFrstRideInCar = (ridesCounterInCar == 0);

                        bool isRideFitted;
                        var toInsertedRideTotalTime = 0;
                        if (isFrstRideInCar)
                        {
                            //try to fit the inserted ride before this _first_ ride
                            toInsertedRideTotalTime = Ride.DistanceGet(new Point(0, 0), insertedRide.StartPoint);

                            isRideFitted = IsRideFittableInPlace(
                                toInsertedRideTotalTime,
                                insertedRide,
                                car.SuccessfulRides.GetRange(0, car.SuccessfulRides.Count),
                                checkBonus);

                            if (isRideFitted)
                            {
                                car.SuccessfulRides.Insert(0, insertedRide);
                                sortedRides.RemoveAt(ridesCounter);
                                isRideUnfittableToCar = false;
                                break;
                            }
                        }

                        //try to fit the inserted ride after this ride
                        var headingPoint = (isFrstRideInCar)
                            ? new Point(0, 0)
                            : car.SuccessfulRides[ridesCounterInCar - 1].DestiPoint;

                        toFinishThisRideTotalTime +=
                            Ride.DistanceGet(headingPoint,
                                rideCurrentCar.StartPoint) +
                            rideCurrentCar.Distance;

                        toInsertedRideTotalTime =
                            toFinishThisRideTotalTime +
                            Ride.DistanceGet(rideCurrentCar.DestiPoint, insertedRide.StartPoint);

                        var isLastRideInCar = ridesCounterInCar == car.SuccessfulRides.Count - 1;
                        var ridesAfterRangeCount = car.SuccessfulRides.Count - ridesCounterInCar - 1;

                        var ridesAfter = isLastRideInCar
                            ? null
                            : car.SuccessfulRides.GetRange(ridesCounterInCar + 1, ridesAfterRangeCount);

                        isRideFitted = IsRideFittableInPlace(
                            toInsertedRideTotalTime,
                            insertedRide,
                            ridesAfter,
                            checkBonus);
                        if (isRideFitted)
                        {
                            car.SuccessfulRides.Insert(ridesCounterInCar + 1, insertedRide);
                            sortedRides.RemoveAt(ridesCounter);
                            isRideUnfittableToCar = false;
                            break;
                        }
                    }

                    if (isRideUnfittableToCar)
                        carCounter++;
                    else if (!isRideUnfittableToCar && ridesCounter < sortedRides.Count)
                        insertedRide = sortedRides[ridesCounter];
                    else
                        break;
                }

                ridesCounter++;
            }
        }

        private static bool IsRideFittableInPlace(
            int totalTimeBefore,
            Ride insertedRide,
            List<Ride> ridesAfter,
            bool checkBonus)
        {
            var isRideFittable = true;
            isRideFittable &= totalTimeBefore <= insertedRide.EarlitTime;

            if (isRideFittable && ridesAfter != null)
            {
                totalTimeBefore += insertedRide.Distance;

                for (var ridesCounter = 0; ridesCounter < ridesAfter.Count; ridesCounter++)
                {
                    var rideCurrent = ridesAfter[ridesCounter];

                    if (ridesCounter == 0)
                        totalTimeBefore +=
                            Ride.DistanceGet(
                                insertedRide.DestiPoint,
                                rideCurrent.StartPoint);
                    else
                        totalTimeBefore +=
                            Ride.DistanceGet(
                                ridesAfter[ridesCounter - 1].DestiPoint,
                                rideCurrent.StartPoint);

                    isRideFittable &= (checkBonus && rideCurrent.IsBonusable)
                        ? totalTimeBefore <= rideCurrent.EarlitTime
                        : (totalTimeBefore + rideCurrent.Distance) <= rideCurrent.LatestTime;

                    if (isRideFittable)
                        totalTimeBefore += rideCurrent.Distance;
                    else
                        break;
                }
            }

            return isRideFittable;
        }

        private static void ConsoleLineEnter()
        {
            Console.WriteLine();
        }

        private static void GenerateOuputFile()
        {
            File.WriteAllText(_outputFilePath, string.Empty);
            foreach (var car in CarsFleet)
            {
                var ridesLine = string.Empty;
                foreach (var t in car.SuccessfulRides)
                {
                    ridesLine += " " + t.Index;
                }

                var carLine = (car.SuccessfulRides.Count) + " " + ridesLine + Environment.NewLine;
                File.AppendAllText(_outputFilePath, carLine);
            }
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

            internal int TimeStartShortest { get; }
            internal int TimeFinshShortest { get; }
            internal int TimeFinshLongest { get; }

            internal bool IsBonusable { set; get; }

            internal bool BonusWillBeScored { set; get; }

            public Ride(
                int index,
                Point startPoint,
                Point destiPoint,
                int earlitTime,
                int latestTime,
                int distance,
                int timeStartShortest,
                int timeFinshShortest,
                int timeFinshLongest,
                bool bonusability) : this(index)
            {
                StartPoint = startPoint;
                DestiPoint = destiPoint;
                EarlitTime = earlitTime;
                LatestTime = latestTime;

                Distance = distance;

                TimeStartShortest = timeStartShortest;
                TimeFinshShortest = timeFinshShortest;
                TimeFinshLongest = timeFinshLongest;

                IsBonusable = bonusability;
            }

            public Ride(int index)
            {
                Index = index;
            }

            internal static int DistanceGet(Point startPoint, Point destiPoint)
            {
                return Math.Abs(destiPoint.R - startPoint.R) + Math.Abs(destiPoint.C - startPoint.C);
            }

            public override string ToString()
            {
                return " " + Index.ToString();
            }

            internal int TotalScore()
            {
                var score = IsBonusable ? (Distance + Bonus) : Distance;
                return score;
            }
        }

        internal class Car
        {
            public int Index { get; }

            private Ride _rideCurrent;

            internal Ride RideCurrent
            {
                set
                {
                    if (_rideCurrent != null)
                        SuccessfulRides.Add(_rideCurrent);

                    _rideCurrent = value;

                    UpdateArrivalStatus();
                }
                get => _rideCurrent;
            }

            private void UpdateArrivalStatus()
            {
                var nullRide = RideCurrent == null;

                StartArrived = nullRide ? false : (CheckIfArrived(RideCurrent.StartPoint) || WorkingOnRide);
                DestiArrived = nullRide ? false : CheckIfArrived(RideCurrent.DestiPoint);
            }

            internal List<Ride> SuccessfulRides { set; get; }

            private Point _location;

            internal Point LocationCurrent
            {
                set
                {
                    _location = value;
                    OnCarLocationChanged();
                }
                get => _location;
            }

            internal bool WorkingOnRide { set; get; }

            internal bool DestiArrived { private set; get; }
            internal bool StartArrived { private set; get; }

            public Car(int index)
            {
                Index = index;

                LocationCurrent = new Point(0, 0);

                RideCurrent = null;

                SuccessfulRides = new List<Ride>();
                CarLocationChanged += Car_CarLocationChanged;

                if (_loggedCars)
                    CleanCarLogFile();
            }

            public void Move(Point pointDestination = null)
            {
                if (RideCurrent == null && pointDestination == null) return;

                var pointTo = pointDestination ?? (WorkingOnRide ? RideCurrent.DestiPoint : RideCurrent.StartPoint);

                var Rnew = LocationCurrent.R;
                var Cnew = LocationCurrent.C;
                if (LocationCurrent.R != pointTo.R)
                {
                    //move rows
                    var step = pointTo.R > LocationCurrent.R ? 1 : -1;
                    Rnew = LocationCurrent.R + step;
                }
                else
                {
                    //move columns
                    var step = pointTo.C > LocationCurrent.C ? 1 : -1;
                    Cnew = LocationCurrent.C + step;
                }

                LocationCurrent = new Point(Rnew, Cnew);
            }

            private bool CheckIfArrived(Point pointTo)
            {
                return (LocationCurrent.C == pointTo.C && LocationCurrent.R == pointTo.R);
            }

            public event PropertyChangedEventHandler CarLocationChanged;

            protected virtual void OnCarLocationChanged([CallerMemberName] string propertyName = null)
            {
                CarLocationChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private void Car_CarLocationChanged(object sender, PropertyChangedEventArgs e)
            {
                UpdateArrivalStatus();

                if (_loggedCars)
                    WriteCarLog();

                if (RideCurrent == null)
                    return; //no need to update anything

                if (WorkingOnRide && DestiArrived)
                    RideCurrent = null; //ride finished

                if (!WorkingOnRide && StartArrived)
                    WorkingOnRide = true; //we started ride now!
            }

            private string _logFilePath;

            private void CleanCarLogFile()
            {
                _logFilePath = Path.Combine(_outputDirectory, $"car_{Index}__.log");

                File.WriteAllText(_logFilePath, string.Empty);
                WriteCarLog();
            }

            private void WriteCarLog()
            {
                var rideIndex = RideCurrent?.Index.ToString() ?? "null";
                File.AppendAllText(_logFilePath,
                    $"({LocationCurrent.R}, {LocationCurrent.C})\tRide: {rideIndex}\tStartArrived: {StartArrived}\tDestiArrived: {DestiArrived}\n");
            }

            public bool IsFree()
            {
                return (RideCurrent == null);
            }
        }
    }
}