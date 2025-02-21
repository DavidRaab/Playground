#!/usr/bin/env perl
use 5.036;
use utf8;
use open ':std', 'encoding(UTF-8)';
use DateTime;
use Sq -sig => 1;
use Sq::Parser qw(p_run parser);
use Chart::Clicker;

sub days_delta($entry1, $entry2) {
    # t_assert($is_entry, $entry1, $entry2);
    return $entry1->{date}->delta_days($entry2->{date})->in_units('days');
}

# Parser for strom.txt
my $p_entry = parser [
    map => record(qw/date kwh comment/), [and =>
        [matchf => qr/(\d\d) \. (\d\d) \. (\d\d\d\d)/x, sub($d,$m,$y) {
            DateTime->new(day => $d, month => $m, year => $y)
        }],
        [match => qr/\s+ (\d+)/x],
        [maybe => [match => qr/\s* (.*)/]],
    ]
];

# Parse text lines into a hash
my $data =
    Sq->fs
    ->read_text('strom.txt')
    ->choose(sub($line) { p_run($p_entry, $line) })
    ->fsts;

# dump($data);

# Add all kinds of information to every entry for the data we need to print
# in the total sum and delta print
my $total = assign {
    my $days = 0;
    my $tkwh = 0;

    my $total =
        $data
        ->windowed(2)
        ->map(sub($tuple) {
            my ($previous, $entry) = @$tuple;
            my $delta     = days_delta($previous,$entry);
            my $kwh_delta = $entry->{kwh} - $previous->{kwh};
            $days += $delta;
            $tkwh += $kwh_delta;
            $entry->with(
                days             => $days,
                days_delta       => $delta,
                kwh_total        => $tkwh,
                kwh_total_daily  => $tkwh / $days,
                kwh_delta_daily  => $kwh_delta / $delta,
                kwh_yearly       => ($tkwh / $days) * 365,
            );
        })
        # We either need to call ->to_array or add ->cache.
        #
        # This is important when a sequence captures a mutable variable
        # otherwise when the sequence is evaluated multiple times the mutable
        # variable is changed/increased every time the sequence is evaluated.
        # Adding to_array/cache means the sequence is only ever evaluated once and it
        # then works correctly with mutable state.
        ->to_array;

    # add/generate kwh_change to every entry
    if ( @$total >= 2 ) {
        for (my $idx=1; $idx < @$total; $idx++) {
            my $p = $total->[$idx-1];
            my $c = $total->[$idx];
            $c->{kwh_change} = $c->{kwh_delta_daily} - $p->{kwh_delta_daily};
        }
    }

    return $total;
};

# dump($total);

print "Total\n";
{
    # TODO: This could be better
    my $data = $total->map(sub($hash) {
        $hash->withf(
            date            => sub($date) { $date->dmy('.') },
            kwh_total_daily => sub($kwh)  { sprintf('%0.2f', $kwh) },
            kwh_yearly      => sub($kwh)  { sprintf('%0d',   $kwh) },
        )
        ->extract(qw/date days kwh_total_daily comment/)
        ->map(call 'or', "");
    })->to_array;
    # dump($data);

    # TODO: spacing => 2
    # TODO: Re-print header after X items
    # TODO: Alignment of Cells
    Sq->fmt->table({
        header => ['Date', 'Days', 'KwH/Days', 'Comment'],
        data   => $data,
        border => 1,
    });
}
print "\n";

### PRINT DELTA

print "Delta\n";
{
    my $data = $total->map(sub($hash) {
        $hash->withf(
            date            => sub($date) { $date->dmy('.')        },
            kwh_delta_daily => sub($kwh)  { sprintf('%0.2f', $kwh) },
            kwh_change      => sub($kwh)  { sprintf('%+0.2f', $kwh) },
        )
        ->extract(qw/date days_delta kwh_delta_daily kwh_change comment/)
        ->map(call 'or', "");
    })->to_array;
    # dump($data);

    Sq->fmt->table({
        header => ['Date', 'Days', 'KwH/Days', 'Change', 'Comment'],
        data   => $data,
        border => 1,
    });
}
print "\n";

=pod

# Generate a Chart
{
    my $cc   = Chart::Clicker->new(width => 1500, height => 1000, format => 'png');
    my $days = $total->map(sub($entry) { $entry->{days} });

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