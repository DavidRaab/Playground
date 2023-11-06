#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';
use Data::Printer;

say "Random 1 to 6\n";
my %times;
for ( 1 .. 600_000 ) {
    my $rand = int(rand(6)) + 1;
    $times{$rand}++;
}

p %times;


say "Random two dice added\n";
%times = ();
for ( 1 .. 600_000 ) {
    my $a = int(rand(6)) + 1;
    my $b = int(rand(6)) + 1;
    $times{$a+$b}++;
}
p %times;
