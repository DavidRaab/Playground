#!/usr/bin/env perl
use 5.040;
use utf8;
use open ':std', ':encoding(UTF-8)';
use Sq -sig => 1;

sub to_entry($array) {
    state $entry = record(qw/gpu row1 row2 row3 row4 row5/);
    $entry->(@$array);
}

# percentage to number
sub percentage($str) {
    if ( $str =~ m/\A ( \d+ \. \d+ | \d+ ) %\z/x ) {
        return Some($1);
    }
    return None;
};

# The .txt file was created by selecting the chart in web-browser and
# copy & pasted. It created a file with the graphics card name,
# following 5 lines for every row, the change and a empty line. So i want
# to process this data directly as possible without changing the txt file.
#
# Thhe following does.
# 1) put 8 lines into it's own chunk. So we have an array of array with 8 entries
# 2) i don't care for the last two lines. the %change and the empty line. So "take"
#    creates new array of the inner array with just the first 6 elements.
# 3) then we map every inner array to an hash, by applying the function "to_entry" to them
# 4) i ->cache() the result. So iterating $data multiple times would only read/parse
#    the file once.
my $data =
    Sq->fs
    ->read_text('2026-01-15-steam_survey.txt')
    ->chunked(8)
    ->map(call 'take', 6)
    ->map(\&to_entry)
    ->cache;

# Those mapping are not perfect yet, but still are a very good estimate
sub level($card) {
    state $profiles = hash(
        # everything <= RTX 5050
        #   when a card is a little bit faster, but performance is closer to 5050 instead
        #   of 5060, it is put into Low. Like 3060 Ti
        Low    => [
            qr/RTX 5050/, qr/RTX (2050|2060|2070|2080|3050|3060|4050|4060)/,
            qr/GTX/, qr/RX (570$|570 |580$|580 |5700|6600|6650|7600)/, qr/Radeon 780M/,
            qr/Intel UHD/,
        ],
        # between 5060 - 5070
        Medium => [qr/RTX 5060/, qr/RTX 2080 (Ti|SUPER)/, qr/RTX (3070|3080|4060 Ti|4070)/, qr/RX (6700|6750|7700)/],
        # between 5070 - 5080
        High   => [qr/RTX 5070/, qr/RTX (3090|4080|4070 Ti SUPER)/, qr/RX 7800/],
        # everything >= 5080
        Ultra  => [qr/RTX (5080|5090)/, qr/RTX 4090/, qr/7900 XTX/],
    );

    my $level = "Unknown";
    for my $prof ( qw/Low Medium High Ultra/ ) {
        for my $match ( $profiles->{$prof}->@* ) {
            $level = $prof if $card =~ $match;
        }
    }
    return $level;
}

# This shows GPU to Level mapping
#
# $data->map(key 'gpu')->sort(by_str)->iter(sub($card){
#     my $level = level($card);
#     printf "%-6s => %s\n", $level, $card;
# });

my $empty = { map {$_ => 0} qw/row1 row2 row3 row4 row5/ };
my $level =
    $data
    ->group_by(sub($entry) { level($entry->{gpu}) })
    ->map(sub($k,$array) {
        return $k, $array->fold_mut(copy($empty), sub($entry, $state) {
            for my $row ( qw/row1 row2 row3 row4 row5/ ) {
                percentage($entry->{$row})->iter(sub($num){
                    $state->{$row} += $num;
                });
            }
        })
    });

# dump($level);
print "Usage of Graphic Cards. Cards are divided into Low, Medium, High, Ultra\n";
print "Low = <RTX 5050 | Medium = >5060 | High = >5070 | Ultra > 5080\n";
print "Steam doesn't recognize a lot of AMD cards, so sadly they are UNKNOWN\n\n";
my @rows = qw/row1 row2 row3 row4 row5/;
Sq->fmt->table({
    data => [
        ["", @rows],
        [Low     => $level->{Low}    ->extract(@rows)->all_some->or([])->expand ],
        [Medium  => $level->{Medium} ->extract(@rows)->all_some->or([])->expand ],
        [High    => $level->{High}   ->extract(@rows)->all_some->or([])->expand ],
        [Ultra   => $level->{Ultra}  ->extract(@rows)->all_some->or([])->expand ],
        [Unknown => $level->{Unknown}->extract(@rows)->all_some->or([])->expand ],
    ]
});
print "\n";

my $overview = $data->group_by(sub($entry) {
    if ( $entry->{gpu} =~ m/\ANVIDIA/ ) {
        return 'NVIDIA';
    }
    elsif ( $entry->{gpu} =~ m/\AAMD/ ) {
        return 'AMD';
    }
    elsif ( $entry->{gpu} =~ m/\AIntel/i ) {
        return 'Intel';
    }
    else {
        return $entry->{gpu};
    }
});

$overview->{NVIDIA} = $overview->{NVIDIA}->group_by(sub($entry){
    if ( $entry->{gpu} =~ m/GTX/ ) {
        return 'GTX';
    }
    elsif ( $entry->{gpu} =~ m/RTX/ ) {
        return 'RTX';
    }
    else {
        dump($entry);
        die "unknown";
    }
});

# dump $overview;

my $usage = hash;
for my $row ( qw/row1 row2 row3 row4 row5/ ) {
    my $to_num = sub($entry) {
        if ( $entry->{$row} =~ m/\A ( \d+ \. \d+ | \d+ ) %\z/x ) {
            return Some($1);
        }
        return None;
    };

    $usage->push(AMD          => $overview->{AMD}        ->choose($to_num)->sum);
    $usage->push('NVIDIA GTX' => $overview->{NVIDIA}{GTX}->choose($to_num)->sum);
    $usage->push('NVIDIA RTX' => $overview->{NVIDIA}{RTX}->choose($to_num)->sum);
    $usage->push(Intel        => $overview->{Intel}      ->choose($to_num)->sum);
    $usage->push(Other        => Str->chop($overview->{Other}[0]{$row}));
}

Sq->fmt->table({
    data => $usage->to_array(sub($k,$v){ [$k,@$v] })->sort_by(by_str, idx 0),
});

sub gpu_usage_of($regex) {
    return sub($entry) {
        if ( $entry->{gpu} =~ $regex ) {
            return Some(Str->chop($entry->{row5}))
        }
        return None
    }
}

sub gpu_is($regex) {
    return sub($entry) {
        return $entry->{gpu} =~ $regex ? 1 : 0;
    }
}

sub rtx_usage($regex) {
    my $rtx = $overview->{NVIDIA}{RTX};
    return $rtx->keep(gpu_is $regex)->fold_mut(hash, sub($entry, $state) {
        $state->{row1} += percentage($entry->{row1})->or(0);
        $state->{row2} += percentage($entry->{row2})->or(0);
        $state->{row3} += percentage($entry->{row3})->or(0);
        $state->{row4} += percentage($entry->{row4})->or(0);
        $state->{row5} += percentage($entry->{row5})->or(0);
    })->extract(qw/row1 row2 row3 row4 row5/)
      ->all_some->or(array)
      ->map(sub($num) { sprintf "%5.2f%%", $num })
      ->expand;
}

print "\n";
Sq->fmt->table({
    data => [
        ['RTX 2000', rtx_usage(qr/RTX 2/)],
        ['RTX 3000', rtx_usage(qr/RTX 3/)],
        ['RTX 4000', rtx_usage(qr/RTX 4/)],
        ['RTX 5000', rtx_usage(qr/RTX 5/)],
    ],
});

__END__
Usage of Graphic Cards. Cards are divided into Low, Medium, High, Ultra
Low = <RTX 5050 | Medium = >5060 | High = >5070 | Ultra > 5080
Steam doesn't recognize a lot of AMD cards, so sadly they are UNKNOWN

        row1  row2  row3  row4  row5
Low     51.16 50.48 50.06 49.3  48.38
Medium  20.71 20.64 20.64 21.12 21
High    5.94  6.4   6.6   7.34  7.76
Ultra   2.25  2.7   2.69  2.91  3.03
Unknown 19.26 19.17 19.66 19.29 19.87

AMD        11.26 11.9  12.24 12.14 12.69
Intel      2.7   2.82  2.88  2.76  2.88
NVIDIA GTX 13.96 13.75 13.86 13.37 13.15
NVIDIA RTX 58.68 58.53 57.99 59.12 58.52
Other      12.72 12.39 12.68 12.57 12.80

RTX 2000  5.89%  5.58%  5.61%  5.42%  5.31%
RTX 3000 22.71% 22.12% 21.99% 21.65% 21.44%
RTX 4000 24.38% 23.99% 22.84% 23.18% 22.18%
RTX 5000  5.70%  6.84%  7.55%  8.87%  9.59%