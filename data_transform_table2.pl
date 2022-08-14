#!/usr/bin/env perl
use strict;
use warnings;
use v5.32;
use Data::Printer;

my %hash = (
  '2022-08-04' => {
      'Method 1' => {
            'Count' => 50,
            'Size' => '10 MB'
      },
      'Method 2' => {
            'Count' => 40,
            'Size' => '5 MB'
      }
  },
  '2022-08-05' => {
      'Method 1' => {
            'Count' => 30,
            'Size' => '3 MB'
      },
      'Method 2' => {
            'Size' => '50 MB',
            'Count' => '100'
      }
  }
);

# First get all the keys of the hashes
my @dates   = keys %hash;
my %methods = map { map { $_ => 1 } keys %$_ } values %hash;
my @methods = keys %methods;
my %args    = map { map { $_ => 1 } keys %$_ } map { values %$_ } values %hash;
my @args    = keys %args;

p @dates;
#[
#    [0] "2022-08-05" (dualvar: 2022),
#    [1] "2022-08-04" (dualvar: 2022)
#]
p @methods;
#[
#    [0] "Method 2",
#    [1] "Method 1"
#]
p @args;
#[
#    [0] "Count",
#    [1] "Size"
#]


# Then create a new data-structure from it
my @data;
for my $date ( @dates ) {
    my %data;
    $data{Date} = $date;
    for my $method ( @methods ) {
        for my $arg ( @args ) {
            $data{$method . ' ' . $arg} = $hash{$date}{$method}{$arg};
        }
    }
    push @data, \%data;
}

p @data;
# [
#    [0] {
#            Date               "2022-08-05" (dualvar: 2022),
#            "Method 1 Count"   30,
#            "Method 1 Size"    "3 MB" (dualvar: 3),
#            "Method 2 Count"   100,
#            "Method 2 Size"    "50 MB" (dualvar: 50)
#        },
#    [1] {
#            Date               "2022-08-04" (dualvar: 2022),
#            "Method 1 Count"   50,
#            "Method 1 Size"    "10 MB" (dualvar: 10),
#            "Method 2 Count"   40,
#            "Method 2 Size"    "5 MB" (dualvar: 5)
#        }
# ]

## Now, you can create your table from it

my @fields     = ("Date", "Method 1 Count", "Method 1 Size", "Method 2 Count", "Method 2 Size");
my @table_data = map { [ @$_{@fields} ] } @data;

# Get the maximum length of every field
my @lengths = (0) x @fields;
for my $row ( \@fields, @table_data ) {
    for (my $idx=0; $idx < @fields; $idx++) {
        $lengths[$idx] = max($lengths[$idx], length $row->[$idx]);
    }
}

# Print the Table
print_with_length(\@fields, \@lengths, " | ");
for my $row ( @table_data ) {
    print_with_length($row, \@lengths, " | ");
}


sub max {
    my ($x, $y) = @_;
    return $x > $y ? $x : $y;
}

sub print_with_length {
    my ($array, $lengths, $seperator) = @_;
    my $sep = $seperator // " ";
    for (my $idx = 0; $idx < @$array; $idx++) {
        my $l = $lengths->[$idx] // 5;
        printf "%${l}s$sep", $array->[$idx];
    }
    print "\n";
}
