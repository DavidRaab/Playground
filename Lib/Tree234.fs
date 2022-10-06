namespace Tree234

type Tree<'a> =
    | Empty
    | Node2 of a:Tree<'a> * x:'a * b:Tree<'a>
    | Node3 of a:Tree<'a> * x:'a * b:Tree<'a> * y:'a  * c:Tree<'a>
    | Node4 of a:Tree<'a> * x:'a * b:Tree<'a> * y:'a  * c:Tree<'a> * z:'a * d:Tree<'a>

module Tree =
    let empty = Empty

    let inline (|IsNode4|_|) input =
        match input with
        | Node4(_,_,_,_,_,_,_) as n -> Some n
        | _                         -> None

    let inline node2 a x b         = Node2(a,x,b)
    let inline node3 a x b y c     = Node3(a,x,b,y,c)
    let inline node4 a x b y c z d = Node4(a,x,b,y,c,z,d)
    let inline singleton x         = Node2(Empty,x,Empty)
    let inline n3 x y              = Node3(Empty,x,Empty,y,Empty)
    let inline n4 x y z            = Node4(Empty,x,Empty,y,Empty,z,Empty)

    let rec depth tree =
        match tree with
        | Empty -> 0
        | Node2(l,_,r)           -> 1 + max (depth l) (depth r)
        | Node3(l,_,m,_,r)       -> 1 + max (depth l) (max (depth m) (depth r))
        | Node4(l,_,ml,_,mr,_,r) -> 1 + List.max [depth l; depth ml; depth mr; depth r]

    let rec add value tree =
        match tree with
        | Empty ->
            singleton value
        | Node2(Empty,x,Empty) ->
            if value <= x then n3 value x else n3 x value
        | Node2(a,x,b) ->
            if value <= x then
                match a with
                | Node4(Empty,y1,Empty,y2,Empty,y3,Empty) ->
                    if   value < y1 then (node3 (n3 value y1)  y2 (singleton y3) x b)
                    elif value < y2 then (node3 (n3 y1 value)  y2 (singleton y3) x b)
                    elif value < y3 then (node3 (singleton y1) y2 (n3 value y3)  x b)
                    else                 (node3 (singleton y1) y2 (n3 y3 value)  x b)
                | Node4(c1,y1,c2,y2,c3,y3,c4) ->
                    if   value < y1 then (node3 (node2 (add value c1) y1 c2) y2 (node2 c3 y3 c4) x b)
                    elif value < y2 then (node3 (node2 c1 y1 (add value c2)) y2 (node2 c3 y3 c4) x b)
                    elif value < y3 then (node3 (node2 c1 y1 c2)             y2 (node2 (add value c3) y3 c4) x b)
                    else                 (node3 (node2 c1 y1 c2)             y2 (node2 c3 y3 (add value c4)) x b)
                | a -> node2 (add value a) x b
            else
                match b with
                | Node4(Empty,y1,Empty,y2,Empty,y3,Empty) ->
                    if   value < y1 then (node3 a x (n3 value y1)  y2 (singleton y3))
                    elif value < y2 then (node3 a x (n3 y1 value)  y2 (singleton y3))
                    elif value < y3 then (node3 a x (singleton y1) y2 (n3 value y3))
                    else                 (node3 a x (singleton y1) y2 (n3 y3 value))
                | Node4(c1,y1,c2,y2,c3,y3,c4) ->
                    if   value < y1 then (node3 a x (node2 (add value c1) y1 c2) y2 (node2 c3 y3 c4))
                    elif value < y2 then (node3 a x (node2 c1 y1 (add value c2)) y2 (node2 c3 y3 c4))
                    elif value < y3 then (node3 a x (node2 c1 y1 c2)             y2 (node2 (add value c3) y3 c4))
                    else                 (node3 a x (node2 c1 y1 c2)             y2 (node2 c3 y3 (add value c4)))
                | b -> node2 a x (add value b)
        | Node3(Empty,x,Empty,y,Empty) ->
            if   value <= x then n4 value x y
            elif value <= y then n4 x value y
                            else n4 x y value
        | Node3(a,x,b,y,c) ->
            if value <= x then
                match a with
                | Node4(Empty,y1,Empty,y2,Empty,y3,Empty) ->
                    if   value <= y1 then (node4 (n3 value y1)  y2 (singleton y3) x b y c)
                    elif value <= y2 then (node4 (n3 y1 value)  y2 (singleton y3) x b y c)
                    elif value <= y3 then (node4 (singleton y1) y2 (n3 value y3)  x b y c)
                    else                  (node4 (singleton y1) y2 (n3 y3 value)  x b y c)
                | Node4(c1,y1,c2,y2,c3,y3,c4) ->
                    if   value <= y1 then (node4 (node2 (add value c1) y1 c2) y2 (node2 c3 y3 c4) x b y c)
                    elif value <= y2 then (node4 (node2 c1 y1 (add value c2)) y2 (node2 c3 y3 c4) x b y c)
                    elif value <= y3 then (node4 (node2 c1 y1 c2)             y2 (node2 (add value c3) y3 c4) x b y c)
                    else                  (node4 (node2 c1 y1 c2)             y2 (node2 c3 y3 (add value c4)) x b y c)
                | a -> node3 (add value a) x b y c
            elif value <= y then
                match b with
                | Node4(Empty,y1,Empty,y2,Empty,y3,Empty) ->
                    if   value <= y1 then (node4 a x (n3 value y1)  y2 (singleton y3) y c)
                    elif value <= y2 then (node4 a x (n3 y1 value)  y2 (singleton y3) y c)
                    elif value <= y3 then (node4 a x (singleton y1) y2 (n3 value y3)  y c)
                    else                  (node4 a x (singleton y1) y2 (n3 y3 value)  y c)
                | Node4(c1,y1,c2,y2,c3,y3,c4) ->
                    if   value <= y1 then (node4 a x (node2 (add value c1) y1 c2) y2 (node2 c3 y3 c4) y c)
                    elif value <= y2 then (node4 a x (node2 c1 y1 (add value c2)) y2 (node2 c3 y3 c4) y c)
                    elif value <= y3 then (node4 a x (node2 c1 y1 c2)             y2 (node2 (add value c3) y3 c4) y c)
                    else                  (node4 a x (node2 c1 y1 c2)             y2 (node2 c3 y3 (add value c4)) y c)
                | b -> node3 a x (add value b) y c
            else
                match c with
                | Node4(Empty,y1,Empty,y2,Empty,y3,Empty) ->
                    if   value <= y1 then (node4 a x b y (n3 value y1)  y2 (singleton y3))
                    elif value <= y2 then (node4 a x b y (n3 y1 value)  y2 (singleton y3))
                    elif value <= y3 then (node4 a x b y (singleton y1) y2 (n3 value y3))
                    else                  (node4 a x b y (singleton y1) y2 (n3 y3 value))
                | Node4(c1,y1,c2,y2,c3,y3,c4) ->
                    if   value <= y1 then (node4 a x b y (node2 (add value c1) y1 c2) y2 (node2 c3 y3 c4))
                    elif value <= y2 then (node4 a x b y (node2 c1 y1 (add value c2)) y2 (node2 c3 y3 c4))
                    elif value <= y3 then (node4 a x b y (node2 c1 y1 c2)             y2 (node2 (add value c3) y3 c4))
                    else                  (node4 a x b y (node2 c1 y1 c2)             y2 (node2 c3 y3 (add value c4)))
                | c -> node3 a x b y (add value c)
        | Node4(Empty,x,Empty,y,Empty,z,Empty) ->
            if   value <= x then (node2 (n3 value x)  y (singleton z))
            elif value <= y then (node2 (n3 x value)  y (singleton z))
            elif value <= z then (node2 (singleton x) y (n3 value z))
            else                 (node2 (singleton x) y (n3 z value))
        | Node4(a,x,b,y,c,z,d) ->
            if   value <= x then (node2 (node2 (add value a) x b) y (node2 c z d))
            elif value <= y then (node2 (node2 a x (add value b)) y (node2 c z d))
            elif value <= z then (node2 (node2 a x b) y (node2 (add value c) z d))
            else                 (node2 (node2 a x b) y (node2 c z (add value d)))

    let add' value tree =
        match tree with
        | Empty ->
            singleton value
        | Node2(Empty,x,Empty) ->
            if value <= x then n3 value x else n3 x value
        | Node2(IsNode4 a,x,b) when value <= x ->
            match add value a with
            | Node2(c1,y,c2) -> node3 c1 y c2 x b
            | a              -> node2 a x b
        | Node2(a,x,b) when value <= x ->
            node2 (add value a) x b
        | Node2(a,x,IsNode4 b) when value > x ->
            match add value b with
            | Node2(c1,y,c2) -> node3 a x c1 y c2
            | b              -> node2 a x b
        | Node2(a,x,b) ->
            node2 a x (add value b)
        | Node3(Empty,x,Empty,y,Empty) ->
            if   value <= x then n4 value x y
            elif value <= y then n4 x value y
                            else n4 x y value
        | Node3(IsNode4 a,x,b,y,c) when value <= x ->
            match add value a with
            | Node2(c1,v,c2) -> node4 c1 v c2 x b y c
            | a              -> node3 a x b y c
        | Node3(a,x,IsNode4 b,y,c) when value > x && value <= y ->
            match add value b with
            | Node2(c1,v,c2) -> node4 a x c1 v c2 y c
            | b              -> node3 a x b y c
        | Node3(a,x,b,y,IsNode4 c) when value > y ->
            match add value c with
            | Node2(c1,v,c2) -> node4 a x b y c1 v c2
            | c              -> node3 a x b y c
        | Node3(a,x,b,y,c) ->
            if   value <= x then node3 (add value a) x b y c
            elif value <= y then node3 a x (add value b) y c
            else                 node3 a x b y (add value c)
        | Node4(Empty,x,Empty,y,Empty,z,Empty) ->
            if   value <= x then (node2 (n3 value x)  y (singleton z))
            elif value <= y then (node2 (n3 x value)  y (singleton z))
            elif value <= z then (node2 (singleton x) y (n3 value z))
            else                 (node2 (singleton x) y (n3 z value))
        | Node4(a,x,b,y,c,z,d) ->
            if   value <= x then (node2 (node2 (add value a) x b) y (node2 c z d))
            elif value <= y then (node2 (node2 a x (add value b)) y (node2 c z d))
            elif value <= z then (node2 (node2 a x b) y (node2 (add value c) z d))
            else                 (node2 (node2 a x b) y (node2 c z (add value d)))

    let rec show tree =
        match tree with
        | Empty                                -> "Empty"
        | Node2(Empty,x,Empty)                 -> sprintf "(singleton %A)" x
        | Node2(a,x,b)                         -> sprintf "(node2 %s %A %s)" (show a) x (show b)
        | Node3(Empty,x,Empty,y,Empty)         -> sprintf "(n3 %A %A)" x y
        | Node3(a,x,b,y,c)                     -> sprintf "(node3 %s %A %s %A %s" (show a) x (show b) y (show c)
        | Node4(Empty,x,Empty,y,Empty,z,Empty) -> sprintf "(n4 %A %A %A)" x y z
        | Node4(a,x,b,y,c,z,d)                 -> sprintf "(node4 %s %A %s %A %s %A %s)" (show a) x (show b) y (show c) z (show d)

    let rec toList tree =
        match tree with
        | Empty                  -> []
        | Node2(l,x,r)           -> List.concat [toList l; [x]; toList r]
        | Node3(l,x,m,y,r)       -> List.concat [toList l; [x]; toList m;  [y]; toList r]
        | Node4(l,x,c1,y,c2,z,r) -> List.concat [toList l; [x]; toList c1; [y]; toList c2; [z]; toList r]
