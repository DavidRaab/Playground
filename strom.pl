#!/usr/bin/env perl

# StromEntry Class
package StromEntry;
use Moose;
use v5.32;
use Types::Standard qw(Any Int Str InstanceOf);
use Type::Params qw(compile);
use namespace::clean;
use overload '""' => 'to_string';

has 'date'    => ( is => 'ro', required => 1, isa => InstanceOf['DateTime'] );
has 'kwh'     => ( is => 'ro', required => 1, isa => Int );
has 'comment' => ( is => 'ro', required => 1, isa => Str );

sub to_string {
    my ( $self ) = @_;
    return sprintf("[Date: %s; Kwh: %d; Comment: %s]",
        $self->date->datetime,
        $self->kwh,
        $self->comment
    );
}

my $StromEntry = InstanceOf['StromEntry'];

# Calculates days between two entries
sub days_delta {
    state $check = compile(Any, $StromEntry, $StromEntry);
    my ( $class, $entry1, $entry2 ) = &$check;
    return $entry1->date->delta_days($entry2->date)->in_units('days');
}

__PACKAGE__->meta->make_immutable;

# Strom Class
package Strom;
use Moose;
use v5.32;
use List::Util qw(reduce);
use Scalar::Util qw(refaddr);
use Types::Standard qw(Any CodeRef ArrayRef Object InstanceOf);
use Type::Params qw(compile);
use namespace::clean -except => 'before';

my sub Strom { InstanceOf['Strom']      }
my sub Entry { InstanceOf['StromEntry'] }

has 'start' => (
    is      => 'ro',
    isa     => Entry,
    lazy    => 1,
    builder => '_start',
);

has 'start_date' => (
    is       => 'ro',
    isa      => InstanceOf['DateTime'],
    required => 1,
);

has 'entries' => (
    is      => 'ro',
    isa     => ArrayRef[Entry],
    default => sub { [] },
    traits  => ['Array'],
    handles => {
        get         => 'get',
        elements    => 'elements',
        has_entries => 'count',
        count       => 'count',
        map         => 'map',
        filter      => 'grep',
        foreach     => 'natatime',
    }
);

sub _start {
    my ( $self ) = @_;
    return StromEntry->new(
        date    => $self->start_date,
        kwh     => 0,
        comment => "",
    );
}

no Moose;

sub BUILD {
    my ( $self ) = @_;

    # Check if dates are in ascending order
    for my $entry ( $self->elements ) {
        my $before = $self->before($entry);

        if ( $entry->date < $before->date ) {
            die sprintf(
                "Datum %s muss größer sein als vorheriger Eintrag\n",
                $entry->dmy('.')
            );
        }
    }
}

sub add {
    state $check = compile(Strom, Entry);
    my ( $self, $entry ) = &$check;
    return Strom->new(
        start_date => $self->start_date,
        entries    => [ $self->elements, $entry ],
    );
}

sub last {
    state $check = compile(Strom);
    my ( $self ) = &$check;
    return $self->get(-1);
}

sub fold {
    state $check = compile(Strom, Any, CodeRef);
    my ( $self, $init, $code ) = &$check;
    return reduce { $code->($a,$b) } $init, $self->elements;
}

sub days_since_start {
    state $check = compile(Strom, Entry);
    my ( $self, $entry ) = &$check;
    return StromEntry->days_delta($entry, $self->start);
}

# search and returns the StromEntry exactly before entry
sub before {
    state $check = compile(Strom, Entry);
    my ( $self, $current ) = &$check;

    my @elements = $self->elements;
    my $before   = shift @elements;

    for my $entry ( @elements ) {
        if ( refaddr $entry == refaddr $current ) {
            return $before;
        }
        $before = $entry;
    }

    return $self->start;
}

sub average_since_start {
    state $check = compile(Strom, Entry);
    my ( $self, $entry ) = &$check;

    my $days    = $self->days_since_start($entry);
    my $average = $entry->kwh / $days;

    return $average;
}

sub average_before {
    state $check = compile(Strom, Entry);
    my ( $self, $entry ) = &$check;

    my $before  = $self->before($entry);
    my $days    = StromEntry->days_delta($entry, $before);

    if ( $days == 0 ) {
        return 0;
    }

    return ($entry->kwh - $before->kwh) / $days;
}

__PACKAGE__->meta->make_immutable;

# Main Program
package main;
use strict;
use warnings;
use v5.32;
use Data::Dump qw(dump dd);
use Data::Dump::Filtered qw(add_dump_filter);
use Carp qw(croak);
use DateTime;
use List::Util qw(max);
use Chart::Clicker;

# Dump DateTime as String
add_dump_filter(sub{
    my ($ctx, $obj) = @_;

    if ( $ctx->class eq 'DateTime' ) {
        return { dump => $obj->datetime };
    }

    return;
});

# Parsing
my @data;
open my $fh, '<', 'strom.txt' or die "Cannot open file: $!\n";
while ( my $line = <$fh> ) {
    if ( $line =~ m/\A \s* (\d\d)\.(\d\d)\.(\d\d\d\d) \s+ (\d+) \s* ([^\r\n]*) \Z/xms ) {
        my $dt = DateTime->new(
            day   => $1,
            month => $2,
            year  => $3,
        );
        my $kwh     = $4;
        my $comment = $5;

        push @data, [$dt, $kwh, $comment];
    }
}
close $fh;

# Create Strom
my $strom = (sub {
    my $first    = shift @data;
    my $init_kwh = $first->[1];
    my @entries  = map {
        StromEntry->new(
            date    => $_->[0],
            kwh     => $_->[1] - $init_kwh,
            comment => $_->[2],
        )
    } @data;

    return Strom->new(
        start_date => $first->[0],
        entries    => \@entries,
    );
})->();

# Insgesamt Ausgabe
printf "Insgesamt:\n";
for my $entry ( $strom->elements ) {
    my $days    = $strom->days_since_start($entry);
    my $average = $strom->average_since_start($entry);
    my $comment = $entry->comment ? "-- " . $entry->comment : "";

    printf "Datum: %s | Days: %3d | Kwh/Day: %.2f | 365-Total: %4d %s\n",
        $entry->date->dmy('.'),
        $days,
        $average,
        $average * 365,
        $comment;
}

# Delta Ausgabe
printf "\nDelta:\n";
for my $entry ( $strom->elements ) {
    my $before  = $strom->before($entry);
    my $days    = StromEntry->days_delta($entry, $before);
    my $average = $strom->average_before($entry);
    my $change  = $average - $strom->average_before($before);
    my $comment = $entry->comment ? "-- " . $entry->comment : "";

    printf "Datum: %s | Days: %3d | Kwh/Day: %.2f | Change: %+.2f kwh %s\n",
        $entry->date->dmy('.'),
        $days,
        $average,
        $change,
        $comment;
}

# Generate a Chart
{
    my $cc   = Chart::Clicker->new(width => 1500, height => 1000, format => 'png');
    my $days = [ $strom->map(sub{ $strom->days_since_start($_) }) ];

    my $insgesamt = Chart::Clicker::Data::Series->new(
        name    => 'Insgesamt',
        keys    => $days,
        values  => [$strom->map(sub{ $strom->average_since_start($_) })],
    );

    my $deltas = Chart::Clicker::Data::Series->new(
        name    => 'Delta',
        keys    => $days,
        values  => [$strom->map(sub{ $strom->average_before($_) })],
    );

    my $ds = Chart::Clicker::Data::DataSet->new(
        series => [ $insgesamt, $deltas ]
    );

    $cc->title->text('Stromverbrauch');
    $cc->title->padding->bottom(5);
    $cc->add_to_datasets($ds);

    my $defctx = $cc->get_context('default');

    $defctx->renderer->shape(
        Geometry::Primitive::Circle->new({
        radius => 5,
        })
    );

    $defctx->renderer->shape_brush(
        Graphics::Primitive::Brush->new(
            width => 2,
            color => Graphics::Color::RGB->new(red => 1, green => 1, blue => 1)
        )
    );

    $defctx->domain_axis->label('Tage');
    $defctx->range_axis->label('Kwh pro Tag');
    $defctx->range_axis->range->min(2);
    $defctx->range_axis->range->max(5);
    $defctx->renderer->brush->width(2);

    $cc->write_output('strom_pl.png');
}