#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Getopt::Long::Descriptive;
use Time::HiRes qw(time);

my ($opt, $usage) = describe_options(
    'Usage: %c %o',
    ['help|h', 'Print this message', {shortcircuit => 1}],
);

$usage->die if $opt->help;

my $amount = 50_000;

# Hash
my %dic;
for my $i ( 1 .. $amount ) {
    $dic{$i} = rand();
}

# Array
my @array = map { [$_,rand] } 1 .. $amount;

# Benchmark Hash
my ($sum1, $time1) = benchmark(sub(){
    my $sum = 0;
    while ( my ($key,$value) = each %dic ) {
        $sum += $key;
    }
    return $sum;
});
printf "Dic1 Sum %d Time %f\n", $sum1, $time1;

# Benchmark Array
my ($sum2, $time2) = benchmark(sub() {
    my $sum = 0;
    for my $tuple ( @array ) {
        $sum += $tuple->[0];
    }
    return $sum;
});
printf "Arr1 Sum %d Time %f\n", $sum2, $time2;



sub benchmark($f) {
    my $start = time();
    my $ret   = $f->();
    my $stop  = time();
    return ($ret, $stop-$start);
}