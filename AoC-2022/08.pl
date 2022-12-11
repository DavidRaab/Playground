#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use List::Util qw(max sum0);

# read file as Array of Array
my @forest = map {[grep { not m/\A\s+\z/ } split //]} <>;
my $width  = max map { scalar @$_ } @forest;
my $height = @forest;

printf "W: %d H: %d\n", $width, $height;

print_aoa(\@forest);

my @seen;
for (my $y=0; $y < $height; $y++) {
    my @result;
    for (my $x=0; $x < $width; $x++) {
        push @result, can_be_seen($x,$y,\@forest);
    }
    push @seen, \@result;
}

print_aoa(\@seen);

# Add Visible trees together
my $visible = sum0 map {map { s/\A([TLBR])\z/1/gr } @$_} @seen;
printf "Visible: %d\n", $visible;


sub can_be_seen ($x, $y, $forest) {
    my $current = $forest->[$y][$x];

    # Scan to left
    my $left_visible = 1;
    if ( $x == 0 ) {
        return 1;
    }
    else {
        for (my $i=$x-1; $i >= 0; $i--) {
            if ( $forest->[$y][$i] >= $current ) {
                $left_visible = 0;
                last;
            }
        }
    }
    return "L" if $left_visible;

    # Scan to Right
    my $right_visible = 1;
    if ( $x == $width-1 ) {
        return 1;
    }
    else {
        for (my $i=$x+1; $i < $width; $i++) {
            if ( $forest->[$y][$i] >= $current ) {
                $right_visible = 0;
                last;
            }
        }
    }
    return "R" if $right_visible;

    # Scan to Top
    my $top_visible = 1;
    if ( $y == 0 ) {
        return 1;
    }
    else {
        for (my $i=$y-1; $i >= 0; $i--) {
            if ( $forest->[$i][$x] >= $current ) {
                $top_visible = 0;
                last;
            }
        }
    }
    return "T" if $top_visible;

    # Scan to Bottom
    my $bottom_visible = 1;
    if ( $y == $height-1 ) {
        return 1;
    }
    else {
        for (my $i=$y+1; $i < $height; $i++) {
            if ( $forest->[$i][$x] >= $current ) {
                $bottom_visible = 0;
                last;
            }
        }
    }
    return "B" if $bottom_visible;

    return 0;
}

sub print_aoa ($aoa) {
    for my $row ( @$aoa ) {
        for my $col ( @$row ) {
            printf "%s", $col;
        }
        print "\n";
    }
    print "\n";
}