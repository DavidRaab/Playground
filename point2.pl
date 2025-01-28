#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';
use Benchmark qw(cmpthese timethis);

package PointP;
use 5.036;

sub new($class, $x, $y) {
    return bless({x => $x, y => $y}, $class);
}

sub x($self, $x=undef) {
    if ( defined $x ) {
        $self->{x} = $x;
    }
    return $self->{x};
}

sub y($self, $y=undef) {
    if ( defined $y ) {
        $self->{y} = $y;
    }
    return $self->{y};
}

sub add($self, $other) {
    return PointP->new(
        $self->{x} + $other->{x},
        $self->{y} + $other->{y},
    );
}

sub to_string($self) {
    sprintf "Point(%f,%f)", $self->x, $self->y;
}

package Point3D;
use 5.036;
our @ISA = 'PointP';

sub new($class, $x, $y, $z) {
    return bless({ x => $x, y => $y, z => $z }, $class);
}

sub z($self, $z=undef) {
    if ( defined $z ) {
        $self->{z} = $z;
    }
    return $self->{z};
}

sub add($self, $other) {
    return Point3D->new(
        $self->{x} + $other->{x},
        $self->{y} + $other->{y},
        $self->{z} + $other->{z},
    );
}

package main;

my $benchmarks = 0;

# Scheme like
sub make_point($x,$y)     { return { x => $x, y => $y }          }
sub make_point3($x,$y,$z) { return { x => $x, y => $y, z => $z } }
sub point_x($point)       { $point->{x} }
sub point_y($point)       { $point->{y} }
sub point_z($point)       { $point->{z} }
sub point_str($point)     {
    return defined $point->{z}
         ? sprintf "3D: %f,%f,%f", $point->{x}, $point->{y}, $point->{z}
         : sprintf "2D: %f,%f",    $point->{x}, $point->{y};
}
sub point_add($p1, $p2) {
    my $p = {
        x => $p1->{x} + $p2->{x},
        y => $p1->{y} + $p2->{y},
    };
    if ( defined $p1->{z} && defined $p1->{z} ) {
        $p->{z} = $p1->{z} + $p2->{z};
    }
    return $p;
}

# Scheme version
my $p = make_point(1,2);

my $p1 = Point3D->new(1,2,3);
my $p2 = Point3D->new(3,2,1);
my $p3 = $p1->add($p2);
my $p4 = point_add(
    Point3D->new(1,2,3),
    { x => 3, y => 2, z => 1 },
);
my $p5 = point_add(
    {x => 1, y => 2},
    {x => 1, y => 2, z => 3},
);
my $p6 = point_add(
    make_point(1,2),
    {x => 1, y => 2, z => 3},
);
my $p7 = point_add(make_point3(1,2,3), make_point3(3,2,1));
my $p8 = point_add(make_point(1,2),    make_point3(3,2,1));

printf "%s\n", point_str($p1);
printf "%s\n", point_str($p2);
printf "%s\n", point_str($p3);
printf "%s\n", point_str($p4);
printf "%s\n", point_str($p5);
printf "%s\n", point_str($p6);
printf "%s\n", point_str($p7);
printf "%s\n", point_str($p8);

{
    my $o1 = Point3D->new(1,2,3);
    my $o2 = Point3D->new(3,2,1);
    my $s1 = make_point3(1,2,3);
    my $s2 = make_point3(3,2,1);

    cmpthese(-1, {
        point3d => sub { $o1->add($o2)       for 1 .. 1000 },
        scheme  => sub { point_add($s1, $s2) for 1 .. 1000 },
    });
}