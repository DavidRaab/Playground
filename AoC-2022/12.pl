#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Scalar::Util qw(blessed);
use List::MoreUtils qw(zip);

# read input as Array2D
my $map = Array2D->from_aoa([ map {chomp; [split //]} <> ]);

# get start/stop indices
my $start = $map->reduce([-1,-1], sub ($acc, $val, $x, $y) {
    $val eq 'S' ? Pos->new($x,$y) : $acc
});

my $stop = $map->reduce([-1,-1], sub ($acc, $val, $x, $y) {
    $val eq 'E' ? Pos->new($x,$y) : $acc
});

printf "Start: %s\n", np($start);
printf "Stop:  %s\n\n", np($stop);

# mapping from chars to num
my @chars   = ('a' .. 'z', 'S', 'E');
my @nums    = ( 1  ..  26,  1,   26);
my %to_num  = zip @chars, @nums;
my %to_char = zip @nums, @chars;
my $input   = $map->map(sub ($x) { $to_num{$x} });

# Show original input and then number array
# say $map->show  (sub ($pos,$v) { sprintf "%2s",  $v }), "\n";
# say $input->show(sub ($pos,$v) { sprintf "%02d", $v }), "\n";

# Show Dijkstra map
my $field = dijkstra($input, $start);
my $path  = path($field, $stop);

# show Path visually
show_path($map, $path);

# Print length
printf "Path Length: %d\n\n", (scalar @$path - 1);

# Part 2
my $shortest_field = $field;
my $shortest_path  = $path;
{
    for my $y ( 0 .. $input->height-1 ) {
        for my $x ( 0 .. $input->width-1 ) {
            if ( $input->get_xy($x,$y) == 1 ) {
                my $field = dijkstra($input, Pos->new($x,$y));
                my $path  = path($field, $stop);

                if ( defined $path && @$path < @$shortest_path ) {
                    $shortest_field = $field;
                    $shortest_path  = $path;
                }
            }
        }
    }
}

print "Shortest\n\n";
show_path($map, $shortest_path);
printf "Shortest Length: %d\n", (scalar @$shortest_path - 1);


# Computes the Dijkstra map
sub dijkstra($input, $start) {
    # Empty initialized target map for dijkstra algorithm
    my $field = Array2D->init($map->width, $map->height, sub { -1 } );

    # Consider Pos(-1,-1) as target
    $field->set($start, Pos->new(-1,-1));

    # Compute Dijkstra Map
    my @queue = ($start);
    while ( my $pos = shift @queue ) {
        my $current = $input->get($pos);

        for my $next ( $pos->top, $pos->right, $pos->bottom, $pos->left ) {
            my $next_value = $input->get($next);
            next if not defined $next_value;

            if ( $next_value <= $current + 1 ) {
                if ( not ref $field->get($next) ) {
                    $field->set($next, $pos);
                    push @queue, $next;
                }
            }
        }
    }

    return $field;
}

# Produces the Path
sub path($dij, $target) {
    my @path;
    my $stop = Pos->new(-1,-1);

    my $node = $target;
    NODE:
    push @path, $node;
    $node = $dij->get($node);
    goto NOPATH if $node == -1;
    goto NODE   if not $node->equal($stop);

    return wantarray ? @path : \@path;

    NOPATH:
    return;
}

sub show_path($input, $path) {
    my $show = Array2D->init($input->width, $input->height, sub { "." });
    for my $pos ( @$path ) {
        $show->set($pos, $input->get($pos));
    }
    say $show->show(sub ($pos, $value) { $value });
}

package Pos;
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';

sub new     ($class,$x,$y) { bless [$x,$y], $class }
sub x       ($self) { $self->[0] }
sub y       ($self) { $self->[1] }
sub xy      ($self) { return @$self }
sub top     ($self) { Pos->new($self->x    , $self->y - 1) }
sub right   ($self) { Pos->new($self->x + 1, $self->y    ) }
sub bottom  ($self) { Pos->new($self->x    , $self->y + 1) }
sub left    ($self) { Pos->new($self->x - 1, $self->y    ) }

sub equal ($self,$pos) {
    return 1 if $self->x == $pos->x && $self->y == $pos->y;
    return;
}

sub _data_printer ($self,$ddp) {
    return sprintf("Pos(%d,%d)", $self->x, $self->y);
}

# Array2D Helper
package Array2D;
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use List::Util qw(max);

sub from_aoa($class, $aoa) {
    return bless({
        data   => $aoa,
        width  => max(map { scalar @$_ } @$aoa),
        height => scalar @$aoa,
    }, $class);
}

sub init ($class, $width, $height, $f) {
    my @arr;
    for my $y ( 0 .. $height-1 ) {
        my @row;
        for my $x ( 0 .. $width-1 ) {
            push @row, $f->($x,$y);
        }
        push @arr, \@row;
    }

    return bless({
        data   => \@arr,
        width  => $width,
        height => $height,
    }, $class);
}

sub height ($self) { return $self->{height} }
sub width  ($self) { return $self->{width}  }

sub is_inside ($self, $pos) {
    my ($x, $y) = $pos->xy;
    if ( $x >= 0 && $y >= 0 && $x < $self->{width} && $y < $self->{height} ) {
        return 1;
    }
    return;
}

sub get($self, $pos) {
    my ($x, $y) = $pos->xy;
    if ( $x >= 0 && $y >= 0 && $x < $self->{width} && $y < $self->{height} ) {
        return $self->{data}[$y][$x];
    }
    return;
}

sub get_xy($self, $x, $y) {
    if ( $x >= 0 && $y >= 0 && $x < $self->{width} && $y < $self->{height} ) {
        return $self->{data}[$y][$x];
    }
    return;
}

sub set($self, $pos, $value) {
    $self->{data}[$pos->y][$pos->x] = $value;
}

sub set_xy($self, $x, $y, $value) {
    $self->{data}[$y][$x] = $value;
}

sub reduce ($self, $init, $f) {
    for my $y ( 0 .. $self->height - 1 ) {
        for my $x ( 0 .. $self->width - 1 ) {
            $init = $f->($init, $self->get_xy($x,$y), $x, $y);
        }
    }
    return $init;
}

sub iter ($self, $f) {
    for my $y ( 0 .. $self->height - 1 ) {
        for my $x ( 0 .. $self->width - 1 ) {
            $f->(Pos->new($x,$y), $self->get_xy($x,$y));
        }
    }
}

sub map ($self, $f) {
    my @new;
    $self->iter(sub($pos, $val) {
        $new[$pos->y][$pos->x] = $f->($val);
    });
    return bless({
        data   => \@new,
        width  => $self->width,
        height => $self->height,
    }, ref $self);
}

sub show($self, $fmt) {
    my $str = "";
    for my $y ( 0 .. $self->height - 1 ) {
        my @row;
        for my $x ( 0 .. $self->width - 1 ) {
            my $pos = Pos->new($x,$y);
            push @row, $fmt->($pos, $self->get($pos));
        }
        $str .= join("", @row) . "\n";
    }
    return $str;
}