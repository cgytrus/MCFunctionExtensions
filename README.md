# Minecraft Function Extensions
Some extensions to the vanilla functions syntax

## Features
- Self namespaces
- Inline functions
- Else statements

## TODO
- Libraries?
- loops
- function arguments?
- Writing multiple commands in a line separated with ;
- idk what else yet but you can suggest your ideas ;)

## Example
this example would be compiled to 3 files located next to test.extmcfunction: text.mcfunction, zombie_detected.mcfunction and pog.mcfunction
```
# test.extmcfunction
execute unless entity @s run say not bruh :(
else if entity @e[type=zombie] run function :zombie_detected {
    say yooo
    say duuuude
    function :pog {
        say this is Pog
    }
}
else if entity @e[type=boat] run say bruh
else run say gimme B  O  A  T  S
```
