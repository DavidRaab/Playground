# Benchmarking of Iterate performane

Here I test how fast it is to iterate through a Dictionary compared to an Array.

I am trying to figure out the best storage for an ECS (Entity-Component-System).
In my use cases I usually need a good insert and lookup speed to fetch Components
of an Entity. That's why I pick a Dictionary.

However most of the systems need to iterate through the whole data-structure,
so iterative performance also becomes important.
