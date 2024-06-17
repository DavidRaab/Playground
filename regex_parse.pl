#!/usr/bin/env perl
use strict;
use warnings;
use 5.036;
use Data::Dump qw(dump dd);
use Carp qw(croak);


#my $num = grep m/[wizard]/i, split //, "Wayne Drimaz";
#print $num, "\n";


my $bool       = qr/(?:true|false)/;
my $int        = qr/\d+/;
my $assignment = qr/(\w+) \s+ = \s+ ($bool|$int)/x;

sub parse($str, $pos=0) {
    pos $str = $pos;

    # End Condition
    return if $str =~ m/\G\z/gc;

    if ( $str =~ /\G($bool)/gc ) {
        if ( $1 eq "true" ) {
            return ["bool", 1], parse($str, pos($str));
        }
        else {
            return ["bool", 0], parse($str, pos($str));
        }
    }

    if ( $str =~ m/\G(\s+)/gc ) {
        return ["whitespace", $1], parse($str, pos($str));
    }

    if ( $str =~ m/\G($int)/gc ) {
        return ["int", $1], parse($str, pos($str));
    }

    if ( $str =~ m/\G$assignment/gc ) {
        return ["assignment", $1, $2], parse($str, pos($str));
    }

    # Start of Array
    if ( $str =~ m/\G \[ /gcx ) {
        my @values;
        while (1) {
            if ( $str =~ m/\G ($bool|$int) /gcx ) {
                push @values, $1;
            }
            if ( $str =~ m/\G , /xgc ) {
                redo;
            }
            if ( $str =~ m/\G \] /xgc ) {
                return \@values, parse($str, pos($str));
            }
            die (sprintf "Error at beginning of: %s", substr($str,pos($str)));
        }
    }

    return;
}

dd parse("true");
dd parse("false");
dd parse("1234");
dd parse("true     1234");
dd parse("foobar  =  123");
dd parse("foobar  =  true");
dd parse("[12,true,false,1234]");
