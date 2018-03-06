using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            //path = "./input/c_no_hurry.in";
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
                var endTimeLongest = latest - earlit + strtTimeShortest;

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

            //RidesBasedApproach();

            //CarsBasedApproach();

            Console.WriteLine();

            GenerateOuputFile();

            Console.WriteLine("+~-+-+-+-+ Finished +-+-+-+-~+");
            Console.ReadLine();
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

            // trying 1 - a & b:: with bonus
            foreach (var car in CarsFleet)
            {
                var indexRide = 0;
                while (indexRide < sortedRides.Count)
                {
                    var ride = sortedRides[indexRide];
                    var rideUnfittable = false;

                    var earlitTime = ride.TimeStartShortest;
                    var finishTime = ride.TimeFinshShortest;

                    for (var i = 0; i < car.SuccessfulRides.Count; i++)
                    {
                        (Point headingPoint, Point nextRidPoint) = GetLimitingPoints(i, car);
                        // headingPoint: point which the car will head from it before current ride
                        // nextRidPoint: point which the car will heat to it after current ride

                        var headingTime = Ride.DistanceGet(headingPoint, ride.StartPoint);
                        var toNextRideTime =
                            (nextRidPoint == null) ? -1 : Ride.DistanceGet(ride.DestiPoint, nextRidPoint);

                        var accomulatedRidesTime =
                            (i == 0) ? 0 : GetTotalTimeOfRides(car.SuccessfulRides.GetRange(0, i - 1));

                        var toThisRideTime = accomulatedRidesTime + headingTime;

                        var toNextRideAccomulatedTime = toThisRideTime + ride.Distance + toNextRideTime;

                        var canInsertRideBefore = (toThisRideTime <= ride.EarlitTime) &&
                                                  (toNextRideAccomulatedTime <= car.SuccessfulRides[i].EarlitTime);
                        if (canInsertRideBefore)
                        {
                            //this ride can be inserted before current car's ride
                            car.SuccessfulRides.Insert(i, ride);
                            sortedRides.RemoveAt(indexRide);
                        }
                        else
                        {
                            //check if ride can be fit after the current car's ride
                            accomulatedRidesTime = GetTotalTimeOfRides(car.SuccessfulRides.GetRange(0, i));

                            headingTime = Ride.DistanceGet(
                                car.SuccessfulRides[i].DestiPoint,
                                sortedRides[indexRide].StartPoint);


                        }

                        //todo assign rideUnfittable
                    }

                    if (rideUnfittable)
                        indexRide++;
                }
            }

            //trying 1 - a & b:: without bonus
        }

        private static (Point, Point) GetLimitingPoints(int i, Car car)
        {
            Point headingPoint;
            if (i == 0)
                headingPoint = new Point(0, 0);
            else
                headingPoint = car.SuccessfulRides[i - 1].DestiPoint;

            Point nextRidPoint;
            if (car.SuccessfulRides.Count == 0)
                nextRidPoint = null;
            else
            {
                if ((i + 1) == car.SuccessfulRides.Count) //this is the last ride
                    nextRidPoint = null;
                else
                    nextRidPoint = car.SuccessfulRides[i].StartPoint;
            }

            return (headingPoint, nextRidPoint);
        }


        private static int GetTotalTimeOfRides(List<Ride> rides)
        {
            var totalTime = 0;

            if (rides != null)
                if (rides.Count != 0)
                    for (var i = 0; i < rides.Count; i++)
                    {
                        var headingPoint = (i == 0) ? new Point(0, 0) : rides[i - 1].DestiPoint;

                        totalTime += Ride.DistanceGet(headingPoint, rides[i].StartPoint) + rides[i].Distance;
                    }

            return totalTime;
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