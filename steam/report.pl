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

my $amd_owners = $overview->{AMD}        ->map(key 'row5')->sum_by(Str->chop);
my $nv_gtx     = $overview->{NVIDIA}{GTX}->map(key 'row5')->sum_by(Str->chop);
my $nv_rtx     = $overview->{NVIDIA}{RTX}->map(key 'row5')->sum_by(Str->chop);
my $intel      = $overview->{Intel}      ->map(key 'row5')->sum_by(Str->chop);

Sq->fmt->table({
    data => [
        ['AMD',        sprintf('%.2f%%', $amd_owners)],
        ['NVIDIA GTX', sprintf('%.2f%%', $nv_gtx)],
        ['NVIDIA RTX', sprintf('%.2f%%', $nv_rtx)],
        ['Intel',      sprintf('%.2f%%', $intel)],
        ['Other',      $overview->{Other}[0]{row5}],
    ],
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
AMD        12.69%
NVIDIA GTX 13.15%
NVIDIA RTX 58.52%
Intel      2.88%
Other      12.80%

RTX Owners
RTX 2000  5.31%
RTX 3000 21.44%
RTX 4000 22.18%
RTX 5000  9.59%
