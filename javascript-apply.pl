#!/usr/bin/env perl
use strict;
use warnings;
use v5.32;
use Data::Dump qw(dump);
use List::Util qw(max);

sub show {
    my ($num) = @_;
    printf "%d\n", $num;
}

# Math.max(5,6,2,3,7)
show( max(5,6,2,3,7) ); # 7

# let nums = [5,6,2,3,7]
#
# This dosn't work:
# Math.max(nums)
#
# You must do
# Math.max.apply(null, nums)

my @nums = (5,6,2,3,7);
show( max(@nums) );

# Perl differentiates between an array and reference to an array
# By default perl expands an array
# if you want a reference you must write \@nums
# Other languages like JavaScript, Python, Java, C#, F#, ... only know "by reference"

# But you can work with reference only, also in Perl
my $nums = [5,6,2,3,7];

# Then you must expand the reference by putting an @ before the variable
show( max(@$nums) );

# So in generel. There are always Pro/Contra in Language Design!
# There is no "better" or "worse".

# But JavaScript could have elimanated that problem if the function expected
# an array as a first argument, instead of a variable argument list.
# This is what you see in F#, so there you don't have the problem alltogether.
