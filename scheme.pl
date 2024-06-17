#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
# use Getopt::Long::Descriptive;

# my ($opt, $usage) = describe_options(
#     'Usage: %c %o',
#     ['help|h', 'Print this message', {shortcircuit => 1}],
# );

# $usage->die if $opt->help;

# executor dispatch-table
my $execute = {
    'add1' => sub($x) { $x + 1 },
    'sub1' => sub($x) { $x - 1 },
    '+' => sub(@args) {
        my $sum = 0;
        $sum += $_ for @args;
        return $sum;
    },
    '*' => sub(@args) {
        my $sum = 1;
        $sum *= $_ for @args;
        return $sum;
    },
    'sqrt'      => sub($arg)  { sqrt $arg },
    'displayln' => sub(@args) {
        print for @args;
        say "";
    },
};

# printer dispatch-table
my $printer = {
    '+'         => sub(@args) { '(+ ' . join(' ', @args) . ')'         },
    '*'         => sub(@args) { '(* ' . join(' ', @args) . ')'         },
    'sqrt'      => sub($arg)  { sprintf '(sqrt %f)', $arg              },
    'displayln' => sub(@args) { '(displayln ' . join(" ", @args) . ')' },
};

# Long examples to execute AST with any given dispatch-table
# say eval_ast($execute, $ast);
# say eval_ast($printer, $ast);

# Example AST
my $ast =
    ['*',
        ['sqrt', 2],
        ['+', 2, ['*', 4, 6]],
        ['+', 3, 5, 7]];

# prints: 551.543289325507
say run($ast);

# prints 5
say run(parse_scheme('(+ 2 3)'));

# prints: 3, 4, 468
run(parse_scheme('
    (displayln 3)
    (displayln 4)
    (displayln
        (*
            (+ 1 2)
            (+ 3 (+ 4 5))
            (+ 6 7)))'));

# prints 12
say run(['*', 4, 3]);

# parses string to scheme, and converts it back to string
say to_string(parse_scheme('
    (displayln 1)
    (+ 1 2)
'));

# prints 21
say run(parse_scheme('
  (+ 1
     2
     (+ 3 4)
     (+ 5 6))'));

# prints 4
say run(parse_scheme('(add1 3)'));

# prints 2
say run(parse_scheme('(sub1 3)'));



###---------
#- Executing AST with dispatch-table and Scheme Parsing Implementation
#- from here on
###---------

# check if we have a function. An array is seen as a function that contains
# the function name as its first value
sub is_func($expr) {
    return ref $expr eq 'ARRAY' ? 1 : 0;
}

sub eval_ast($dispatch, @exprs) {
    # multiple expression can be passed, the return value of
    # each one is stored and returned
    my @rets;

    # we can pass multiple expressions, don't need to be a single function
    for my $expr ( @exprs ) {
        # when $expr is a function we want to execute it
        if ( is_func($expr) ) {
            # first array entry is function-name
            my $func = $expr->[0];

            # everything else is an argument to that function. But they can be
            # expression again, including other function calls. so we need
            # to recursively call eval_ast on the arguments to "execute"
            # those expressions.
            my @args;
            for ( my $idx=1; $idx < @$expr; $idx++ ) {
                push @args, eval_ast($dispatch, $expr->[$idx]);
            }

            # finally once all arguments are computed, we can execute the
            # current function by getting the function from the
            # dispatch-table and pass it the computed @args
            if ( exists $dispatch->{$func} ) {
                push @rets, $dispatch->{$func}(@args);
            }
            # when AST contains a function name and the user didn't pass
            # an implementation for that function, we throw an error
            else {
                die sprintf('Cannot find func "%s" in $dispatch', $func);
            }
        }
        # everything else is returned as-is
        else {
            push @rets, $expr;
        }
    }

    return @rets;
}

# directly execute AST with $execute dispatch-table to run code
sub run(@exprs) {
    return eval_ast($execute, @exprs);
}

# converts AST back to string by using $printer dispatch-table
sub to_string(@exprs) {
    return eval_ast($printer, @exprs);
}

# parses a Scheme string to an AST
# no kind of error handling when string is incorrect
sub parse_scheme($str) {
    # everything not-whitespace and parenthesis
    state $expr = qr/[^\s()]+/;

    my @scopes;
    my $scope  = [];

    # The first scope is also the result
    my $result = $scope;

    while (1) {
        # End Condition - end of string
        return @$result if $str =~ m/\G\z/gc;

        # EXPR
        if ( $str =~ m/\G ($expr) /xmsgc ) {
            push @$scope, $1;
            next;
        }

        # whitespace
        next if $str =~ m/\G \s+ /xmsgc;

        # end of function
        if ( $str =~ m/\G \) /xmsgc ) {
            # when function is closed, we need to restore previous scope
            $scope = pop @scopes;
        }

        # start of function
        if ( $str =~ m/\G \( \s* ($expr) /xmsgc ) {
            # a new function is found, so we have to keep track of the current
            # scope first. so when the new function is closed, we can restore
            # to the previously scope
            push @scopes, $scope;
            # then we create an array to hold the function name
            my $func = [$1];
            push @$scope, $func;
            $scope = $func;
        }
    }

    return @scopes;
}
