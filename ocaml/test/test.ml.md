# Manual Compiling & Running

## Compiling

```
ocamlc -o test.byte test.ml
```

## Running

```
./test.byte
```

-OR-

```
ocamlrun test.byte
```

# Compiling using Dune Build System

```
dune build test.exe
```

run

```
./_build/default/_test.exe
```

# Compiling & Running with Dune

```
dune exec ./test.exe
```
