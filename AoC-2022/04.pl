#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;

# https://adventofcode.com/2022/day/4

while ( my $line = <DATA> ) {
    if ( $line =~ m/\A (\d+) - (\d+) , (\d+) - (\d+) \Z/x ) {
        chomp $line;
        if ( $1 >= $3 && $2 <= $4 ) {
            printf "First is contained in Second: %s\n", $line;
        }
        if ( $3 >= $1 && $4 <= $2 ) {
            printf "Second is contained in First: %s\n", $line;
        }
    }
}

__DATA__
2-4,6-8
2-3,4-5
5-7,7-9
2-8,3-7
6-6,4-6
2-6,4-8
11-20,15-18
10-20,1-40
