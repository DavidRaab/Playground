#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;

my $content = join "", <>;

my @input;
while ( $content =~ m/^ (\N+) \n (\N+) (?: \n\n | $) /xmsg ) {
    push @input, [eval $1, eval $2];
}

my $sum_idx = 0;
my $idx = 1;
for my $tuple ( @input ) {
    my ($left, $right) = @$tuple;
    my $res = compare($left, $right);
    if ( $res == -1 ) {
        $sum_idx += $idx;
    }

    printf "%03d %d\n", $idx, $res;

    $idx++;
}

printf "Sum of Right Orders: %d\n", $sum_idx;


# left is:
# -1 = smaller, 0 = equal, 1 = greater
sub compare ($left, $right) {
    my $l = shift @$left;
    my $r = shift @$right;

    # printf "L: %s\nR: %s\n", np($l), np($r);

    return  0 if !defined $l && !defined $r;
    return -1 if !defined $l &&  defined $r;
    return  1 if  defined $l && !defined $r;

    # say "BOTH DEFINED";

    # if both arrays
    if ( ref $l  &&  ref $r ) {
        my $res = compare($l, $r);
        return $res if $res;
    }

    # say "NOT ARRAYS";

    # if both numbers
    if ( !ref $l  &&  !ref $r ) {
        printf "compare %d %d\n", $l, $r;
        return -1 if $l < $r;
        return  1 if $l > $r;
        goto &compare;
    }

    # say "NOT NUMBERS";

    # number; array
    if ( !ref $l  &&  ref $r ) {
        my $res = compare([$l], $r);
        return $res if $res;
    }

    # say "NOT NA";

    # array; number
    if ( ref $l  &&  !ref $r ) {
        my $res = compare($l, [$r]);
        return $res if $res;
    }

    # say "NOT AN";

    goto &compare;
}