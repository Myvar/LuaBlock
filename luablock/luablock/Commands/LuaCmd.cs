using MoonSharp.Interpreter;
using NetLua;
using NetLua.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luablock.Commands
{
    public class LuaCmd : Command
    {
        public string Lua { get; set; }

        public override string Parse(string Name, List<string> args)
        {
            Script script = new Script();
            UserData.RegisterType<List<string>>();
    
            script.Globals.SetAsObject("args", args);
            script.Globals.SetAsObject("argsl", args.Count - 1);
            return script.DoString(Lua).String;
        }

        private string TypeToString(IExpression e)
        {
            //this feels so cheety(whatt ever you do dont show this too kenny! xD)
            dynamic z = e;
            try
            {
                try
                {
                    return z.Value;
                }
                catch
                {
                    return z.Name;
                }
            }
            catch
            {
                return "";
            }
        }
    }
}
