#!/usr/bin/env -S dotnet fsi

(* A Lambda can be used as a Callback

You probably already have seen that a million times. Probably
in a bad version. This usage sometimes can lead to the so called
Pyramid of Doom.

For example in JavaScript, or in general async code, it was common
to pass a callback. That gets later executes when the async completes.

In the async examples it often leads to code like:

    asyncDownload("/download/1", function(content) {
        asyncCompute(content, function(computed) {
            // Do something with content
        })
    });

But in general. It doesn't mean a callback will deeply indent. Probably
the biggest issue is, that a callback itself is no difference as any other
function-passing.

It is the intend how it used. For example "List.map" is also a higher-order
function taking a function as an argument. But the first argument is not considered
a "callback".

The purpose of a callback is often:

1. Because of timed calls. We don't know exactly when something finish or is run. And
   the user that calls that function can be notified imidiately.
2. It is often used as some kind of notification.

A Callback is nearly the same as an Event. The difference to an Event is that an Event
is a collections of callbacks. Not just a single one. An event is also not passed as an
argument. Usually an Event is public accessible and you always can add/remove a
Function/Handler to it. A callback is a function that is directly passed as an argument.

But under the hood an callback or event is just a function.

It is sometimes blurry to say what a callback is. For example in Perl we have the
following API to traverse a dictionary.

    use File::Find;

    find(sub { ... }, @dictionaries );

Here the first argument to `find()` is a function and I would declare it as a callback.

Maybe the difference is that in that case, or in general, callbacks are often used to
invoke some kind of side-effects. This means, either change something outside of the
function it is being called or the return value of the function we pass to `find`
is not used anywhere.

In that case using a callback has one big advantage over the fact that `find` would return
a list of all files.

If it would return all files, and it does recursively it must build a list of every file.
So if you have 60,000 files in a directory. You would get a array with 60,000 elements in
it. It would be time and memory consuming to build such a list. Especially time, as it means
every directory has to be traversed until you get a result. In the callback example
we can imiditaly for example just print matching files or just build a list of those
files we are really really want.

A callback is btw. pushed. So it means the function we pass to find. Is executed by find()
at the correct time. The control is in hand of find().

That is also the disadvantage. As we have no control over it. We for example could not abort
the find() operation, it runs until it finish. Ok, we could throw an Exception, but i consider this
a not good approach.

But pushing can sometimes be faster. Another approach would be a so called iterator. An
iterator again is just ... you guessed it ... a function.
*)
