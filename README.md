# HugeNumber
An exploration of arbitrary precision and base(-2) numbers.
Most programmers are familiar with the binary system, or encoding numbers in base(2). Some may not realize that you can encode any 
number using ANY base (other than 0 or 1), even NEGATIVE bases. The mechanics is basically the same for all bases, but base(-2)
has some interesting properties.

This project demonstrates how to create and work with numbers represented in base(-2). Here numbers are encoded as an "infinite" 
(actually int.MaxValue) string of bits representing powers of (-2). As such these bit strings have a couple of interesting properties:
1. There is no "sign bit" and both positive and negative numbers can be represented.
2. Very large (Huge) numbers can be manipulated because there is no overflow or wrap-around.

Of course there are other ways to do arbitrary precision arithmetic using different basis and lists, but base(-2) offers some code
simplification.

This C# implementation is not space efficient because List<bool> stores actual bool elements, but in C++ the vector<bool> specialization
uses actual bits and may offer an actually practical mechanism.

This project if primarily for intellectual curiosity and shows how create, negate, add, subtract and compare infinite precision numbers
in base(-2).
