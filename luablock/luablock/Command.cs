using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luablock
{
    public class Command
    {
        public string Name { get; set; }

        public virtual string Parse(string Name, List<string> args)
        {
            return "/say [ §4Error ] Identifier " + Name + " Does not exzist";
        }
    }
}
