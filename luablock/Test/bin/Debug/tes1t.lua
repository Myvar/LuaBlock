
function war()
	if item == 0 then
	 give("@a","minecraft:diamond_sword", 1, 0,"{Unbreakable:1}")
	end
end

function hunger()
	if item == 1 then
		give("@a","minecraft:beef", 1, 0)
	end
end

function redstone()
	if item == 2 then
		give("@a","minecraft:redstone_block", 64, 0)
	end
end

war()
hunger()
redstone()