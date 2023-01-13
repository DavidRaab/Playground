#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Gtk3 -init;
use AnyEvent;

############################################
# create a window and some label

my $window = Gtk3::Window->new("toplevel");
$window->set_default_size(200,200);
my $label  = Gtk3::Label->new("soon replaced by name");
$window->add($label);
$window->show_all;

p $window;

############################################
# do our AnyEvent stuff

$| = 1; print "enter your name> ";

my $wait_for_input = AnyEvent->io (
   fh => \*STDIN, poll => "r",
   cb => sub {
      # set the label
      $label->set_text(scalar <STDIN>);
      print "enter another name> ";
   }
);

############################################
# Now enter Gtk2's event loop

Gtk3->main;