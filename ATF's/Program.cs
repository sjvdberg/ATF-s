using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATF_s
{
    class Program
    {
        static void Main(string[] args)
        {
            //Define 2 functions.
            var function1 = new List<(double, double)>() { (0, 3), (5, 13), (7, 17) };
            var function2 = new List<(double, double)>() { (0, 3), (6, 10), (13, 18), (20, 23) };
            var ATF1 = new PiecewiseATF(function1);
            var ATF2 = new PiecewiseATF(function2);

            //Check Arrival times.
            var stop_time1 = ATF1.ArrivalTime(7);
            var stop_time2 = ATF1.ArrivalTime(5);

            //Check composition.
            var arrival_time1 = ATF2.ArrivalTime(stop_time1);
            var arrival_time2 = ATF2.ArrivalTime(stop_time2);

            //Compose functions.
            var ATF3 = new PiecewiseATF(ATF1, ATF2);

            //Check composition.
            double arrival_time1b = ATF3.ArrivalTime(7);
            double arrival_time2b = ATF3.ArrivalTime(5);
            ;
        }
    }

    public class PiecewiseATF
    {
        //List of tuples referencing the borders of the intervals in the function
        readonly List<(double, double)> function;


        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="function"> list of tuples in function.</param>
        public PiecewiseATF(List<(double, double)> function)
        {
            this.function = function;
        }

        /// <summary>
        /// Composes two different linear piecewise ATF's so that 
        /// the first is executed first and the second after.
        /// </summary>
        /// <param name="first">First function to compose.</param> 
        /// <param name="second">Second function to compose</param> 
        public PiecewiseATF(PiecewiseATF first, PiecewiseATF second)
        {
            List<(double, double)> func1 = first.function;
            List<(double, double)> func2 = second.function;

            //result function which we will add to.
            function = new List<(double, double)>();

            //If either function is empty the result is empty as well
            if (func2.Count == 0 | func1.Count == 0) return;
            //If the second function contains only a single value there can be at most one point in the result
            if (func2.Count == 1)
            {
                try
                {
                    double departure_time = DepartureTime(func2[0].Item1);
                    function.Add((func1[0].Item1, func2[0].Item2));
                    function.Add((departure_time, func2[0].Item2));
                }
                //If an error occurs the only value of the second function is not in range, so the result is empty.
                catch (Exception) { return; };
            }

            //s is the iterator for func2. t, which will be defined in the later loop, is the iterator for func1.
            int s = 0;

            //Discard any departure times of the second function that are not viable.
            while (func2[s + 1].Item1 < func1[0].Item2)
            {
                s++;
                if (s == func2.Count - 1) return;
            }

            //Loop invariant: func1[t].Item2 <= func2[s+1].item1.
            //t strictly increases in each iteration.
            for (var t = 0; t < func1.Count; t++)
            {
                double departure_time = func1[t].Item1;
                //Determine stop time. May include waiting time at the destination in the first iteration.
                double stop_time = Math.Max(func1[t].Item2, func2[s].Item1);

                //Derive slope of the second function.
                double slope2 = (func2[s + 1].Item2 - func2[s].Item2) / (func2[s + 1].Item1 - func2[s].Item1);
                //Derive result of the second function at the stop time.
                double arrival_time = func2[s].Item2 + slope2 * (stop_time - func2[s].Item1);
                function.Add((departure_time, arrival_time));

                //If we had to wait at the stop location we also include the later possible start time
                if (stop_time > func1[t].Item2)
                    function.Add((first.DepartureTime(func2[s].Item1), arrival_time));

                //End of first function has been reached.
                if (t == func1.Count - 1) return;

                //Inner loop ensures loop invariant for the next iteration.
                //t strictly increases in each iteration.
                while (func2[s + 1].Item1 < func1[t + 1].Item2)
                {
                    //Derive slope of the first function
                    double slope1 = (func1[t + 1].Item2 - func1[t].Item2) / (func1[t + 1].Item1 - func1[t].Item1);
                    arrival_time = func2[s + 1].Item2;
                    departure_time = (func2[s + 1].Item1 - func1[t].Item2) / slope1;
                    function.Add((departure_time, arrival_time));
                    s++;
                    //End of second function has been reached.
                    if (s == func2.Count - 1) return;
                }
                if (func2[s + 1].Item1 == func1[t + 1].Item2)
                {
                    s++;
                    //End of second function has been reached.
                    if (s == func2.Count - 1) return;
                }
            }
        }

        /// <summary>
        /// Gives the earliest possible arrival time when 
        /// leaving at a certain time.
        /// </summary>
        /// <param name="departure_time">Departure time to check.</param>
        /// <returns>Earliest possible arrival time from departure time.</returns>
        public double ArrivalTime(double departure_time)
        {
            //Check if the departure time is in the domain of the function.
            if (function.Count == 0)
                throw new Exception("function empty");
            /*
            Here we assume we assume we can not leave outside the interval.
            If you allow waiting around at the starting point it would be possible to give
            an arrival time even if the departure time is before the start of the interval.
            */
            if (departure_time < function[0].Item1 | departure_time > function[function.Count - 1].Item1)
                throw new Exception("departure time not in range");

            //Perform binary search to find the left side of interval the departure time is in.
            int left, right, middle;
            left = 0;
            right = function.Count - 1;
            while (left + 1 < right)
            {
                middle = (left + right) / 2;
                if (departure_time >= function[middle].Item1)
                    left = middle;
                else
                    right = middle;
            }
            //If there is only one moment we can leave, we already know the arrival time.
            if (function.Count == 1)
                return function[0].Item2;

            //Find departure time within interval.
            double slope = (function[right].Item2 - function[left].Item2) / (function[right].Item1 - function[left].Item1);
            return function[left].Item2 + slope * (departure_time - function[left].Item1);
        }

        /// <summary>
        /// Finds the latest departure time which lets you arrive at 
        /// the arrival time.
        /// </summary>
        /// <param name="arrival_time">arrival time we want to check.</param>
        /// <returns>Latest possible departure time.</returns>
        public double DepartureTime(double arrival_time)
        {
            //Check if the departure time is in the domain of the function.
            if (function.Count == 0)
                throw new Exception("Function empty.");
            if (arrival_time < function[0].Item2 | arrival_time > function[function.Count - 1].Item2)
                throw new Exception("Arrival time not in range.");

            //Perform binary search to find the interval the arrival time is in.
            int left, right, middle;
            left = 0;
            right = function.Count - 1;
            while (left + 1 < right)
            {
                middle = (left + right) / 2;
                if (arrival_time >= function[middle].Item2)
                    left = middle;
                else
                    right = middle;
            }
            //Find Arrival time within interval.
            double slope = (function[right].Item2 - function[left].Item2) / (function[right].Item1 - function[left].Item1);
            return (arrival_time - function[left].Item2) / slope;
        }
    }
}