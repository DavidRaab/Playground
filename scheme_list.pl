#!/usr/bin/env perl
use v5.36;
# use Sq -sig => 1;

# Example picked from SICP: Page 125

# (define (cons x y)
#     (lambda (m) (m x y)))
# (define (car z)
#     (z (lambda (p q) p)))

sub cons($x, $y) {
    return sub($m) {
        $m->($x, $y);
    }
}

sub car($cons) {
    $cons->(sub($x,$y){ $x });
}

sub cdr($cons) {
    $cons->(sub($x,$y){ $y });
}

sub is_empty($cons) {
    return 1 if !defined $cons;
    return 0;
}

sub fold($list, $state, $f) {
    return $state if is_empty($list);
    my $head = car($list);
    return $f->($head, fold(cdr($list), $state, $f));
}

sub mapl($list, $f) {
    fold($list, undef, sub($x,$rest) { cons($f->($x), $rest) })
}

sub to_string($list) {
    '[' . (fold($list, "", sub($x,$rest) { $rest eq "" ? $x : "$x,$rest" })) . ']';
}

my $list   = cons(1, cons(2, cons(3, cons(4, undef))));
say to_string($list);

my $double = mapl($list, sub($x) { $x * 2 });
say to_string($double);

# say car($list);      # 1
# say car(cdr($list)); # 2
