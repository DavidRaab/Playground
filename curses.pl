#!/usr/bin/env perl
use v5.32;
use warnings;
use feature 'signatures';
no warnings 'experimental::signatures';
use open ':std', ':encoding(UTF-8)';
use Data::Printer;
use Curses;

my ( $r, $c, $nrows, $ncols ) = ( 0, 0, 0, 0 );

my $win = initscr();
cbreak();
noecho();
getmaxyx( $win, $nrows, $ncols );
clear();
refresh();

while (1) {
    my $d = getch();    # curses call to input from keyboard
    last if $d eq 'q';
    draw($d);           # draw the character
}

endwin();

sub draw($dc) {
    move( $r, $c );     # curses call to move cursor to row r, column c
    delch();
    insch($dc);         # curses calls to replace character under cursor by dc
    refresh();          # curses call to update screen
    $r++;               # go to next row
                        # check for need to shift right or wrap around
    if ( $r == $nrows ) {
        $r = 0;
        $c++;
        if ( $c == $ncols ) {
            $c = 0;
        }
    }
}
