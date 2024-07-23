# malloc vs. GC

Some people always complain that Garbage Collection would be somehow slow and would be
obviously not good for soft real-time. Well that not quite true.

First of garbage collection is usually faster than typicall malloc/free calls in C.
Those Benchmarks should show it. allocatin a lot of objects and freeing them is
usully a lot faster in F# compared to C with malloc/free.

The reason for GC not good for games is not about the performance, its about that
a GC collects more stuff basically in a batch and then do it works. While malloc/free
always immediately does it work. So it is a matter of doing things immediately and
the work is done over the full time. vs collecting things and then doing more stuff at once.

The later, what a GC does, can yield in so called "stutter". Because a lot of time it does
nothing, and then after some time it does all it work at once. That doesn't mean a GC always
will lead to stutter. Maybe only when you start allocating millions of objects per second.

But then you will have performance problems anyway, it is not releated much of using a GC
or not. When you malloc millions of structs in C your program will slow down like hell.
This is also not better than having "stutter" from time to time.

In both cases it means you must reduce dynamic allocation. Usually that is done by preallocating
data in arrays. This can be done in either C or a managed language like C#. In both
languages either witg GC or not it means no allocation happens and no GC stutter or
slow down will happen.

So it is not about using a GC or not. It is more about how good or bad you are as a programmer.

Btw. in .Net you also can call System.GC.Collect() explicitly. This means calls to GC happen
more frequently and the work done on every GC Collect is reduced to a minimum.

C malloc() is slow because every time you call malloc() your program basically stops, ask
the kernel for memory and your program needs to wait until it gets its memory. Its like
a threading context switch.

You can avoid this by either pre-allocating data. But you know, that is basically what a
GC in a managed language does. It usually preallocates memory that then can be used freely
for all kind of objects, so it doesn't need to go through the kernel every time again and
ask for some memory.

You can do that in C too. Yeah, so you just write your own Garbage Collector and still
call it "manual memory management". Whatever that means.