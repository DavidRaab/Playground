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

package Square;
use 5.036;
sub new($class, $side) { bless({side => $side}, $class) }
sub area($self) { return $self->{side} * $self->{side} }

package Rectangle;
use 5.036;
sub new($class,$x,$y) { bless({x => $x, y => $y}, $class) }
sub area($self) { return $self->{x} * $self->{y} }

package Triangle;
use 5.036;
sub new($class, $base, $height) { bless({base => $base, height => $height}, $class) }
sub area($self) { return $self->{base} * $self->{height} * 0.5 }

package Circle;
use 5.036;
sub new($class, $radius) { bless({radius => $radius}, $class) }
sub area($self) { return $self->{radius} * 3.141592654 }

package main;

my $cstart = time;
my @shapes;
for ( 1 .. 1_000_000 ) {
    push @shapes, Square   ->new(rand 10);
    push @shapes, Rectangle->new(rand 10, rand 10);
    push @shapes, Triangle ->new(rand 10, rand 10);
    push @shapes, Circle   ->new(rand 10);
}
my $cstop = time;
printf "Creation time: %f\n", ($cstop-$cstart);

my $start = time;
my $sum = 0;
for my $shape ( @shapes ) {
    $sum += $shape->area;
}
my $stop = time;

printf "Sum Time: %f\n", ($stop-$start);
