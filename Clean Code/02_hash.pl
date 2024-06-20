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
    return {shape => $is_square, side => $side};
}
sub rectangle($width,$height) {
    return {shape => $is_rectangle, width => $width, height => $height};
}
sub triangle($base,$height) {
    return {shape => $is_triangle, base => $base, height => $height};
}
sub circle($radius) {
    return {shape => $is_circle, radius => $radius};
}

sub area($shape) {
    if    ( $shape->{shape} == $is_square    ) { return $shape->{side}   * $shape->{side}         }
    elsif ( $shape->{shape} == $is_rectangle ) { return $shape->{width}  * $shape->{height}       }
    elsif ( $shape->{shape} == $is_triangle  ) { return $shape->{base}   * $shape->{height} * 0.5 }
    elsif ( $shape->{shape} == $is_circle    ) { return $shape->{radius} * $shape->{radius} * 3.141592654 }
    else {
        die "Unknown shape.\n";
    }
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

my $start = time;
my $sum = 0;
for my $shape ( @shapes ) {
    $sum += area($shape);
}
my $stop = time;

printf "Time: %f\n", ($stop-$start);
