package Schema::Result::Album;
use base qw/DBIx::Class::Core/;

__PACKAGE__->table('albums');
__PACKAGE__->add_columns(qw/id artist_id name/);
__PACKAGE__->set_primary_key('id');
__PACKAGE__->has_many(songs => 'Schema::Result::Song', 'album_id');
__PACKAGE__->belongs_to(artist => 'Schema::Result::Artist', 'artist_id');

1;