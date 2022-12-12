#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use List::Util qw(any);

my $processor = {
    register => 1,
    cycle    => 0,
    queue    => [],
};

# Adds a command to the queue, but does a transformation on it
sub add_command ($cmd, @args) {
    if ( $cmd eq 'noop' ) {
        push $processor->{queue}->@*, ['noop'];
    }
    elsif ( $cmd eq 'addx' ) {
        push $processor->{queue}->@*, (
            ['noop'],
            ['noop'],
            ['add', $args[0]],
        );
    }
    else {
        die "unknown command.\n";
    }
    return;
}

# Advance the processor for ony cycle
sub pnext ($processor) {
    COMMAND:
    # Do nothing if queue is empty
    return if $processor->{queue}->@* == 0;

    # otherwise read command
    my $command      = shift $processor->{queue}->@*;
    my ($cmd, @args) = @$command;

    if ( $cmd eq 'noop' ) {
        $processor->{cycle}++;
    }
    elsif ( $cmd eq 'add' ) {
        $processor->{register} = $processor->{register} + $args[0];
        goto COMMAND;
    }
    else {
        die "unknown command.\n";
    }

    return 1;
}

# parse the input and fill processor queue
while ( my $line = <> ) {
    if ( $line =~ m/\A noop \Z/xms ) {
        add_command('noop');
    }
    elsif ( $line =~ m/\A addx \s+ (-? \d+) \Z/xms ) {
        add_command('addx', $1);
    }
    else {
        die "unknown command\n";
    }
}

# p $processor;

sub show ($processor) {
    printf "Cycle: %d Register: %d\n", $processor->{cycle}, $processor->{register};
}

# run the processor
my $sum = 0;
while ( pnext($processor) ) {
    if ( any { $processor->{cycle} eq $_ } 20, 60, 100, 140, 180, 220 ) {
        show $processor;
        $sum += $processor->{cycle} * $processor->{register};
    }
}

printf "Signal strength: %d\n", $sum;
