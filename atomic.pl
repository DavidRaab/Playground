#!/usr/bin/env perl

package Atomic;
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';

sub new ($class, $value) {
    return bless { Value => $value }, $class;
}

sub map ($self, $f) {
    return bless {
        Atomic => $self,
        F      => $f,
    }, ref $self;
}

sub get ($self) {
    if ( exists $self->{Value} ) {
        return $self->{Value};
    }
    else {
        my $value = $self->{Atomic}->get;
        return $self->{F}($value);
    }
}

sub set ($self, $x) {
    $self->{Value} = $x;
}


package main;
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Math::Complex qw(:pi);

my $radius = Atomic->new(1);
my $area   = $radius->map(sub ($r) { $r * $r * pi });

print  "Radius 1\n";
printf "Area: %f\n", $area->get;

printf "Radius 3\n";
$radius->set(3);
printf "Area: %f\n", $area->get;

printf "Radius 5\n";
$radius->set(5);
printf "Area: %f\n", $area->get;