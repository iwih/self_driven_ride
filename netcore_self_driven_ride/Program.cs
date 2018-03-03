using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace netcore_self_driven_ride
{
    public class Program
    {
        internal static List<Ride> RidesBooked;
        internal static List<Car> CarsAvailable;
        internal static Board CityBoard;
        internal static int T_Time;
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
            path = "./input/c_no_hurry.in";
            //path = ".\\input\\d_metropolis.in";
//            path = ".\\input\\e_high_bonus.in";

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
            CarsAvailable = new List<Car>(cars);
            Console.WriteLine($"Cars #{cars}");
            for (var i = 0; i < cars; i++)
            {
                CarsAvailable.Add(new Car(i));
            }

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

                var ride = new Ride(i - 1, startPoint, destiPoint, earlit, latest);

                RidesBooked.Add(ride);
            }

            Console.WriteLine("Case file loaded successfully...");
        }

        public static void StartOperations()
        {
            LoadCaseFile();

            RidesBasedApproach();

            CarsBasedApproach();

            Console.WriteLine();

            GenerateOuputFile();

            Console.WriteLine("+~-+-+-+-+ Finished +-+-+-+-~+");
            Console.ReadLine();
        }

        private static void CarsBasedApproach()
        {
            var totalTikToks = CarsAvailable.Count * RidesBooked.Count;
            for (var i = 0; i < CarsAvailable.Count; i++)
            {
                var car = CarsAvailable[i];
                for (var tikTok = 1; tikTok <= T_Time; tikTok++)
                {
                    //first layer looks for bonused rides
                    for (var rideCounter = 0; rideCounter < RidesBooked.Count; rideCounter++)
                    {
                        var rideCurrent = RidesBooked[rideCounter];

                        if (car.RideCurrent != null) continue; // skip this ride

                        var distCar = Ride.DistanceGet(car.LocationCurrent, rideCurrent.StartPoint);

                        var timeToTik = tikTok - 1 + distCar;

                        if (timeToTik == rideCurrent.EarlitTime)
                        {
                            car.RideCurrent = rideCurrent;
                            RidesBooked.Remove(rideCurrent);
                            break;
                        }
                    }

                    //second layer looks for scored rides
                    for (var rideCounter = 0; rideCounter < RidesBooked.Count; rideCounter++)
                    {
                        var rideCurrent = RidesBooked[rideCounter];

                        if (car.RideCurrent != null) continue; // skip this ride

                        var distToStart = Ride.DistanceGet(car.LocationCurrent, rideCurrent.StartPoint);
                        var distToDesti = Ride.DistanceGet(car.LocationCurrent, rideCurrent.DestiPoint);

                        var timeToTik = tikTok - 1 + distToStart + distToDesti;

                        if (timeToTik <= rideCurrent.LatestTime)
                        {
                            car.RideCurrent = rideCurrent;
                            RidesBooked.Remove(rideCurrent);
                            break;
                        }
                    }

                    var currentTikTok = i * tikTok;
                    car.Move();
                    Console.Write(
                        $"\rProgress: {currentTikTok} - {totalTikToks}, Percentage: {(float) currentTikTok / totalTikToks * 100} %                         ");
                }
            }
        }

        private static void RidesBasedApproach()
        {
            for (var tikTok = 1; tikTok <= T_Time; tikTok++)
            {
                var indexInvokedCars = new List<int>();

                //first layer looks for bonused rides
                for (var rideCounter = 0; rideCounter < RidesBooked.Count; rideCounter++)
                {
                    var rideCurrent = RidesBooked[rideCounter];

                    for (var carCounter = 0; carCounter < CarsAvailable.Count; carCounter++)
                    {
                        var carCurrent = CarsAvailable[carCounter];
                        if (carCurrent.RideCurrent != null) continue; // skip this car

                        var distCar = Ride.DistanceGet(carCurrent.LocationCurrent, rideCurrent.StartPoint);

                        var timeToTik = tikTok - 1 + distCar;

                        if (timeToTik == rideCurrent.EarlitTime)
                        {
                            carCurrent.RideCurrent = rideCurrent;
                            RidesBooked.Remove(rideCurrent);
                            carCurrent.Move();
                            indexInvokedCars.Add(carCounter);
                            break;
                        }
                    }
                }

                //second layer looks for scored rides
                for (var rideCounter = 0; rideCounter < RidesBooked.Count; rideCounter++)
                {
                    var rideCurrent = RidesBooked[rideCounter];

                    for (var carCounter = 0; carCounter < CarsAvailable.Count; carCounter++)
                    {
                        var carCurrent = CarsAvailable[carCounter];
                        if (carCurrent.RideCurrent != null) continue; // skip this car

                        var distToStart = Ride.DistanceGet(carCurrent.LocationCurrent, rideCurrent.StartPoint);
                        var distToDesti = Ride.DistanceGet(carCurrent.LocationCurrent, rideCurrent.DestiPoint);

                        var timeToTik = tikTok - 1 + distToStart + distToDesti;

                        if (timeToTik <= rideCurrent.LatestTime)
                        {
                            carCurrent.RideCurrent = rideCurrent;
                            RidesBooked.Remove(rideCurrent);
                            carCurrent.Move();
                            indexInvokedCars.Add(carCounter);
                            break;
                        }
                    }
                }

                for (var i = 0; i < CarsAvailable.Count; i++)
                {
                    var car = CarsAvailable[i];
                    if (car.RideCurrent == null) continue;

                    if (!indexInvokedCars.Contains(i))
                        car.Move();
                }

                Console.Write(
                    $"\rProgress: {tikTok} - {T_Time}, Percentage: {(float) tikTok / T_Time * 100} %                         ");
            }
        }

        private static void GenerateOuputFile()
        {
            File.WriteAllText(_outputFilePath, string.Empty);
            foreach (var car in CarsAvailable)
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

            public Ride(int index, Point startPoint, Point destiPoint, int earlitTime, int latestTime) : this(index)
            {
                StartPoint = startPoint;
                DestiPoint = destiPoint;
                EarlitTime = earlitTime;
                LatestTime = latestTime;

                Distance = DistanceGet(startPoint, destiPoint);
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
        }
    }
}
