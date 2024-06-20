# Clean Code, Horrible Performance

This follows the [Clean Code, Horrible Performance](https://www.youtube.com/watch?v=tD5NrevFtbU)
video on YouTube. I used Perl for most code. Not all optimizations translate to Perl,
because it doesn't provide low-level types like in C or C++, also it cannot
optimize everything the same way. Still it shows different solutions and the performance
impact of it.

# 01 Classes vs 02 Hash

The first optimization in the video was about not using object-orientation and instead
use a struct with a function. The closest you can do in Perl is turn a Class just
into a Hash. Technically in the class i anyway use a Hash for its data.

While creation of objects is slower, polymorphic method calls are faster. Reason
for that is that is somewhat of its dynamic typing. In Perl i don't need a abstract base
class to make the four shapes as subtypes. I just can use the four classes directly
also put them into an array. As long they have the same interface (duck-typing)
with an area() it will work. But in Perl there is no type coercion needed. Every
class is already known and it exactly knows on every object whch area() must be called.

On the other hand the hash version with its integer type need a switch branch.

# 03 Array instead of Hash

Instead of using a Hash to store all values, we can use an Array. Arrays are faster to
create, and also accessing a value by its index is a lot faster then getting an entry
from a hash.

This is sometimes used. To be practically you can create constants that are used as
field numbers so you don't need to remember the exact index.

This is the fastest way to represent and use data, even with different shapes.

But in my benchmarks still the same speed as #1. Because this version has maybe faster
data access compared to #1, but instead it needs a switch to decide which area() should be
picked. Something that was not needed with the dynamic class solution.

# 04 Inline code

Technically the same as solution #3. But function calls are slow in Perl. When we
omit the function call, it just becomes faster. 2x faster compared to #3

# 05 bitwise & 06

instead of integer comparision we can use bitwise operators. But didn't turned out to be faster.
Neither by storing the integer in values or directly use them (omiting variable access).

# 07 Branchless

Fastest version. Its basically like solution #4. But instead of a switch (if/elseif/else)
branch we can directly use the integer-type to get an factor. This optimization
is probably hardest to-do and not always possible.

# 08 Class with Array instead of Hash

Basically same as solution #1. But instead of using a hash for the object storage it
uses an array. Array are fasterw to create, and getting something by index is also
faster.

# Fsharp

A typical F# solution is like Perl #4. This is maybe considered Clean Code in F#. But not 
in the object-oriented world.

# Summary for Perl

Using an array instead of Hash can make things a little bit faster. But overall in Perl
it doesn't matter much if we use Classes or do our own switching. At least all kinds
of polymorphic code is basically a switch branch. Just done for you, you just don't see it,
but it is still there.

You get the most performance jumps by avoiding function calls and then trying to be branchless.
