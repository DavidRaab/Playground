# Array Scanning or Dictionary?

Whenever we want to check something in the form "is some X inside Y" then
in Perl we use a Hash. The alternative many beginners (including me) made
is to scan an Array and check if it is inside an Array.

One of the first major performance problems i had in Perl and learned to
properly use a Hash instead. In general most performance problems i ever
had in Perl was because of ussing an Array and insead somehow switched
to a Dictionary.

But every language is different. While scanning an Array usually is
an O(N*M) algorithm and a lookup of a Hash is just O(1) there are
intelligent people complaining that this O(1) might be bigger than
O(N*M).

Also other languages have a lot of optimizations. So can be that for small N
array scanning is faster, and so on. Whatever here are just some Benchmarks
testing some stuff.
