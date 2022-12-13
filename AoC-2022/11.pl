#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Getopt::Long;

my $rounds = 1;
GetOptions(
    'r|rounds=i' => \$rounds,
) or die "Error parsing command line arguments\n";


sub divisible ($num, $true, $false) {
    return sub ($item) {
        return $item % $num == 0 ? $true : $false;
    }
}

sub add ($x) { return sub ($old) { $old + $x } }
sub mul ($x) { return sub ($old) { $old * $x } }

my @monkeys = (
    # 0
    {
        items   => [89, 95, 92, 64, 87, 68],
        inspect => mul(11),
        test    => divisible(2,7,4),
    },
    # 1
    {
        items   => [87,67],
        inspect => add(1),
        test    => divisible(13,3,6),
    },
    # 2
    {
        items   => [95, 79, 92, 82, 60],
        inspect => add(6),
        test    => divisible(3,1,6),
    },
    # 3
    {
        items   => [67, 97, 56],
        inspect => sub ($old) { $old * $old },
        test    => divisible(17,7,0),
    },
    # 4
    {
        items   => [80, 68, 87, 94, 61, 59, 50, 68],
        inspect => mul(7),
        test    => divisible(19,5,2),
    },
    # 5
    {
        items   => [73, 51, 76, 59],
        inspect => add(8),
        test    => divisible(7,2,1),
    },
    # 6
    {
        items   => [92],
        inspect => add(5),
        test    => divisible(11,3,0),
    },
    #7
    {
        items   => [99, 76, 78, 76, 79, 90, 89],
        inspect => add(7),
        test    => divisible(5,4,5),
    },
);

for my $round ( 1 .. $rounds ) {
    for my $monkey ( @monkeys ) {
        while ( my $item = shift $monkey->{items}->@* ) {
            $monkey->{inspected}++;
            my $inspect     = $monkey->{inspect}($item);
            my $new         = int ($inspect / 3);
            my $next_monkey = $monkey->{test}($new);
            # printf "Inspect %d New %d Next %d\n", $inspect, $new, $next_monkey;
            push $monkeys[$next_monkey]{items}->@*, $new;
        }
    }
    printf "After Round %d\n", $round;
    show_items(@monkeys);
    print "\n";
}

# p @monkeys;

my ($fst, $snd) = sort { $b <=> $a } map { $_->{inspected} } @monkeys;
printf "Monkey Business: %d * %d = %d\n", $fst, $snd, $fst * $snd;



sub show_items (@monkeys) {
    my $idx = 0;
    for my $monkey ( @monkeys ) {
        printf "Monkey %d: %s\n", $idx++, join(", ", $monkey->{items}->@*);
    }
}
