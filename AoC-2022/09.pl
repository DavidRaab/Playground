#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use List::Util qw(sum0 any);
use Getopt::Long;

my $visualize = 0;
GetOptions(
    'v|visualize' => \$visualize,
) or die "Error in command line arguments\n";

# Creates a vector
sub vector ($x,$y) {
    return { X => $x, Y => $y };
}

# Default vectors
my $up    = vector(0,1);
my $right = vector(1,0);
my $down  = vector(0,-1);
my $left  = vector(-1,0);

# Initial state
my $head = vector(0,0);
my $tail = vector(0,0);

print visualize(8,8,$head,$tail), "\n" if $visualize;

# Position visited
my %visited;

for my $line ( <> ) {
    if ( $line =~ m/\A ([RULD]) \s+ (\d+) \Z/xms ) {
        my $dir    = $1;
        my $amount = $2;

        state $movement = {
            'R' => $right,
            'U' => $up,
            'L' => $left,
            'D' => $down,
        };

        for my $x ( 1 .. $amount ) {
            # Apply movement from line
            $head = add($head,$movement->{$dir});

            # When not adjacent then also move tail
            if ( not is_adjacent($head,$tail) ) {
                $tail = add($tail, direction($tail, $head));
            }

            # Save which positions the tail visited
            $visited{join ",", $tail->{X}, $tail->{Y}} = 1;

            # Visualize
            if ( $visualize ) {
                printf "%s\n", $dir;
                print visualize(8,8,$head,$tail), "\n";
            }
        }
    }
}

my $sum_visited = scalar keys %visited;
printf "Visited: %d\n", $sum_visited;

sub show ($vector) {
    return sprintf "(%d,%d)", $vector->{X}, $vector->{Y};
}

# check if two vectors are adjacent to each other
sub is_adjacent ($head, $tail) {
    my $diff_x = $head->{X} - $tail->{X};
    my $diff_y = $head->{Y} - $tail->{Y};
    if ( any { $diff_x == $_ } -1, 0, 1 ) {
        if ( any { $diff_y == $_ } -1, 0, 1 ) {
            return 1;
        }
    }
    return 0;
}

# Adds two vectors
sub add ($v1, $v2) {
    return {
        X => $v1->{X} + $v2->{X},
        Y => $v1->{Y} + $v2->{Y},
    };
}

sub equal ($v1,$v2) {
    return $v1->{X} == $v2->{X} && $v1->{Y} == $v2->{Y};
}

# Returns a vector that moves $from in the direction to $to
sub direction ($from, $to) {
    return vector(0,0) if equal($from,$to);

    # If in the same column
    my $hori = vector(0,0);
    $hori = $up   if $from->{Y} < $to->{Y};
    $hori = $down if $from->{Y} > $to->{Y};

    # if in the same row
    my $vertical = vector(0,0);
    $vertical = $right if $from->{X} < $to->{X};
    $vertical = $left  if $from->{X} > $to->{X};

    return add($hori,$vertical);
}

sub visualize ($width, $height, $head, $tail) {
    my @field = map {[(".") x $width]} 1 .. $height;

    $field[$head->{Y}][$head->{X}] = "H";
    $field[$tail->{Y}][$tail->{X}] = "T";

    return reverse map { join("", @$_) . "\n" } @field;
}