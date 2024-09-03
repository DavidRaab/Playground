#!/usr/bin/env perl
use v5.36;
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use List::Util qw(reduce);
use Text::CSV;
use Getopt::Long::Descriptive;
use Benchmark qw(cmpthese);

my ($opt, $usage) = describe_options(
    'Usage: %c %o',
    ['benchmark|b', 'Enable Benchmark'  , {default      => 0}],
    ['help|h',      'Print this message', {shortcircuit => 1}],
);
$usage->die if $opt->help;


# Example of Parsing CSV as Data
my $csv = Text::CSV->new({
    sep_char        => ',',
    skip_empty_rows => 1,
});
$csv->header(*DATA);

my @data;
while (my $row = $csv->getline_hr(*DATA)) {
    push @data, $row;
}

# Sorting data by id
@data = sort { $a->{id} <=> $b->{id} } @data;

# Generate some big test-data
my $current_id = 0;
my @big_data = map {
    # At every iteration add 1-3 to $current_id. This way i get unique
    # "random" ids
    $current_id += (int rand(3)) + 1;
    my $id   = $current_id;
    my $name = "foo";
    { id => $id, name => $name }
} 1 .. 100_000;

# get the maximum id from data
my $max_id = reduce { $a > $b->{id} ? $a : $b->{id} } 0, @big_data;

# Testing
use Test2::Bundle::More;

# Static tests
{
    my @checks = (
        [4,  -1],
        [5,   0],
        [6,   1],
        [7,   2],
        [8,   3],
        [9,   4],
        [10,  5],
        [11, -1],
    );

    for my $check ( @checks ) {
        my $search   = $check->[0];
        my $expected = $check->[1];

        my $idx = binary_search({
            data   => [5..10],
            search => $search,
        });

        cmp_ok($idx, '==', $expected, 'Static Tests');
    }
}

my $comparer = sub { $a <=> $b->{id} };

# Test on @data
my $found_binary = binary_search({
    data     => \@data,
    search   => 10,
    comparer => $comparer,
});

## search for 10
my $key_id       = sub { $_->{id} };
my $found_linear = linear_search(10, $key_id, \@data);
cmp_ok($found_binary, '==', $found_linear, 'Same result on @data');
cmp_ok($found_binary, '==', 3,             'Found at index 3');

## search for a key that doesn't exists
cmp_ok(
    linear_search(4, $key_id, \@data),
    '==',
    binary_search({search => 4, comparer => $comparer, data => \@data}),
    'search id == 4'
);

# Test on @big_data

## Pick an existing key - we now that there are 100_000 elements in it
## i pick some in the middle
my $search_for = $big_data[50_000]{id};
my $bs = binary_search({search => $search_for, comparer => $comparer, data => \@big_data });
cmp_ok(
    $bs, '==', linear_search($search_for, $key_id, \@big_data),
    'Same result on @big_data'
);
ok($bs > -1, '$bs found entry');

## Search for a key that doesn't exists
my $bs2 = binary_search({search => $max_id+1, comparer => $comparer, data => \@big_data });
my $ls2 = linear_search($max_id+1, $key_id, \@big_data);

cmp_ok($bs2, '==', $ls2, 'Same result when key not exists');
cmp_ok($bs2, '==', -1,   'key not exists and is bigger than data');
cmp_ok(
    binary_search({search => -100, comparer => $comparer, data => \@big_data}),
    '==',
    linear_search(-100, $key_id, \@big_data),
    'key does not exists and is small',
);

# random tests
for my $i ( 1 .. 100 ) {
    my $id    = int rand $max_id;
    my $b_idx = binary_search({search => $id, comparer => $comparer, data => \@big_data });
    my $l_idx = linear_search($id, $key_id, \@big_data);

    cmp_ok($b_idx, '==', $l_idx, "random check $i");
}

done_testing;

# Benchmarking
if ( $opt->benchmark ) {
    cmpthese(-1, {
        'Binary Search' => sub {
            my $id = int rand $max_id;
            binary_search({
                search   => $id,
                comparer => $comparer,
                data     => \@big_data,
            });
        },
        'Linear Search' => sub {
            my $id = int rand $max_id;
            linear_search($id, $key_id, \@big_data);
        }
    });
}

# Linear Search
sub linear_search($search, $key_by, $data) {
    for my $idx ( 0 .. (scalar @$data) - 1 ) {
        local $_ = $data->[$idx];
        my $key  = $key_by->();
        if ( $search == $key ) {
            return $idx;
        }
    }
    return -1;
}


# my @data = ( ... );
# binary_search({
#     data     => \@data,
#     search   => 10,
#     comparer => sub { $a <=> $b },  # default
#     start    => 0,                  # default
#     stop     => scalar @data - 1,   # default
# });
# comparer ->
#   By default comparer is a function to compare numbers.
#   Inside comparer $a is set to $search and $b is an entry
#   from the $data array.

sub binary_search {
    my ( $args ) = @_;
    my $data     = $args->{data}     // die "data not given";
    my $search   = $args->{search}   // die "search not specified";
    my $comparer = $args->{comparer} // sub { $a <=> $b };
    my $start    = $args->{start}    // 0;
    my $stop     = $args->{stop}     // @{$args->{data}} - 1;

    # Avoid lexical scoped variable inside loop - makes code faster
    my ($index, $result, $diff) = (0,0,0);

    # We set $a and $b because our comparer function use this values
    local $a = $search;
    local $b = 0;

    while ( $start <= $stop ) {
        # compute index to check
        $diff  = ($stop - $start) / 2;
        $index = int ($start + $diff);

        # call comparer
        $b      = $data->[$index];
        $result = $comparer->();

        # when comparer returns -1, it says that $a is smaller than $b.
        # $a is what we search for and $b is the current indexed entry from array.
        # So whatever we search must between $start and $index and we need
        # to modify $stop.
        if ( $result < 0 ) {
            $stop  = $index - 1;
        }
        # the opposite. what we search for is between $index and $stop.
        # we modify $start to $index.
        elsif ( $result > 0 ) {
            $start = $index + 1;
        }
        # found entry
        else {
            return $index;
        }
    }

    # when entry does not exists
    return -1;
}

# Sample Data
__DATA__
id,name
1,Alice
5,Bob
3,Rob
20,Ron
10,Anakin
100,Luke
35,Luca
957,Sabine
324,Mena
