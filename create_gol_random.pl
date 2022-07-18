#!/usr/bin/env perl
use strict;
use warnings;
use v5.10;
use Data::Dumper qw(Dumper);
use Carp qw(croak);

my $width  = 100;
my $height = 49;
my $count  = $width * $height;

my $output = "";
while (length $output < $count) {
    my $case = int (rand 40);
    
    $output .=
          $case == 0  ? 'x'
        : $case == 1  ? 'x.'
        : $case == 2  ? 'x..'
        : $case == 3  ? 'x...'
        : $case == 4  ? 'x....'
        : $case == 5  ? 'x.....'
        : $case == 6  ? 'x......'
        : $case == 7  ? 'x.......'
        : $case == 8  ? 'x........'
        : $case == 9  ? 'x.........'
        : $case == 10 ? '.........x'
        : $case == 11 ? '........x.'
        : $case == 12 ? '.......x..'
        : $case == 13 ? '......x...'
        : $case == 14 ? '.....x....'
        : $case == 15 ? '....x.....'
        : $case == 16 ? '...x......'
        : $case == 17 ? '..x.......'
        : $case == 18 ? '.x........'
        : $case == 19 ? 'x.........'
        : $case == 20 ? '.........x'
        : $case == 21 ? '........x'
        : $case == 22 ? '.......x'
        : $case == 23 ? '......x'
        : $case == 24 ? '.....x'
        : $case == 25 ? '....x'
        : $case == 26 ? '...x'
        : $case == 27 ? '..x'
        : $case == 28 ? '.x'
        : $case == 29 ? '.'
        : $case == 30 ? '..'
        : $case == 31 ? '.x.'
        : $case == 32 ? 'x..x'
        : $case == 33 ? 'x.x.x'
        : $case == 34 ? 'x....x'
        : $case == 35 ? 'x..x..x'
        : $case == 36 ? 'x......x'
        : $case == 37 ? 'x...x...x'
        : $case == 38 ? 'x........x'
        : $case == 39 ? 'x....x....x'
        :               'x..........x'
        ;
}

$output = substr $output, 0, $count;
$output =~ s/(.{$width})/$1\n/g;
print $output, "\n";
