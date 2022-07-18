# Info

This project shows how you can use a SQL database in Perl. In this
case SQLite, but through the Database layer DBI, different databases
looks the same.

## select.pure

Just pure DBI Access. The most basic database access in Perl
with pure SQL Statements.

## select.abstract

SQL::Abstract is a tool to create SQL Statements for you, without
creating SQL strings on yourself.

## select.dbix

Uses ORM mapper DBIx::Class. For this every table get a file on
its own. The "Schema.pm" and "Schema/Result/*.pm" files are part
of this.
