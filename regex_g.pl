#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;

*perlArray   = sub($str) { parseArray(qw/\[ , \]/, $str) };
*fsharpArray = sub($str) { parseArray(qw/\[ ; \]/, $str) };
*lispArray   = sub($str) { parseArray(qr{\(}, qr/\s+/, qr{\)}, $str) };


my $data1 = perlArray("[1,2,3,4]");
p $data1;

my $data2 = perlArray("[1,2,3,[4,5,6]]");
p $data2;

my $data3 = perlArray("[1,[2,3,[4,5,[6,7]],8,9],11]");
p $data3;

# my $data4 = parseArray(qw/\[ , \]/, "[1,,2]");
# p $data4;

my $data5 = fsharpArray("[1;2;3;4;[5;6;7]]");
p $data5;

my $data6 = lispArray("(1 2 3 4 (5 6 7))");
p $data6;

sub parseArray($start, $del, $end, $str) {
    my @final;
    my @arrays  = \@final;
    my $current = \@final;

    # check if string starts with [
    if ( $str =~ m/\G $start /gxmsc ) {
        # continue until end of string
        while ( $str !~ m/\G\z/gxmsc ) {
            # start of array
            if ( $str =~ m/\G $start /gxmsc ) {
                my @new;
                push @arrays, \@new;
                push @$current, \@new;
                $current = \@new;
            }
            # end of array
            elsif ( $str =~ m/\G $end /gxmsc ) {
                pop @arrays;
                $current = $arrays[-1];
            }
            # digit
            elsif ( $str =~ m/\G (\d+) (?= $del | $end ) /gxmsc ) {
                push @$current, $1;
            }
            # delimeter
            elsif ( $str =~ m/\G $del (?= \d+ | $start )/gxmsc ) {
                # just check if a comma follow with another digit or array
            }
            else {
                die (sprintf "Invalid input at pos %d\n", pos($str));
            }
        }
    }

    return \@final;
}
