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
    Points: VerletPoint list
    Sticks: Stick list
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

        stick.Start.Position  <- stick.Start.Position  + (n * correction * 0.5f)
        stick.End.Position <- stick.End.Position - (n * correction * 0.5f)

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
        | { Points = []          } -> None
        | { Points = first::rest } -> Some { Point = first; PinnedPosition = first.Position }

    let triangle (x:Vector2) y z =
        let a,b,c = point 10f x, point 10f y, point 10f z
        {
            Points = [a; b; c]
            Sticks = [stick a b; stick b c; stick c a]
        }

    let quad x y z w =
        let a = point 10f x
        let b = point 10f y
        let c = point 10f z
        let d = point 10f w
        {
            Points = [a;b;c;d]
            Sticks = [stick a b; stick b c; stick c d; stick d a; stick a c; stick b d]
        }

    let rectangle x y w h =
        let tl = vec2 x y
        let tr = tl + (vec2 w 0f)
        let bl = tl + (vec2 0f h)
        let br = tl + (vec2 w h)
        quad tl tr bl br

    let ropePoints points =
        let rec loop points =
            match points with
            | []      -> { Points = [];      Sticks = [] }
            | [point] -> { Points = [point]; Sticks = [] }
            | current :: rest ->
                let rope = loop rest
                let next = List.head rope.Points
                {
                    Points = current :: rope.Points
                    Sticks = (stick current next) :: rope.Sticks
                }
        loop points

    let rope radius steps (start:Vector2) stop =
        let point  = point radius
        let moveby = Vector2.Divide(stop - start, (float32 steps + 1f))
        ropePoints [
            yield  point start
            yield! List.init steps (fun i -> point (start + (moveby * (1f + float32 i))))
            yield  point stop
        ]