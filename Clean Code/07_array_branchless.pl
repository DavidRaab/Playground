#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';
use Time::HiRes qw(time);
use Data::Printer;
use Getopt::Long::Descriptive;

my ($opt, $usage) = describe_options(
    'Usage: %c %o',
    ['help|h', 'Print this message', {shortcircuit => 1}],
);

$usage->die if $opt->help;

my $is_square    = 1;
my $is_rectangle = 2;
my $is_triangle  = 3;
my $is_circle    = 4;

sub square($side) {
    return [$is_square, $side, $side];
}
sub rectangle($width,$height) {
    return [$is_rectangle, $width, $height];
}
sub triangle($base,$height) {
    return [$is_triangle, $base, $height];
}
sub circle($radius) {
    return [$is_circle, $radius, $radius];
}

my $cstart = time;
my @shapes;
for ( 1 .. 1_000_000 ) {
    push @shapes, square(rand 10);
    push @shapes, rectangle(rand 10, rand 10);
    push @shapes, triangle(rand 10, rand 10);
    push @shapes, circle(rand 10);
}
my $cstop = time;
printf "Creation Time: %f\n", ($cstop-$cstart);

# factor table - maps shape types to a factor
my @factor = (undef, 1, 1, 0.5, 3.141592654);

my $start = time;
my $sum = 0;
for my $shape ( @shapes ) {
    $sum += $factor[$shape->[0]] * $shape->[1] * $shape->[2];
}
my $stop = time;

printf "Time: %f\n", ($stop-$start);
