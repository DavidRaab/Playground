#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';

# Example of Aspect-Oriented-Programming (AOP)
#
# The idea is the following. By default every function exists in something
# that is named the "Symbol Table". The symbol table in Perl is actually
# just a Hash.
#
# Whenever you acces a variable, array, hash, subroutine and so on. The symbol
# you access must be searched in the symbol table. But Perl also allows to
# read/change this symbol table at runtime.
#
# So you can for example read the current 'CODE' reference of a defined function.
# and replace that function with a new one. So you can for example, read the current
# function, do something else before it, call the original function and do something
# after the call. You not only can replace a function by a whole new one. You basically
# can "enhance" a function with any additional behaviour.
#
# This feature for example is also used in the Memoize module. You can for example
# call
#
# use Memoize;
# memoize('slow_function');
#
# and slow_function() will be replaced/added with a function that does a caching
# for you. Without that you need to implement it yourself.
#
# In Sq i use this approach to load type-checking for functions only when a module
# is loaded. So type-checking can easily enabled and disabled.

sub around($func_name, $fn) {
    no strict 'refs';
    no warnings 'redefine';
    my $orig = *{$func_name}{CODE};
    *{$func_name} = sub { $fn->($orig) };
    return;
}

sub hello() {
    print "Hello ";
}

around('main::hello', sub($orig) {
    $orig->();
    print "World!\n";
});

hello();