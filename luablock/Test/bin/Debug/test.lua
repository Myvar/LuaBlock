
function Hi()
say("[ §aInfo ]Wellcome to Lua block");
end

function Panic()
say("[ §cPanic ]an faital error has ocured");
end

function Error()
say("[ §cError ]an error has ocured");
end

setblock(1 ,1 ,1 , "minecraft:stone")
gamemode("a", "@a")
Hi()
say("test", "@a")
say("test")
Hi()
Panic()
Error()
gamemode("c", "@a")