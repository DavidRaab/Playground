#!/usr/bin/env perl
package Game;
use strict;
use warnings;
use Moo;
use Types::Standard qw(Int ArrayRef);
use List::Util qw(max);
use namespace::clean;

our $Dead  = 0;
our $Alive = 1;

has 'init'     => ( is => 'ro', isa => Int, default => $Dead );
has 'columns'  => ( is => 'ro', isa => Int->where('$_ >= 0'), required => 1);
has 'rows'     => ( is => 'ro', isa => Int->where('$_ >= 0'), required => 1);
has 'field'    => (
    is      => 'ro', isa => ArrayRef[Int],
    builder => 1, lazy => 1,
);
has 'colCount' => ( 
    is      => 'ro', isa => Int, 
    builder => 1, lazy => 1,
);

sub _build_field {
    my ($self) = @_;

    my $count = ($self->columns+2) * ($self->rows+2);
    my @field = ($self->init) x $count;
    return \@field;
}

sub _build_colCount {
    my ($self) = @_;
    return $self->columns + 2;
}

sub fromLoL {
    my ($class, $init, $lol) = @_;

    my $maxY = scalar @$lol;
    my $maxX = max (map { scalar @$_ } @$lol);
    my $game = Game->new(
        init    => $init,
        rows    => $maxY,
        columns => $maxX,
    );

    my $colCount = $game->colCount;
    my $field    = $game->field;

    for my $y (0 .. $maxY-1) {
        for my $x (0 .. $maxX-1) {
            my $index = $colCount * ($y+1) + ($x+1);
            $field->[$index] = $lol->[$y][$x];
        }
    }
    
    return $game;
}

sub fromStr {
    my ($class, $init, $str) = @_;

    my @lines = map {
            [map { $_ eq 'x' or $_ eq 'X' ? 1 : 0 } split //] 
       } split /\r?\n/, $str;
    
    return Game->fromLoL($init, \@lines);
}

sub get {
    my ($self, $x, $y) = @_;

    my $index = $self->colCount * $y + $x;
    return $self->field->[$index];
}

sub show {
    my ($self) = @_;

    my $colCount = $self->colCount;
    my $field    = $self->field;

    for my $y (1 .. $self->rows) {
        for my $x (1 .. $self->columns) {
            my $index = $colCount * $y + $x;
            printf ($field->[$index] ? 'X' : '.');
        }
        printf "\n";
    }
    printf "\n";

    return;
}

sub map {
    my ($self, $f) = @_;

    my $colCount = $self->colCount;
    my $field    = $self->field;

    my $newGame  = Game->new(
        init    => $self->init,
        columns => $self->columns,
        rows    => $self->rows
    );
    my $newField = $newGame->field;

    for (my $y=1; $y <= $self->rows; $y++) {
        for (my $x=1; $x <= $self->columns; $x++) {
            my $index  = $colCount * $y + $x;
            my $alives =
                  $field->[$index - ($colCount+1)]
                + $field->[$index -  $colCount]
                + $field->[$index - ($colCount-1)]
                + $field->[$index - 1]
                + $field->[$index + 1]
                + $field->[$index + ($colCount-1)]
                + $field->[$index +  $colCount]
                + $field->[$index + ($colCount+1)];

            $newField->[$index] = $f->($field->[$index], $alives);
        }
    }

    return $newGame;
}

sub nextState {
    my ($self) = @_;

    return $self->map(sub {
        my ($state, $alives) = @_;
        if ($state == $Dead && $alives == 3) {
            return $Alive;
        }
        elsif ($state == $Alive && ($alives == 2 || $alives == 3)) {
            return $Alive;
        }
        else {
            return $Dead;
        }
    });
}

sub equal {
    my ($self, $game) = @_;

    my $source = $self->field;
    my $target = $game->field;

    return 0 if @$source != @$target;

    for my $i (0 .. @$source-1) {
        return 0 if $source->[$i] != $target->[$i];
    }

    return 1;
}

package main;
use strict;
use warnings;
use v5.10;
use Data::Dump qw(dump dd);
use Carp qw(croak);
use Time::HiRes qw(sleep time);
use File::Slurp qw(read_file);
use Term::Cap;
use IO::Handle;

# App Helper
my $term = Term::Cap->Tgetent;
sub setCursor {
    my ($y, $x) = @_;
    print $term->Tgoto("cm", $y, $x);
}
sub clear {
    print $term->Tputs("cl", 1);
}
sub setCursorInvisible {
    print $term->Tputs("vi");
}
sub setCursorVisible {
    print $term->Tputs("ve");
}
sub quitApp {
    my ($signame) = @_;

    setCursorVisible();
    exit 0;
}

# App-Initialization
STDOUT->autoflush(0);
clear();
setCursorInvisible();

$SIG{INT}  = \&quitApp;
$SIG{QUIT} = \&quitApp;

# Read File
my $file      = read_file($ARGV[0]);
my $sleepTime = ($ARGV[1] || 200) / 1000;
my $init      = Game->fromStr($Game::Dead, $file);

my $startTime = time();

# Print Phase 1
printf "Phase: 1\n";
$init->show();
STDOUT->flush();

# Main Loop
my $prev    = $init;
my $current = $init->nextState;
my $phase = 2;
while (not $current->equal($prev)) {
    setCursor(0,0);
    printf "Phase: %d\n", $phase++;
    $current->show();
    STDOUT->flush;
    sleep $sleepTime;

    $prev    = $current;
    $current = $prev->nextState;
}

my $stopTime = time();
my $elapsed  = $stopTime - $startTime;
printf "Time: %4.5f\n", $elapsed;

quitApp();
