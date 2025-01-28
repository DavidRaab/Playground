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


package Stuff;
use 5.036;

sub gen_push_array($field) {
    return sub($self, @args) {
        push @{ $self->{$field} }, @args;
    }
}

sub new($class) {
    return bless({options => []}, $class);
}

# getter
sub options($self) {
    return $self->{options};
}

no warnings;
*add_option = gen_push_array('options');

package main;

my $stuff = Stuff->new;
$stuff->add_option('red', 'blue');

for my $x ( $stuff->options->@* ) {
    say $x;
}
