using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luablock.Commands
{
    public class Say : Command
    {
        public Say()
        {
            Name = "say";                 
        }

        public override string Parse(string Name, List<string> args)
        {
            string re = "/say";

            foreach(var i in args)
            {
                re += " " + i;
            }

            return re;
        }
    }
}
