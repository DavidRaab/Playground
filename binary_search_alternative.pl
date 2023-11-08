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

# needed for linear search, not needed by lookup
@data = sort { $a->{id} <=> $b->{id} } @data;

# generate lookup table
my $data_lookup = generate_lookup(sub { $_->{id} }, \@data);

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

# generate lookup table
my $big_data_lookup = generate_lookup(sub { $_->{id} }, \@big_data);

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

    my @data   = (5 .. 10);
    my $lookup = generate_lookup(sub {$_}, \@data);

    for my $check ( @checks ) {
        my $search   = $check->[0];
        my $expected = $check->[1];

        my $idx = get_index($lookup, $search);

        cmp_ok($idx, '==', $expected, 'Static Tests');
    }
}

my $by_id = sub { $_->{id} };

# Test on @data
my $found_binary = get_index($data_lookup, 10);

## search for 10
my $found_linear = linear_search(10, $by_id, \@data);
cmp_ok($found_binary, '==', $found_linear, 'Same result on @data');
cmp_ok($found_binary, '==', 3,             'Found at index 3');

## search for a key that doesn't exists
cmp_ok(
    linear_search(4, $by_id, \@data),
    '==',
    get_index($data_lookup, 4),
    'search id == 4'
);

# Test on @big_data

## Pick an existing key - we now that there are 100_000 elements in it
## i pick some in the middle
my $search_for = $big_data[50_000]{id};
my $bs = get_index($big_data_lookup, $search_for);
cmp_ok(
    $bs, '==', linear_search($search_for, $by_id, \@big_data),
    'Same result on @big_data'
);
ok($bs > -1, '$bs found entry');

## Search for a key that doesn't exists
my $bs2 = get_index($big_data_lookup, $max_id+1);
my $ls2 = linear_search($max_id+1, $by_id, \@big_data);

cmp_ok($bs2, '==', $ls2, 'Same result when key not exists');
cmp_ok($bs2, '==', -1,   'key not exists and is bigger than data');
cmp_ok(
    get_index($big_data_lookup, -100),
    '==',
    linear_search(-100, $by_id, \@big_data),
    'key does not exists and is small',
);


for my $i ( 1 .. 100 ) {
    my $id    = int(rand($max_id));
    my $b_idx = get_index($big_data_lookup, $id);
    my $l_idx = linear_search($id, $by_id, \@big_data);

    cmp_ok($b_idx, '==', $l_idx, "random check $i");
}

done_testing;

# Benchmarking
if ( $opt->benchmark ) {
    cmpthese(-1, {
        'Lookup Table' => sub {
            my $id = int rand($max_id);
            get_index($big_data_lookup, $id);
        },
        'Linear Search' => sub {
            my $id = int rand($max_id);
            linear_search($id, $by_id, \@big_data);
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

sub generate_lookup($get_key, $data) {
    my %lookup_table;

    my $idx = 0;
    for my $entry ( @$data ) {
        local $_ = $entry;
        my $key  = $get_key->();
        $lookup_table{$key} = $idx++;
    }

    return \%lookup_table;
}

sub get_index($lookup, $key) {
    if ( exists $lookup->{$key} ) {
        return $lookup->{$key};
    }
    else {
        return -1;
    }
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
