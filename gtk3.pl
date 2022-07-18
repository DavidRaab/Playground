#!/usr/bin/env perl
use strict;
use warnings;
use v5.32;
use Data::Dump qw(dump);
use Gtk3 -init;

#my $window = Gtk3::Window->new('toplevel');
#my $button = Gtk3::Button->new('Quit');

#$button->signal_connect(clicked => sub { Gtk3::main_quit });
#$window->add($button);
#$window->show_all;


# Window definition
my $window =
    window('toplevel',
        button('Quit', sub { Gtk3::main_quit }),
    );


# Main Loop
Gtk3::main;

# Helper Functions
sub button {
    my ($name, $cb) = @_;
    my $b = Gtk3::Button->new($name);
    $b->signal_connect(clicked => $cb);
    return $b;
}

sub window {
    my ($name, @elements) = @_;
    my $w = Gtk3::Window->new($name);
    for my $x (@elements) {
        $w->add($x);
    }
    $w->show_all;
    return $w;
}
