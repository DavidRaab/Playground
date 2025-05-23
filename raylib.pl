#!/usr/bin/env perl
use 5.040;
use utf8;
use open ':std', ':encoding(UTF-8)';
# use Sq;
# use Sq::Sig;
use Raylib::FFI; # defaults to exporting all the functions
use constant Color => 'Raylib::FFI::Color';

InitWindow( 800, 600, "Testing!" );
SetTargetFPS(60);
while ( !WindowShouldClose() ) {
    my $x = GetScreenWidth() / 2;
    my $y = GetScreenHeight() / 2;
    BeginDrawing();
    ClearBackground( Color->new( r => 0, g => 0, b => 0, a => 0 ) );
    DrawFPS( 0, 0 );
    DrawText( "Hello, world!",
        $x, $y, 20, Color->new( r => 255, g => 255, b => 255, a => 255 ) );
    EndDrawing();
}
CloseWindow();
