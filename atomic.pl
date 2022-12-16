#!/usr/bin/env perl

package Atomic;
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';

sub value ($class, $value) {
    my $ref = [ $value ];
    return bless sub { $ref }, $class;
}

sub map ($self, $f) {
    return bless sub {
        my $ref = $self->();
        return [ $f->($ref->[0]) ];
    }, ref $self;
}

sub map2 ($self, $other, $f) {
    return bless sub {
        my $a = $self->()[0];
        my $b = $other->()[0];
        return [$f->($a, $b)];
    }, ref $self;
}

sub get ($self) {
    my $ref = $self->();
    return $ref->[0];
}

sub set ($self, $x) {
    my $ref = $self->();
    $ref->[0] = $x;
    return;
}


package main;
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Math::Complex qw(:pi);
use Test::More;

my $radius = Atomic->value(1);
my $area   = $radius->map(sub ($r) { $r * $r * pi });

# radius of 1
cmp_ok($radius->get, '==', 1, "radius of 1");
cmp_ok($area->get,   '==', pi, "area of pi");

# set radius to 2
$radius->set(2);
cmp_ok($radius->get, '==', 2,      "radius of 2");
cmp_ok($area->get,   '==', (4*pi), "area of 2");

# Does nothing ...
$area->set(10);

cmp_ok($radius->get, '==', 2,      "still radius of 2");
cmp_ok($area->get,   '==', (4*pi), "still area of 2");

# Build two Vector type
my $x1 = Atomic->value(1);
my $y1 = Atomic->value(1);
my $x2 = Atomic->value(3);
my $y2 = Atomic->value(3);

my $v1 = $x1->map2($y1, sub ($x,$y)   { vector($x,$y) });
my $v2 = $x2->map2($y2, sub ($x,$y)   { vector($x,$y) });
my $v3 = $v1->map2($v2, sub ($v1,$v2) { vector_add($v1,$v2) });

is_deeply($v1->get, [1,1], "vector 1");
is_deeply($v2->get, [3,3], "vector 2");
is_deeply($v3->get, [4,4], "vector 3");

$x1->set(0);
$y2->set(10);

is_deeply($v1->get, [0,1],  "vector 1 after changing x1/y2");
is_deeply($v2->get, [3,10], "vector 2 after changing x1/y2");
is_deeply($v3->get, [3,11], "vector 3 after changing x1/y2");

done_testing();


# A Vector type
sub vector($x,$y) {
    return [$x, $y];
}
sub vector_add($v1, $v2) {
    return [$v1->[0] + $v2->[0], $v1->[1] + $v2->[1]];
}
