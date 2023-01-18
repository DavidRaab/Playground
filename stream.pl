#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Carp qw(croak);

my $range         = upto(1, 1_000_000_000);
my $squared       = imap($range, sub ($x) { $x * $x });
my $evenSquared   = filter($squared, sub ($x) { $x % 2 == 0 });
my $evenSquared10 = take($evenSquared, 10);

# execute evenSquared10 twice
iter($evenSquared10, sub { say });
say "";
iter($evenSquared10, sub ($x) { say $x });
say "";

# execute fib() twice
iter(take(fib(), 10), sub($x) { say $x });
say "";
iter(take(fib(), 10), sub($x) { say $x });
say "";

# iterate over hash
my $hash = { Foo => 1, Bar => 1 };
iter(fromHash($hash), sub($tuple) {
    my ($key, $value) = @$tuple;
    printf "Key: %s, Value: %s\n", $key, $value;
});

# cartesian product
my $keys    = wrap(qw/Foo Bar Baz/);
my $values  = wrap(1, 2);
my $product = cartesian($keys, $values);
# iter($product, sub ($val) { printf "[%s,%s]\n", $val->[0], $val->[1] });
# say "";

my $cartH = toHash($product);
p $cartH;


# creates the fibonacci sequence
sub fib {
    my $start = wrap(1,1);
    my $rest  = unfold(sub {[1,1]}, sub($state) {
        my ($x,$y) = @$state;
        my $new    = $x + $y;
        return $new, [$y, $new];
    });
    append($start, $rest);
}


# -- Iterator implemenation from here

# wrap any amount of arguments into an iterator.
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

# turn a arraref into iterator
sub fromArray($aref) {
    return wrap(@$aref);
}

# turns a hash into iterator with key,value pairs
sub fromHash($href) {
    my @data;
    while ( my ($key, $value) = each %$href ) {
        push @data, [$key, $value];
    }
    return wrap(@data);
}

# turns an iterator of tuples (array with key,value) into a hash
sub toHash($iter) {
    my $href = {};
    iter($iter, sub ($tuple) {
        my ($key, $value) = @$tuple;
        $href->{$key} = $value;
    });
    return $href;
}

# Appends two iterators into a single iterator
sub append($iterA, $iterB) {
    return sub {
        # we first initialize current to be $iterA
        my $current = $iterA->();
        my $second  = 0;
        return sub {
            NEXT:
            # As long $current returns an element we return it
            if ( defined(my $x = $current->()) ) {
                return $x;
            }
            # when it returns undef, we either need to switch
            # to $iterB or we finished
            else {
                # when we already switched to $iterB
                if ( $second ) {
                    return;
                }
                # otherwise switch to $iterB and try to re-read
                else {
                    $current = $iterB->();
                    $second  = 1;
                    goto NEXT;
                }
            }
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
            return;
        }
    }
}

# iterate through iterator, also setting $_ to current element
sub iter($iter, $f) {
    # get the internal iterator
    my $i = $iter->();
    # keep calling next element until iterator returns undef
    while ( defined(my $x = $i->()) ) {
        # set $_ to current element so user defined subroutine can use $_
        local $_ = $x;
        # call user defined subroutine
        $f->($x);
    }
    # no return value for iter because it consumes $iter
    return;
}

sub unfold($fstate, $f) {
    return sub {
        my $state = $fstate->();
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
    return unfold(sub {$start}, sub($state) {
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

sub collect($iter, $f) {
    return sub {
        my $i        = $iter->();
        my $finished = 0;         # tells if $i is exhausted
        my $current  = undef;     # will be an iterator
        return sub {
            # abort iterator when finished
            return if $finished;

            NEXT:
            # as long $current is defined we try to read from this iterator
            if ( defined $current ) {
                # read from $current and return value
                if ( defined(my $x = $current->()) ) {
                    return $x;
                }
                # if $current is exhausted
                else {
                    # set $current to undef
                    $current = undef;
                    # immediately try to re-read from $i
                    goto NEXT;
                }
            }
            # if $current is undef we try to read from $i
            else {
                # when $i returns a new element
                if ( defined(my $x = $i->()) ) {
                    # we pass it to $f and assume it returns another iterator.
                    my $iter = $f->($x);
                    # but we want to go through the internal iterator once
                    $current = $iter->();
                    # immediately try to re-read from $current
                    goto NEXT;
                }
                # when $i finished we are done
                else {
                    $finished = 1;
                    return;
                }
            }
        }
    }
}

sub cartesian($iterA, $iterB) {
    return collect($iterA, sub ($x) {
        return collect($iterB, sub ($y) {
            return wrap [$x, $y];
        });
    });
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

sub ifilter($iter, $f) {
    collect($iter, sub ($x) {
        if ( $f->($x) ) {
            wrap($x);
        }
        else {
            wrap();
        }
    });
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
