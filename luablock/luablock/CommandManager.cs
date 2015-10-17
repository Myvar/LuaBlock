using luablock.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luablock
{
    public static class CommandManager
    {
        public static List<Command> Cmds = new List<Command>()
        {
            new Say()
        };

        public static Command GetCommand(string name)
        {
            foreach(var i in Cmds)
            {
                if(i.Name == name)
                {
                    return i;
                }
            }

            return new Command();
        }
    }
}
