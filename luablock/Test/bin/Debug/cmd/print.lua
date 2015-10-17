ret = "/tellraw " .. args[0] .. " [ "
--/tellraw @a [ {text:"Your score is "} , {score:{name:lua,objective:lua_if_1}} , {text:"."} ]
for i=1,argsl - 1,1
do 
   ret = ret .. "{text:\" " .. args[i] .. "\"},"
end

return ret .. "{score:{name:lua,objective:" .. args[argsl] .."}}]"