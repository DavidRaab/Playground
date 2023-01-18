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
my $squared       = imap2($range, sub ($x) { $x * $x });
my $evenSquared   = ifilter($squared, sub ($x) { $x % 2 == 0 });
my $evenSquared10 = take2($evenSquared, 10);

iter($evenSquared10, sub ($x) { say $x });
say "";
iter($evenSquared10, sub ($x) { say $x });
say "";
iter(take2(fib(), 20), sub($x) { say $x });
say "";


sub fib {
    return unfold([1,1], sub($state) {
        my ($x,$y) = @$state;
        my $new    = $x + $y;
        return $new, [$y, $new];
    });
}


# -- Iterator implemenation from here

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
    while ( my $x = $i->() ) {
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
        # if ( ++$state <= $stop ) {
        #     return $state, $state;
        # }
        # else {
        #     return;
        # }
    });
}

sub yield($value,$state=undef) { return ([YIELD => $value], $state) }
sub skip()                     { return [YIELD => undef]            }
sub stop()                     { return undef                       }
sub is_yield($value) {
    ref $value eq 'ARRAY'
    && @$value == 2
    && $value->[0] eq 'YIELD'
}

# [YIELD, 10]    => Keep value 10
# [YIELD, undef] => Skip Value
# undef          => Abort iteration
sub ifor($iter, $state, $f) {
    return sub {
        my $i       = $iter->();
        my $running = 1;
        my $state   = $state;
        my $new;
        return sub {
            while ( $running && defined(my $x = $i->()) ) {
                ($new, $state) = $f->($x, $state);
                if ( defined($new) ) {
                    if ( is_yield $new ) {
                        return $new->[1] if defined $new->[1];
                        next;
                    }
                    else {
                        croak "lambda function in ifor must return yield or undef";
                    }
                }
                else {
                    $running = 0;
                    return;
                }
            }
            return;
        }
    }
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

sub imap2($iter, $f) {
    return ifor($iter, undef, sub($x, $state) {
        return yield $f->($x);
    });
}

sub filter($iter, $f) {
    return sub {
        my $i = $iter->();
        return sub {
            while ( defined(my $x = $i->()) ) {
                if ( $f->($x) ) {
                    return $x;
                }
            }
            return;
        }
    }
}

sub ifilter($iter, $f) {
    return ifor($iter, undef, sub($x, $state) {
        return $f->($x) ? yield $x : skip;
    });
}

sub take($iter, $amount) {
    return sub {
        my $i             = $iter->();
        my $returnedSoFar = 0;
        return sub {
            if ( $returnedSoFar < $amount ) {
                $returnedSoFar++;
                if ( defined(my $x = $i->()) ) {
                    return $x;
                }
            }
            return;
        }
    }
}

sub take2($iter, $amount) {
    return ifor($iter, 0, sub ($x, $state) {
        return $state < $amount ? yield $x, $state+1 : stop;
    });
}