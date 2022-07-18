module Weapon =
    type WeaponType =
        | Sword
        | Staff

    type Handed =
        | OneHanded
        | TwoHanded

    type AttackSpeed =
        | VeryFast
        | Fast
        | Normal
        | Slow
        | VerySlow

    type T = {
        Name:        string
        Damage:      int
        AttackSpeed: AttackSpeed
        Type:        WeaponType
        Handed:      Handed
    }

    // Accessors
    let damage w      = w.Damage
    let name w        = w.Name
    let attackSpeed w = w.AttackSpeed
    let typ w         = w.Type
    let handed w      = w.Handed

    let attackTime w =
        match attackSpeed w with
        | VeryFast -> 0.5
        | Fast     -> 0.75
        | Normal   -> 1.0
        | Slow     -> 1.33
        | VerySlow -> 2.0

    let attacksPerSecond w = 1.0 / attackTime w
    let dps w              = float (damage w) * attacksPerSecond w

    // Creators
    let createWeapon handed typ speed dmg name  = {
        Name        = name
        Damage      = dmg
        AttackSpeed = speed
        Type        = typ
        Handed      = handed
    }

    let create1HWeapon = createWeapon OneHanded
    let create2HWeapon = createWeapon TwoHanded
    let createSword    = create1HWeapon Sword
    let createStaff    = create2HWeapon Staff


module Weapons =
    let excalibur = Weapon.createSword Weapon.VeryFast 10 "Excalibur"
    let moonStaff = Weapon.createStaff Weapon.VerySlow 20 "MoonStaff"


module Character =
    type Enemy =
        | Werwolf
        | Vampire

    type PlayerClass =
        | Wizzard
        | Warrior
        | Paladin

    type Player = {
        Class:  PlayerClass
        Weapon: Weapon.T option
    }

    // Accessors
    let pclass p    = p.Class
    let weapon p    = p.Weapon
    let hasWeapon p = Option.isSome p.Weapon

    // Mutators
    let withWeapon weapon player =
        { player with Weapon=weapon }

    // Creation of Players
    let createPlayer cls weapon = {
         Class  = cls
         Weapon = weapon
    }

    // Data: Default Characters
    let warrior = createPlayer Warrior None
    let wizzard = createPlayer Wizzard None
    let paladin = createPlayer Paladin None

    // Create Chars with weapon
    let createWarrior = createPlayer Warrior
    let createWizzard = createPlayer Wizzard
    let createPaladin = createPlayer Paladin

    let damage player =
        defaultArg
            (weapon player |> Option.map Weapon.damage)
            0

module Rule =
    let weaponAllowed cls weapon =
        match cls,weapon with
        | Character.Warrior, Weapon.Sword -> true
        | Character.Warrior, Weapon.Staff -> false
        | Character.Wizzard, Weapon.Sword -> false
        | Character.Wizzard, Weapon.Staff -> true
        | Character.Paladin, Weapon.Sword -> true
        | Character.Paladin, Weapon.Staff -> true
    
    let tryWeapon (player:Character.Player) (weapon:Weapon.T) =
        match weaponAllowed player.Class weapon.Type with
        | true  -> Some weapon
        | false -> None


module Game =
    let equipPlayer (weapon:Weapon.T) (player:Character.Player) =
        player |> Character.withWeapon (Rule.tryWeapon player weapon)

    let damageAgainst enemy (player:Character.Player) =
        match player.Class,enemy with
        | Character.Paladin, Character.Vampire -> Character.damage player / 2
        | Character.Warrior, Character.Werwolf -> Character.damage player * 2
        | Character.Wizzard, Character.Vampire -> Character.damage player * 2
        | _ -> Character.damage player

    let doDamage enemy player =
        let hero   = string (Character.pclass player)
        let senemy = string enemy

        match Character.hasWeapon player with
        | false -> printfn "Ohhh nooooo!!!1! our Hero %s has no Weapon!" hero
        | true  -> printfn "%s does %d damage against %s" hero (damageAgainst enemy player) senemy



Game.doDamage Character.Vampire (Character.warrior |> Game.equipPlayer Weapons.excalibur)
Game.doDamage Character.Vampire (Character.wizzard |> Game.equipPlayer Weapons.excalibur)
Game.doDamage Character.Vampire (Character.wizzard |> Game.equipPlayer Weapons.moonStaff)
Game.doDamage Character.Vampire (Character.paladin |> Game.equipPlayer Weapons.excalibur)

Weapon.dps Weapons.excalibur
Weapon.dps Weapons.moonStaff
