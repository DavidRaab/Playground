#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use AnyEvent;
use IO::Handle;

STDOUT->printflush("enter your name> ");

my $name_async = AnyEvent->condvar;

my $wait_for_input = AnyEvent->io (
   fh   => \*STDIN,
   poll => "r",
   cb   => sub {
        my $n = <STDIN>;
        $name_async->send($n);
   }
);

# # do something else ...
my $timer = AnyEvent->timer(
    after    => 1,
    interval => 1,
    cb => sub {
        print scalar localtime, "\n";
    }
);

# now wait until the name is available:
my $name = $name_async->recv;
print "\nEcho: $name\n";

# watcher no longer needed
undef $wait_for_input;
