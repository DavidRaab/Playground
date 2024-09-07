#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Getopt::Long::Descriptive;

my ($opt, $usage) = describe_options(
    'Usage: %c %o',
    ['help|h', 'Print this message', {shortcircuit => 1}],
);

$usage->die if $opt->help;

my @array = ( 10 .. 20 );

for my $idx ( -5 .. 5 ) {
    printf "%+02d: %d\n", $idx, $array[$idx];
}

print "index of -30: ";
say $array[-30];