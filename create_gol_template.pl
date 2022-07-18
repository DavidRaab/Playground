#!/usr/bin/env perl
use strict;
use warnings;
use v5.10;
use Data::Dumper qw(Dumper);
use Carp qw(croak);
use FindBin qw($Script);

die "$Script width height [dead|alive]\n" if @ARGV == 0;

my $width  = 0;
my $height = 0;
my $state  = "dead";

if (@ARGV == 2 or @ARGV == 3) {
    $width  = $ARGV[0];
    $height = $ARGV[1];
}

$state = $ARGV[2] if @ARGV == 3;
if (not $state =~ m/\A(alive|dead)\z/) {
    die "$Script: [$state] now allowed must be: [alive|dead]\n";
}

die "$Script: width  should be greater 0\n" if $width  == 0;
die "$Script: height should be greater 0\n" if $height == 0;


for my $y (1..$height) {
    for my $x (1..$width) {
        printf ".";
    }
    printf "\n";
}
