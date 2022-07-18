#!/usr/bin/env perl
use strict;
use warnings;
use v5.32;
use Data::Dump qw(dump dd);
use Carp qw(croak);
use File::Slurp;
use Benchmark qw(cmpthese);
use List::Util qw(reduce);
use Memoize;

my $text = read_file("LoremIpsum.txt");

sub duplicates {
    my ( $text ) = @_;
    
    my %words;
    for my $word ( $text =~ m/\w+/g ) {
        $words{$word}++;
    }
    
    my @result;
    for my $word ( keys %words ) {
        if ( $words{$word} > 1 ) {
            push @result, $word;
        }
    }
    
    return \@result;
}

sub duplicates_split {
    my ( $text ) = @_;
    
    my %words;
    for my $word ( split / /, $text ) {
        $words{$word}++;
    }
    
    my @result;
    for my $word ( keys %words ) {
        if ( $words{$word} > 1 ) {
            push @result, $word;
        }
    }
    
    return \@result;
}




sub contains {
    my ( $element, $array ) = @_;
    for my $x ( @$array ) {
        if ( $x eq $element ) {
            return 1;
        }
    }
    return 0;
}
memoize('contains', 
    SCALAR_CACHE => 'MERGE',
    NORMALIZER   => sub { $_[0] },
);

sub contains2 {
    my ( $element, $array ) = @_;
    my $found = 0;
    for my $x ( @$array ) {
        if ( $x eq $element ) {
            if ( ++$found == 2 ) {
                return 1;
            }
        }
    }
    return 0;
}
memoize('contains2', 
    SCALAR_CACHE => 'MERGE',
    NORMALIZER   => sub { $_[0] },
);

sub duplicates2 {
    my ( $text ) = @_;
    
    my @words = $text =~ m/\w+/g;
    my @result;
    
    for my $word ( @words ) {
        if ( contains2($word, \@words) ) {
            if ( not contains($word, \@result) ) {
                push @result, $word;
            }
        }
    }
    
    return \@result;
}

sub duplicates3 {
    my ( $text ) = @_;
    
    my @words = $text =~ m/\w+/g;
    my @result;
    
    for my $word ( @words ) {
        my $found = 0;
        for my $x ( @words ) {
            if ( $x eq $word ) {
                last if ++$found == 2;
            }
        }
        
        if ( $found == 2 ) {
            my $found = 0;
            for my $x ( @result ) {
                if ( $x eq $word ) {
                    last if ++$found;
                }
            }
            
            if ( not $found ) {
                push @result, $word;
            }
        }
    }
    
    return \@result;
}

duplicates2 $text;

#printf "AVG: %.2f\n", (reduce { $a + $b } @depths) / $#depths;
#@depths = sort { $a <=> $b } @depths;
#print "Mean: ", ($depths[$#depths/2]), "\n";



cmpthese(1000, {
    "Hash"   => sub { duplicates       $text },
    "Split"  => sub { duplicates_split $text },
    #"Array1" => sub { duplicates2 $text }, # Toooooo Slow
    #"Array2" => sub { duplicates3 $text }, # Still tooo Slow
});
