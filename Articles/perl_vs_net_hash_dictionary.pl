#!/usr/bin/env perl
use strict;
use warnings;
use utf8;
use open ':std', ':encoding(UTF-8)';
use v5.32;
use Data::Printer;

my $wordMap = {};
for my $word (qw/Hello There who are you? Missing something?/) {
    my $key = length $word;
    push @{$wordMap->{$key}}, $word;
}

p $wordMap;
