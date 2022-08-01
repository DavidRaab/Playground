// https://www.youtube.com/watch?v=t1e8gqXLbsU
// What is a Monad?
// Code example in JavaScript instead of Haskell

// Representing of Maybe in JS
let Just = function(x) {
    this.match    = m  => m.Just(x);
    this.toString = () => `Just(${x})`;
}

let Nothing = function(x) {
    this.match    = m  => m.Nothing();
    this.toString = () => `Nothing`;
}

// This is how you declare and use it
let x = new Just(5);
let y = new Nothing();

let printJust = function(m) {
    return m.match({
        Nothing: () => console.info("Nothing"),
        Just:    x  => console.info(`Just ${x}`)
    });
}

printJust(x);
printJust(y);

// Now the same for Expr
let Val = function(x) {
    this.match = m => m.Val(x);
}

let Div = function(expr1, expr2) {
    this.match = m => m.Div(expr1,expr2);
}

// omit "new" bullshit
let val = x     => new Val(x);
let div = (x,y) => new Div(x,y);

// Some sample expressions
let expr1 = val(1)
let expr2 = div(val(6), val(2))
let expr3 = div(val(6), div(val(3), val(1)))
let expr4 = div(val(6), val(0))

let eval1 = function(expr) {
    return expr.match({
        Val: x     => x,
        Div: (x,y) => eval1(x) / eval1(y)  // JS returns infinity, instead of exception
    });                                    // Still unwanted, we want Nothing()
}

console.info("eval1.1 = " + eval1(expr1) );
console.info("eval1.2 = " + eval1(expr2) );
console.info("eval1.3 = " + eval1(expr3) );
console.info("eval1.4 = " + eval1(expr4) );


// safeDiv with Maybe
let safeDiv = function(x,y) {
    if ( y === 0 ) {
        return new Nothing();
    }
    else {
        return new Just(x / y);
    }
}

console.info("safeDiv(6,0) = " + safeDiv(6,0).toString() );

// 2. eval2 that uses safeDiv
let eval2 = function(expr) {
    return expr.match({
        Val: x     => new Just(x),
        Div: (x,y) => 
            eval2(x).match({
                Nothing: () => new Nothing(),
                Just:    x  => 
                    eval2(y).match({
                        Nothing: () => new Nothing(),
                        Just:    y  => safeDiv(x,y)
                    })
            })
    });
}

console.info("eval2.1 = " + eval2(expr1) );
console.info("eval2.2 = " + eval2(expr2) );
console.info("eval2.3 = " + eval2(expr3) );
console.info("eval2.4 = " + eval2(expr4) );


// We now implement a bind function
let bind = function(m,f) {
    return m.match({
        Nothing: () => new Nothing(),
        Just:    x  => f(x)
    });
}

// eval3 with bind function
let eval3 = function(expr) {
    return expr.match({
        Val: x     => new Just(x),
        Div: (x,y) =>
            bind(eval3(x), x =>
            bind(eval3(y), y =>
                safeDiv(x,y)
            ))
    });
}

console.info("eval3.1 = " + eval3(expr1) );
console.info("eval3.2 = " + eval3(expr2) );
console.info("eval3.3 = " + eval3(expr3) );
console.info("eval3.4 = " + eval3(expr4) );


// Add bind function as method "then"
// "then" because JS already has a "bind" method
Just.prototype.then    = function(f) { return bind(this,f) }
Nothing.prototype.then = function(f) { return bind(this,f) }

// eval4 with method syntax
let eval4 = function(expr) {
    return expr.match({
        Val: x     => new Just(x),
        Div: (x,y) =>
            eval4(x).then(x =>
            eval4(y).then(y =>
                safeDiv(x,y)
            ))
    });
}

console.info("eval4.1 = " + eval4(expr1) );
console.info("eval4.2 = " + eval4(expr2) );
console.info("eval4.3 = " + eval4(expr3) );
console.info("eval4.4 = " + eval4(expr4) );


// Extra - Expr printer
let printer = function(expr) {
    return expr.match({
        Val: x     => `Val(${x})`,
        Div: (x,y) => `Div(${ printer(x) }, ${ printer (y) })`
    });
}

console.info("expr1 = " + printer(expr1) )
console.info("expr2 = " + printer(expr2) )
console.info("expr3 = " + printer(expr3) )
console.info("expr4 = " + printer(expr4) )
