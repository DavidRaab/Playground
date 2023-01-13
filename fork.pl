#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use IO::Handle;
use AnyEvent;
use AnyEvent::Util;

my @pids;
my $cv = AnyEvent->condvar;

for ( 1 .. 10 ) {
    $cv->begin;
    my $pid = fork_call {
        open STDOUT, '>', '/dev/null';
        system('ffmpeg', '-version');
        return $$;
    } sub ($pid) {
        push @pids, $pid;
        $cv->end;
    }
}

$cv->recv;

printf "%d processes forked: @pids\n", scalar @pids;

