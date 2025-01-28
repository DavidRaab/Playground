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

sub add1($self, $other) {
    return PointP->new(
        $self->x + $other->x,
        $self->y + $other->y,
    );
}

sub add2($self, $other) {
    return PointP->new(
        $self->{x} + $other->{x},
        $self->{y} + $other->{y},
    );
}

sub to_string($self) {
    sprintf "Point(%f,%f)", $self->x, $self->y;
}

package PointM;
use 5.036;
use Moose;

has 'x' => (is => 'rw', isa => 'Num' );
has 'y' => (is => 'rw', isa => 'Num' );

sub add($self, $other) {
    return PointM->new(
        x => $self->x + $other->x,
        y => $self->y + $other->y,
    );
}

sub to_string($self) {
    sprintf "Point(%f,%f)", $self->x, $self->y;
}

__PACKAGE__->meta->make_immutable;

package main;

my $benchmarks = 1;

my $p1 = PointP->new(1, 2);
print $p1->to_string, "\n";

my $p2 = PointM->new(x => 1, y => 2);
print $p2->to_string, "\n";

# Creating Benchmark
if ( $benchmarks ) {
    printf "Init\n";
    cmpthese(-1, {
        perl  => sub { for ( 1 .. 1_000 ) { my $p = PointP->new(1, 2)           } },
        moose => sub { for ( 1 .. 1_000 ) { my $p = PointM->new(x => 1, y => 2) } },
        hash  => sub { for ( 1 .. 1_000 ) { my $h = { x => 1, y => 2 }          } },
    });
}

my $pp = PointP->new(1,2);
my $pm = PointM->new(x => 1, y => 2);
my $h  = { x => 1, y => 2 };

if ( $benchmarks ) {
    printf "get\n";
    cmpthese(-1, {
        perl  => sub { for ( 1 .. 1_000 ) { my $x = $pp->x  } },
        moose => sub { for ( 1 .. 1_000 ) { my $x = $pm->x  } },
        hash  => sub { for ( 1 .. 1_000 ) { my $x = $h->{x} } },
    });

    printf "set\n";
    cmpthese(-1, {
        perl  => sub { for ( 1 .. 1_000 ) { $pp->x(1)   } },
        moose => sub { for ( 1 .. 1_000 ) { $pm->x(1)   } },
        hash  => sub { for ( 1 .. 1_000 ) { $h->{x} = 1 } },
    });
}

# Scheme
sub make_point($x,$y) {
    return { x => $x, y => $y };
}
sub point_x($point) {
    return $point->{x};
}
sub point_y($point) {
    return $point->{y};
}
sub point_set_x($point, $x) {
    $point->{x} = $x;
}

# Scheme version
my $p = make_point(1,2);

if ( $benchmarks ) {
    cmpthese(-1, {
        init => sub { make_point(1,2)   for 1 .. 1_000 },
        get  => sub { point_x($p)       for 1 .. 1_000 },
        set  => sub { point_set_x($p,1) for 1 .. 1_000 },
    });
}

sub point_add_1($p1, $p2) {
    make_point(
        point_x($p1) + point_x($p2),
        point_y($p1) + point_y($p2),
    );
}

sub point_add_2($p1, $p2) {
    make_point(
        $p1->{x} + $p2->{x},
        $p1->{y} + $p2->{y},
    );
}

my $p3 = point_add_1($p, make_point(2,1));
printf "p3{%f,%f}", $p3->{x}, $p3->{y};

{
    my $pp1 = PointP->new(1,2);
    my $pp2 = PointP->new(2,1);

    my $sp1 = make_point(1,2);
    my $sp2 = make_point(2,1);

    cmpthese(-1, {
        class_1 => sub { $pp1->add1($pp2)        for 1 .. 1000 },
        class_2 => sub { $pp1->add2($pp2)        for 1 .. 1000 },
        hash_1  => sub { point_add_1($sp1, $sp2) for 1 .. 1000 },
        hash_2  => sub { point_add_2($sp1, $sp2) for 1 .. 1000 },
    });
}

{
    my $pm1 = PointM->new(x => 1, y => 2);
    my $pm2 = PointM->new(x => 1, y => 2);

    timethis(-3, sub {
        $pm1->add($pm2) for 1 .. 1000;
    });
}