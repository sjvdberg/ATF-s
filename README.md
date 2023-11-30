# ATF's

Contains code for linear piecewise arrival time functions (ATF).
Code was developed in Microsoft Visual Studio 2019.

The main class used is PiecewiseATF. This class contains information about a single linear piecewise ATF. The function itself is stored as a list of break points.

There are two constructors. The first is a basic constructor that just takes a list of tuples, while the second one is used to compose two different ATFs into one and will be discussed later.

The ArrivalTime method is used to determine the earliest arrival time of the function when leaving at a certain departure time. The implementation using binary search makes its complexity O(log(k)), where k is the number of break points of the ATF.

A similar DepartureTime method is also defined with the same O(log(k)) complexity. This method returns the latest possible departure time so that you arrive at a desired arrival time.

The function composition constructor takes 2 ATFs as input and constructs a new ATF which corresponds to applying the first ATF and then giving the result of that to the second ATF.
This is done by iterating through the break points of both input functions and then seeing which ones correspond to break points in the composed function. This makes the time complexity O(k + l), where k and l are the number of break points in the first and second function respectively. 

The code was manually tested using breakpoints and several edge cases such as ones that require waiting between the two functions and ones where a single breakpoint in the resulting ATF corresponds to a break point in both the first and the second ATF.
