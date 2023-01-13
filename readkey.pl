#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use IO::Handle;
use Term::ReadKey;
use Curses;
use AnyEvent;

sub prompt {
    STDOUT->printflush("Press any key to exit");
}

sub clear_line {
    state $width = (GetTerminalSize(*STDOUT))[0];
    print "\r" . (" " x $width) . "\r";
}

# Periodically prints current time on terminal
my $timer = AE::timer 0, 1, sub {
    clear_line;
    print scalar localtime, "\n";
    prompt;
};

# Wait for keypress
my $key = getkey(*STDIN)->recv;
print "\n";


# returns a condvar that contains the next pressed character
sub getkey($handle) {
    my $result = AnyEvent->condvar;

    my $interval; $interval = AE::timer 0, 0.1, sub {
        ReadMode 'cbreak';
        my $key = ReadKey -1, $handle; # non-blocking read

        if ( defined $key ) {
            undef $interval;
            $result->send($key);
            ReadMode 'restore';
        }
    };

    return $result;
}