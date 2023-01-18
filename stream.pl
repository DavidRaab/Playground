#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Carp qw(croak);

# my $iter   = range(1,10);
# my $double = imap($iter, sub($x) { $x * 2  });
# my $square = imap($iter, sub($x) { $x * $x });

# iter($double, sub($x) {
#     printf "%d\n", $x;
# });
# say "";

# iter($square, sub($x) {
#     printf "%d\n", $x;
# });
# say "";

# my $evenSquared = filter($square, sub($x) { $x % 2 == 0 });
# iter($evenSquared, sub($x) {
#     printf "%d\n", $x;
# });
# say "";

# my $evenSquared2 = take($evenSquared, 2);
# iter($evenSquared2, sub($x) {
#     printf "%d\n", $x;
# });
# say "";

my $range         = upto(1, 1_000_000_000);
my $squared       = imap($range, sub ($x) { $x * $x });
my $evenSquared   = filter($squared, sub ($x) { $x % 2 == 0 });
my $evenSquared10 = take($evenSquared, 10);

iter($evenSquared10, sub ($x) { say $x });
say "";
iter($evenSquared10, sub ($x) { say $x });
say "";
iter(take(fib(), 20), sub($x) { say $x });
say "";


sub fib {
    return unfold([1,1], sub($state) {
        my ($x,$y) = @$state;
        my $new    = $x + $y;
        return $new, [$y, $new];
    });
}


# -- Iterator implemenation from here

sub wrap(@vars) {
    return sub {
        my $length = @vars;
        my $i      = 0;
        return sub {
            if ( $i < $length ) {
                my $x = $vars[$i];
                $i++;
                return $x;
            }
            return;
        }
    }
}

sub range($start, $stop) {
    return sub {
        my $current = $start;
        return sub {
            if ( $current <= $stop ) {
                return $current++;
            }
            else {
                undef;
            }
        }
    }
}


sub iter($iter, $f) {
    my $i = $iter->();
    while ( defined(my $x = $i->()) ) {
        $f->($x);
    }
    return;
}

sub unfold($state, $f) {
    return sub {
        my $state = $state;
        my $value = undef;
        return sub {
            if ( defined $state ) {
                ($value, $state) = $f->($state);
                return $value;
            }
            return;
        }
    }
}

sub upto($start, $stop) {
    return unfold($start, sub($state) {
        return ++$state <= $stop ? ($state,$state) : undef;
    });
}

sub imap($iter, $f) {
    return sub {
        my $i = $iter->();
        return sub {
            if ( defined(my $x = $i->()) ) {
                return $f->($x);
            }
            return;
        }
    }
}

sub filter($iter, $f) {
    return sub {
        my $i = $iter->();
        return sub {
            while ( defined(my $x = $i->()) ) {
                return $x if $f->($x);
            }
            return;
        }
    }
}

sub take($iter, $amount) {
    return sub {
        my $i     = $iter->();
        my $count = 0;
        return sub {
            if ( $count < $amount ) {
                $count++;
                if ( defined(my $x = $i->()) ) {
                    return $x;
                }
            }
            return;
        }
    }
}
