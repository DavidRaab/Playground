#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;

my $names = {
    "Frank" => 1,
    "Peter" => 1,
    "David" => 1,
};

my ($f,$d) = $names->@{"Frank", "David"};
printf "%d,%d\n", $f, $d;
