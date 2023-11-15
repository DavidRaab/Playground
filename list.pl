#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Getopt::Long::Descriptive;

my ($opt, $usage) = describe_options(
    'Usage: %c %o',
    ['help|h', 'Print this message', {shortcircuit => 1}],
);

$usage->die if $opt->help;

package List;
use v5.36;
use Sub::Exporter -setup => {
    exports => [qw/null isNull cons car cdr list/],
};
use overload "==" => sub($self, $other, $swap) {
    return equal($self, $other);
};

# Basic "Constructors"
sub null               { return bless([],'List') }
sub isNull($list)      { @$list == 0             }
sub cons($head, $tail) { return bless([$head, $tail],'List') }
sub car($list)         { return $list->[0]     }
sub cdr($list)         { return $list->[1]     }

sub list {
    my @args = @_;
    return null if @args == 0;
    my $head = shift @args;
    return cons($head, null) if @args == 0;
    return cons($head, list(@args));
}

# "Methods"

sub count($list) {
    return 0 if isNull $list;
    return 1 + count(cdr($list));
}

sub equal($list1, $list2) {
    return 1 if isNull($list1) && isNull($list2);
    return 0 if isNull($list1);
    return 0 if isNull($list2);
    if ( car($list1) == car($list2) ) {
        return equal(cdr($list1), cdr($list2));
    }
    else {
        return 0;
    }
}

sub map($list, $f) {
    return null if isNull($list);
    return cons($f->(car($list)), __SUB__->(cdr($list), $f));
}

sub append($xs, $ys) {
    return $ys if isNull $xs;
    return cons(
        car($xs),
        append(cdr($xs), $ys)
    );
}

sub filter($xs, $predicate) {
    return null if isNull $xs;

    if ( $predicate->(car($xs)) ) {
        return cons(car($xs), filter(cdr($xs), $predicate));
    }
    else {
        return filter(cdr($xs), $predicate);
    }
}

sub rev($xs, $ys = []) {
    return $ys if isNull $xs;
    return rev(cdr($xs), cons(car($xs), $ys));
}

## MAIN
package main;
BEGIN { List->import(qw/null isNull cons car cdr list/) }

use Test2::Bundle::More;
use Data::Dumper qw(Dumper);

my $list1 = cons(1, cons(2, cons(3, cons(4, null))));
my $list2 = list(1,2,3,4);

cmp_ok($list1->count, '==', $list2->count, "list have same count");
cmp_ok($list1->count, '==', 4, 'count is 4');
ok($list1 == $list2, 'lists are equal');
ok(not (list(1,2,3) == list(1,2,3,4)), 'lists are not equal');
ok(list == list, 'two equal empty lists');

my $add1    = sub($x) { $x + 1      };
my $double  = sub($x) { $x * 2      };
my $square  = sub($x) { $x * $x     };
my $is_even = sub($x) { $x % 2 == 0 };

ok($list1->map($double) == list(2,4,6,8), 'lmap');
ok($list1 == list(1,2,3,4), 'lmap does not modify $list1');
ok(
    list(1,2,3,4)->map($square) == list(0,3,8,15)->map($add1),
    'two lmap are equal');

ok(list(1,2,3)->append(list(4,5,6)) == list(1 .. 6), 'append');
ok(list(1..10)->filter($is_even) == list(2,4,6)->append(list(8,10)), 'filter');
ok(null->filter(sub{}) == null, 'filter on null');
ok(null->map(sub{})   == null, 'map on null');

ok(
    list(1..10)
    ->map($square)
    ->filter($is_even) == list(4, 16, 36, 64, 100),
    'filter on map');

ok(list(1..5)->rev == list(5,4,3,2,1), 'rev');

done_testing;
