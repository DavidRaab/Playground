package Schema::Result::Artist;
use base qw/DBIx::Class::Core/;

__PACKAGE__->table('artists');
__PACKAGE__->add_columns(qw/id name/);
__PACKAGE__->set_primary_key('id');
__PACKAGE__->has_many(albums => 'Schema::Result::Album', 'artist_id');

1;
