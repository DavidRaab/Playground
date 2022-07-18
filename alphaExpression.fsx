#!/usr/bin/env -S dotnet fsi

#load "wizards.fsx"
open Wizards

// Alpha Expression to pick things
type Expr<'a> =
    | True
    | False
    | And  of Expr<'a> * Expr<'a>
    | Or   of Expr<'a> * Expr<'a>
    | Not  of Expr<'a>
    | Base of 'a

let rec eval evalBase expr =
    let eval' x = eval evalBase x
    match expr with
    | True      -> true
    | False     -> false
    | And (l,r) -> eval' l && eval' r
    | Or  (l,r) -> eval' l || eval' r
    | Not  x    -> not (eval' x)
    | Base x    -> evalBase x

// Some Data for our Expr Language
let weapons = [
    Weapons.excalibur
    Weapons.moonStaff
    Weapon.createSword Weapon.VerySlow 100 "Hammer"
    Weapon.createSword Weapon.Slow     70  "Executioner"
    Weapon.createSword Weapon.Normal   50  "Sword"
    Weapon.createSword Weapon.Fast     20  "Dagger"
    Weapon.createSword Weapon.VeryFast 10  "Zahnstocher"
    Weapon.createStaff Weapon.VerySlow 100 "Fister"
    Weapon.createStaff Weapon.Slow     70  "Licker"
    Weapon.createStaff Weapon.Normal   50  "Burster"
    Weapon.createStaff Weapon.Fast     20  "Vibrator"
    Weapon.createStaff Weapon.VeryFast 10  "Orgasmer"
]

// Build our predicates for our language
let isFast' w =
    match Weapon.attackSpeed w with
    | Weapon.VerySlow
    | Weapon.Slow
    | Weapon.Normal   -> false
    | Weapon.Fast
    | Weapon.VeryFast -> true

let isSlow' w =
    match Weapon.attackSpeed w with
    | Weapon.VerySlow
    | Weapon.Slow     -> true
    | Weapon.Normal
    | Weapon.Fast
    | Weapon.VeryFast -> false

let isNormal' w =
    Weapon.attackSpeed w = Weapon.Normal

let isSword' w = Weapon.typ w = Weapon.Sword
let isStaff' w = Weapon.typ w = Weapon.Staff

let damageHeigher' x w =
    Weapon.damage w > x


// Use our Language
let isFast   w = eval isFast'   w
let isSlow   w = eval isSlow'   w
let isNormal w = eval isNormal' w
let isSword  w = eval isSword'  w
let isStaff  w = eval isStaff'  w
let damageHeigher x w = eval (damageHeigher' x) w

let andExpr = And(Base Weapons.excalibur, Base Weapons.moonStaff)
let orExpr  = Or (Base Weapons.excalibur, Base Weapons.moonStaff)

let isFastOrSlow      w = isFast' w || isSlow' w
let isFastAndDmgOver9 w = isFast' w && damageHeigher' 9 w

eval isFastOrSlow      andExpr
eval isFastAndDmgOver9 andExpr
eval isFastOrSlow      orExpr
eval isFastAndDmgOver9 orExpr


isFast andExpr

let x    = seq { 1; 2; 3; 4; 5 }
let list = seq {
    yield Some (string 1)
    yield None
    for x in 3 .. System.Int32.MaxValue do
        if x % 2 <> 0
        then yield Some (string x)
        else yield None
}

let list' = [
    yield Some (string 1)
    yield None
    for x in 3 .. 1_000_000 do
        if x % 2 <> 0
        then yield Some (string x)
        else yield None
]