#!/usr/bin/env perl
use 5.040;
use utf8;
use open ':std', ':encoding(UTF-8)';
use Sq -sig => 1;

sub to_entry($array) {
    state $entry = record(qw/gpu row1 row2 row3 row4 row5/);
    $entry->(@$array);
}

my $data =
    Sq->fs
    ->read_text('2026-01-15-steam_survey.txt')
    ->chunked(8)
    ->map(call 'slice', 0 .. 5)
    ->map(\&to_entry)
    ->cache;

# my $cards = $data->map(key 'gpu')->sort(by_str);
# dump $cards;

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
