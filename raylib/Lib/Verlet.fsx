open System.Numerics

[<NoComparison; NoEquality>]
type VerletPoint = {
    mutable OldPosition:  Vector2
    mutable Position:     Vector2
    mutable Acceleration: Vector2
    Radius: float32
}

[<NoComparison; NoEquality>]
type Stick = {
    Start:  VerletPoint
    End:    VerletPoint
    Length: float32
}

[<NoComparison; NoEquality>]
type BreakableStick = {
    Stick:  Stick
    Factor: float32
}

[<NoComparison; NoEquality>]
type VerletStructure = {
    Points:        array<VerletPoint>
    Sticks:        array<Stick>
    CollisionMesh: array<Vector2>
}

[<NoComparison; NoEquality>]
type Pinned = {
    Point:          VerletPoint
    PinnedPosition: Vector2
}

module Verlet =
    let inline vec2 x y = Vector2(x,y)

    let point radius position = {
        OldPosition  = position
        Position     = position
        Acceleration = Vector2.Zero
        Radius       = radius
    }

    let stick first second = {
        Start  = first
        End = second
        Length = Vector2.Distance(first.Position, second.Position)
    }

    let breakableStick first second factor = {
        Stick  = stick first second
        Factor = factor
    }

    let inline velocity point =
        point.Position - point.OldPosition

    let newLength length stick = {
        stick with Length = length
    }

    let updatePoint point (dt:float32) =
        // Verlet means that velocity is calculated from the previous position
        // of previous frame. Than velocity and acceleration is used to calculate
        // the new Position. We can do that in two steps or just in one single
        // calculation. Because velocity is calculated from previous frame it
        // is already frame-dependent and we don't need to multiply it with
        // deltaTime.
        //
        // let velocity    = point.Position - point.OldPosition
        // let newPosition = point.Position + velocity + (point.Acceleration * dt * dt)
        let newPosition     = 2f * point.Position - point.OldPosition + (point.Acceleration * dt * dt)
        point.OldPosition  <- point.Position
        point.Position     <- newPosition
        point.Acceleration <- Vector2.Zero

    let updateStick stick =
        let axis       = stick.Start.Position - stick.End.Position
        let distance   = axis.Length ()
        let n          = axis / distance
        let correction = stick.Length - distance

        stick.Start.Position <- stick.Start.Position + (n * correction * 0.5f)
        stick.End.Position   <- stick.End.Position   - (n * correction * 0.5f)

    let pointInsideStructure (point:Vector2) vstruct =
        let arrayItem idx (array:'a array) =
            if idx < 0
            then array.[array.Length + idx]
            else array.[idx]

        let poly  = vstruct.CollisionMesh
        let (x,y) = point.X, point.Y

        let mutable inside = false
        for i=0 to poly.Length-1 do
            let start = arrayItem (i-1) poly
            let stop  = arrayItem  i    poly

            let insideHeight = (y < start.Y && y > stop.Y) || (y < stop.Y && y > start.Y)
            if insideHeight then
                let diffX       = start.X - stop.X
                let diffY       = start.Y - stop.Y
                let n           = diffX / diffY     // x movement per 1 y unit
                let h           = y - start.Y       // height of y relative to start
                let x_collision = start.X + (h * n) // x point for y value on line start to stop
                if x < x_collision then
                    inside <- not inside
        inside

    let shouldBreak bstick =
        let axis = bstick.Stick.Start.Position - bstick.Stick.End.Position
        axis.Length() > (bstick.Stick.Length * bstick.Factor)

    let inline addForce force point =
        point.Acceleration <- point.Acceleration + force

    let placeAt (pos:Vector2) vstruct =
        for point in vstruct.Points do
            point.OldPosition <- point.OldPosition + pos
            point.Position    <- point.Position    + pos

    let pinFirst structure =
        match structure with
        | { Points = [||]   } -> None
        | { Points = points } -> Some { Point = points.[0]; PinnedPosition = points.[0].Position }

    let triangle (x:Vector2) y z =
        let a,b,c = point 10f x, point 10f y, point 10f z
        {
            Points        = [|a; b; c|]
            Sticks        = [|stick a b; stick b c; stick c a|]
            CollisionMesh = [||]
        }

    let quad x y z w =
        let a = point 10f x
        let b = point 10f y
        let c = point 10f z
        let d = point 10f w
        {
            Points        = [|a;b;c;d|]
            Sticks        = [|stick a b; stick b c; stick c d; stick d a; stick a c; stick b d|]
            CollisionMesh = [||]
        }

    let rectangle x y w h =
        let tl = vec2 x y
        let tr = tl + (vec2 w 0f)
        let bl = tl + (vec2 0f h)
        let br = tl + (vec2 w h)
        quad tl tr bl br

    let ropePoints points =
        let sticks (a:VerletPoint array) =
            let ra = ResizeArray()
            if a.Length <= 1 then
                ra.ToArray()
            else
                for i=0 to a.Length-2 do
                    ra.Add(stick a.[i] a.[i+1] )
                ra.ToArray()

        let points = Array.ofSeq points
        {
            Points        = points
            Sticks        = sticks points
            CollisionMesh = [||]
        }

    let rope radius steps (start:Vector2) stop =
        let point  = point radius
        let moveby = Vector2.Divide(stop - start, (float32 steps + 1f))
        ropePoints [
            yield  point start
            yield! List.init steps (fun i -> point (start + (moveby * (1f + float32 i))))
            yield  point stop
        ]