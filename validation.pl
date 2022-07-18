#!/usr/bin/env perl
use strict;
use warnings;
use v5.32;
use Carp qw(croak);

# Works Fine
helloA(
    Name     => "David",
    LastName => "Raab",
);

helloB(
    Name     => "David",
    LastName => "Raab",
);

# Throws: Key [LasstName] not supported by main::valid_arguments at ./validation.pl line 19.
helloA(
    Name      => "David",
    LasstName => "Raab",
    Invalid   => 1,
);

# Would also throw exception.
helloB(
    Name      => "David",
    LasstName => "Raab",
);


# Examples: 
# my %args = valid_arguments(["Name", "LastName"], @_);
# my $args = valid_arguments(["Name", "LastName"], @_);
sub valid_arguments {
    my ($valids, %orig) = @_;
    
    # Turns: ["A","B","C"] into {"A" => 1, "B" => 1, "C" => 1}
    my %valids = map { $_ => 1 } @$valids;
    
    # Get a list of invalid arguments
    my @invalids;
    for my $key ( keys %orig ) {
        push @invalids, $key if not exists $valids{$key};
    }
    
    # Throw error if any invalid exist
    if ( @invalids > 0 ) {
        local $Carp::CarpLevel = 2;
        my $caller = (caller 0)[3];
        
        @invalids == 1 
            ? croak (sprintf "Key [%s] not supported by %s", $invalids[0], $caller)
            : croak (sprintf "Keys [%s] not supported by %s", join(", ", @invalids), $caller);
    }
    
    # return hash in list context. hashref in scalar context.
    return wantarray ? %orig : \%orig;
}

sub helloA {
    my (%args) = valid_arguments(["Name", "LastName"], @_);
    printf "Hello %s %s\n", $args{Name}, $args{LastName};
}

sub helloB {
    my $args = valid_arguments(["Name", "LastName"], @_);
    printf "Hello %s %s\n", $args->{Name}, $args->{LastName};
}
