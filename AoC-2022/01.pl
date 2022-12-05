#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use List::Util qw(sum0);

# https://adventofcode.com/2022/day/1

# Read whole content as a single string
my $content = join "", <DATA>;

# Regex to parse content block-wise
my $block = qr/
    (                     # $1
        (?:
            ^             # Start of line ...
            \d+           # containing digits ...
            (?: \n | $ )  # upto end of line.
        )+                # one or many of them
    )
/xm;

# Parse content into a data-structure
my $idx = 1;
my %elf;
while ( $content =~ m/$block/g ) {
    my @nums = $1 =~ m/^ (\d+) $/xmg;
    $elf{$idx++} = \@nums;
}

# Data-Structur looks like
# {
#    1 => [1000,2000,3000],
#    2 => [4000],
#    3 => [5000,6000],
#    4 => [7000,8000,9000],
#    5 => [10000],
# }

# Get Elf with most calories
my ($elf, $calories) = max_by(\%elf, sub ($value) { sum0 @$value });

# Print the Result
printf "Elf %d has most calories with %d calories\n", $elf, $calories;


# A generic hash utilty function that returns the key/value of the
# highest value. value is calculated by the function $f.
# numeric ">" is used for determining highest value.
sub max_by ($hash, $f) {
    my ($max_key, $max_value) = ("", 0);
    while ( my ($key, $value) = each %$hash ) {
        my $current = $f->($value);
        if ( $current > $max_value ) {
            $max_key   = $key;
            $max_value = $current;
        }
    }
    return ($max_key, $max_value);
}

__DATA__
1000
2000
3000

4000

5000
6000

7000
8000
9000

10000