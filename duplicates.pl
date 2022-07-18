#!/usr/bin/env perl
use strict;
use warnings;
use v5.32;
use Data::Dump qw(dump dd);
use Carp qw(croak);

my $string = "Ohne Clean Code ist Code nicht wartbar.";

my $words = splitIntoWords($string);
my $count = wordCount($words);

print "Duplikates:\n";
for my $word ( keys %$count ) {
    if ( $count->{$word} > 1 ) {
        print "$word\n";
    }
}

sub splitIntoWords {
    my ( $str ) = @_;
    return [$str =~ m/(\w+)/g];
}

sub wordCount {
    my ( $words ) = @_;
    
    my $count = {};
    for my $word ( @$words ) {
        $count->{$word}++;
    }
    return $count;
}

map { $_   } split /\s+/, $string;