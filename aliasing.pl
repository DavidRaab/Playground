#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Getopt::Long::Descriptive;
use Benchmark qw(cmpthese);

my ($opt, $usage) = describe_options(
    'Usage: %c %o',
    ['help|h', 'Print this message', {shortcircuit => 1}],
);

$usage->die if $opt->help;

my @array = 1 .. 10_000;

sub noop {}

cmpthese(-1, {
    'alias' => sub {
        noop(@array);
    },
    'reference' => sub {
        noop(\@array);
    },
});
