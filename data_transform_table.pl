#!/usr/bin/env perl
use strict;
use warnings;
use v5.32;
use Data::Printer;
use Text::Table;
use List::Util qw(uniqstr);

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

# Transform %hash by flattening the inner hashes
my %data;
for my $date ( keys %hash ) {
    $data{$date} = hashref_flatten(" ", $hash{$date});
}

p %data;
# {
#    2022-08-04   {
#        "Method 1 Count"   50,
#        "Method 1 Size"    "10 MB" (dualvar: 10),
#        "Method 2 Count"   40,
#        "Method 2 Size"    "5 MB" (dualvar: 5)
#    },
#    2022-08-05   {
#        "Method 1 Count"   30,
#        "Method 1 Size"    "3 MB" (dualvar: 3),
#        "Method 2 Count"   100,
#        "Method 2 Size"    "50 MB" (dualvar: 50)
#    }
# }

# Get all date and fields sorted
my @fields     = sort {$a cmp $b} uniqstr map { keys %$_ } values %data;
my @dates      = sort keys %data;
my @table_data = map {[ $_, hashref_fields($data{$_}, @fields) ]} @dates;

## Now, you can create your table from it
## I'm using Text::Table

my $table = Text::Table->new("Date", @fields);
$table->load(@table_data);
print $table;

# Date       Method 1 Count Method 1 Size Method 2 Count Method 2 Size
# 2022-08-04 50             10 MB          40            5 MB
# 2022-08-05 30             3 MB          100            50 MB



## Helper Functions

# Turns a Hash of Hash (HoH) into a single hash by combining the keys
sub hashref_flatten {
    my ( $sep, $hash ) = @_;
    my %nk;
    while ( my ($key, $value) = each %$hash ) {
        for my $inner_key ( keys %$value ) {
            $nk{$key.$sep.$inner_key} = $hash->{$key}{$inner_key};
        }
    }
    return wantarray ? %nk : \%nk;
}

# Returns multiple fields of a hashref
# can be inlined, but is hard to read/understand
sub hashref_fields {
    my ($hash, @fields) = @_;
    return @$hash{@fields};
}
