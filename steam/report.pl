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
        Low    => [
            qr/RTX 5050/, qr/RTX (2050|2060|2070|2080|3050|3060|4050|4060)/,
            qr/GTX/, qr/RX (570$|570 |580$|580 |5700|6600|6650|7600)/, qr/Radeon 780M/,
            qr/Intel UHD/,
        ],
        Medium => [qr/RTX 5060/, qr/RTX 2080 (Ti|SUPER)/, qr/RTX (3070|4070)/, qr/RX (6700|6750)/],
        High   => [qr/RTX 5070/, qr/RTX (3080|3090|4080)/, qr/RX 7800/],
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
print "Low <= RTX 5050 | Medium = 5060(Ti) | High = 5070(Ti) | Ultra >= 5080\n";
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

sub select_card($regex) {
    return sub($entry) {
        if ( $entry->{gpu} =~ $regex ) {
            return Some(Str->chop($entry->{row5}))
        }
        return None
    }
}

print "\nRTX Owners\n";
printf "RTX 2000 %5.2f%%\n", $overview->{NVIDIA}{RTX}->choose(select_card qr/RTX 2/)->sum;
printf "RTX 3000 %5.2f%%\n", $overview->{NVIDIA}{RTX}->choose(select_card qr/RTX 3/)->sum;
printf "RTX 4000 %5.2f%%\n", $overview->{NVIDIA}{RTX}->choose(select_card qr/RTX 4/)->sum;
printf "RTX 5000 %5.2f%%\n", $overview->{NVIDIA}{RTX}->choose(select_card qr/RTX 5/)->sum;

__END__
Usage of Graphic Cards. Cards are divided into Low, Medium, High, Ultra
Low <= RTX 5050 | Medium = 5060(Ti) | High = 5070(Ti) | Ultra >= 5080
Steam doesn't recognize a lot of AMD cards, so sadly they are UNKNOWN

        row1  row2  row3  row4  row5
Low     54.3  53.53 52.99 52.2  51.17
Medium  15.96 16.03 16.1  16.68 16.65
High    7.55  7.96  8.21  8.88  9.32
Ultra   2.25  2.7   2.69  2.91  3.03
Unknown 19.26 19.17 19.66 19.29 19.87

AMD        11.26 11.9  12.24 12.14 12.69
Intel      2.7   2.82  2.88  2.76  2.88
NVIDIA GTX 13.96 13.75 13.86 13.37 13.15
NVIDIA RTX 58.68 58.53 57.99 59.12 58.52
Other      12.72 12.39 12.68 12.57 12.80

RTX Owners
RTX 2000  5.31%
RTX 3000 21.44%
RTX 4000 22.18%
RTX 5000  9.59%
