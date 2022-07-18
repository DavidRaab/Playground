package Schema::Result::Song;
use base qw/DBIx::Class::Core/;

__PACKAGE__->table('songs');
__PACKAGE__->add_columns(qw/id album_id track name duration/);
__PACKAGE__->set_primary_key('id');
__PACKAGE__->belongs_to(album => 'Schema::Result::Album', 'album_id');

1;
