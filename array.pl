#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';
use Devel::Size qw(total_size);
use Devel::Peek qw(Dump);
use Getopt::Long::Descriptive;

my ($opt, $usage) = describe_options(
    'Usage: %c %o',
    ['help|h', 'Print this message', {shortcircuit => 1}],
);

$usage->die if $opt->help;

# Size of a normal integer
printf "Size of 0: %d bytes.\n", (total_size 0);

# This prints the interal C represenation of 0
# See: Devel::Peek for explanation
d('Just 0', 0);

# Look how the value changes by changing its type
my $x = "Hello";
d('my $x = "Hello";', $x);

$x = $x;
d('$x = 10;', $x);

# How much memory does it use in Perl?
my @array;
my $max = 100_000_000;
for (my $i=0; $i < $max; $i++) {
    push @array, $i;
}

my $size = total_size(\@array);

printf "\n";
printf "Array with %.02f Mio items\n", to_mega($max);
printf "Size: %.02f MiB\n", to_mega($size);

# Can vary on CPU, OS, ... my results
#  10 Mio =  320.73 MiB
# 100 Mio = 3263.72 MiB

# So one item in Perl (even just a number, maybe a 64bit float?)
# consumes 32 bytes of memory (in an array). It is 4 x more memory
# (800 Mib)  than minimal needed. Expecting a 64 bit int/float.

sub to_mega($number) {
    return ($number / 1000 / 1000);
}

sub d($str, $obj) {
    warn "\n$str\n";
    Dump($obj);
}
