#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';
use Scalar::Util qw(refaddr reftype);

# create_special('red', 'is_red');
#
# Creates a special value and we pass it the function names it should create
sub create_special($name, $is_name) {
    # disable strict refs inside this function
    no strict 'refs';
    # the special value
    my $special = [];

    # create $name function that returns the special value
    *{ $name } = sub {
        return $special;
    };

    # create function to check if any value is $special
    *{ $is_name } = sub {
        my ($value) = @_;
        my $type = reftype($value) // "";
        # check if arrayref was passed
        if ( $type eq 'ARRAY' ) {
            # check if adresses are the same
            if ( refaddr($value) == refaddr($special) ) {
                return 1;
            }
            else {
                return 0;
            }
        }

        return 0;
    };
}

# enum_hash(color => [{value => "red", check => "is_red"}]);
#
# $values must be an array of hashes (AoH)
sub enum_hash($type, $values) {
    no strict 'refs';

    my @is_functions;
    for my $hash ( @$values ) {
        # this creates both functions
        create_special($hash->{value}, $hash->{check});
        # This way i get a reference to the created check function
        # and collect them in @is_functions
        push @is_functions, *{ $hash->{check} }{CODE};
    }

    # Now i want to create a new function `is_$type` that checks if any
    # value passed to it will return true for any function inside
    # @is_functions

    *{ "is_" . $type } = sub {
        my ($value) = @_;
        # we call every check function and return 1 if one
        # of those function returns a truish value
        for my $func ( @is_functions ) {
            return 1 if $func->($value);
        }
        return 0;
    }
}

# enum_positional(color => ["red", "green", {value => "yellow", check => "is_yellow"}])
#
# In the arrayref of enum_positional there can be passed a string or
# a hash. strings that are passed are converted to the hash-call
sub enum_positional($type, $arrayref) {
    my $args = [map {
        my $ref = reftype($_);
        # when not defined - the value $_ is a string/float
        if ( not defined $ref ) {
            {
                value => sprintf("%s_%s",    $type, $_),
                check => sprintf("is_%s_%s", $type, $_),
            }
        }
        # when already a hash - keep it unchanged
        elsif ( $ref eq 'HASH' ) {
            $_
        }
        # throw error in all other cases
        else {
            die "enum_positional only expect string or hashref.\n";
        }
    } @$arrayref];
    enum_hash($type, $args);
}

sub enum_named($args) {
    my $type   = $args->{type}   // die "Argument 'type' not passed.\n";
    my $values = $args->{values} // die "Argument 'values' not passed.\n";
    enum_positional($type, $values);
}

sub enum($data) {
    my $type = reftype($data) // "";

    if ( $type eq 'HASH' ) {
        enum_named($data);
    }
    elsif ( $type eq 'ARRAY' ) {
        for my $hash ( @$data ) {
            enum_named($hash);
        }
    }
    else {
        die "You need to either pass a hashref or arrayref to enum.\n";
    }
}

# create our special values
BEGIN {
    enum_positional(decision => [qw/yes no/]);
    enum_positional(option => [
        { value => "some", check => 'is_some' },
        "none",
    ]);
    enum({
        type   => "bool",
        values => [
            { value => 'true',  check => 'is_true'  },
            { value => 'false', check => 'is_false' },
        ],
    });
    enum([
        { type => 'color', values => [qw/red green blue/] },
        { type => 'error', values => [qw/red yellow/]     },
    ]);
}

my $cred = color_red;
my $ered = error_red;

printf("is_cred \$cred: %d\n", is_color_red($cred)); # 1
printf("is_cred \$ered: %d\n", is_color_red($ered)); # 0
printf("is_ered \$cred: %d\n", is_error_red($cred)); # 0
printf("is_ered \$ered: %d\n", is_error_red($ered)); # 1

printf("is_bool true:  %d\n",  is_bool(true));  # 1
printf("is_bool false: %d\n",  is_bool(false)); # 1
printf("is_bool \$cred: %d\n", is_bool($cred)); # 0

printf("is_decision true: %d\n",         is_decision(true));         # 0
printf("is_decision decision_yes: %d\n", is_decision(decision_yes)); # 1

my $some = some;
my $none = option_none;

printf("is_option \$some: %d\n", is_option($some));
printf("is_option \$none: %d\n", is_option($none));
