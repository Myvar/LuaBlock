using luablock.Commands;
using NetLua;
using NetLua.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luablock
{
    public class Compiler
    {
        public Dictionary<string, List<string>> Methods = new Dictionary<string, List<string>>();

        public int ConstantCount = 0;

        public Compiler()
        {
            if(Directory.Exists("cmd"))
            {
                foreach(var i in Directory.GetFiles("cmd"))
                {
                    if (i.EndsWith(".lua"))
                    {
                        var fl = new FileInfo(i);
                        CommandManager.Cmds.Add(new LuaCmd() { Lua = File.ReadAllText(i), Name = fl.Name.Split('.')[0] });
                    }
                }                     
            }
            else
            {
                Directory.CreateDirectory("cmd");
            }
        }

        public void Convert(string script, string schmaticpath)
        {
            Parser p = new Parser();
            var z = p.ParseString(script);

            Methods.Add("classlevel", BuildBody(z.Statements));

            //build clean up code
            List<string> CleanUp = new List<string>();
            int total = 0;
            foreach(var i in Methods)
            {
                foreach(var x in i.Value)
                {
                    total++;
                    if (x.TrimStart('/').StartsWith("scoreboard objectives add"))
                    {
                        CleanUp.Add("/scoreboard objectives remove " + x.TrimStart('/').Split(' ')[3]);
                    }
                }
            }
            Methods["classlevel"].Add("|" + (total / 4));//indicated we need more repeaters
            Methods["classlevel"].AddRange(CleanUp);
            BuildSchematic(schmaticpath);

        }

        public int IFsfound = 0;

        public void iterateLogic(List<string> Lines, IfStat ifs)
        {

            var a = TypeToString((ifs.Condition as BinaryExpression).Left);
            var b = TypeToString((ifs.Condition as BinaryExpression).Right);

            Lines.Add("#/scoreboard players test lua " + a + " " + b + " " + b);
        }


        public List<string> BuildBody(List<IStatement> Statments, List<string> Linesa = null)
        {
            if(Linesa == null)
            {
                Linesa = new List<string>();
            }
            List<string> Lines = Linesa;
            foreach (var i in Statments)
            {
                if(i is IfStat)
                {
                    var x = i as IfStat;
                    iterateLogic(Lines, x);
                    Methods.Add("lua_if_" + IFsfound, BuildBody(x.Block.Statements));
                    var tmplines = Lines.ToArray().ToList();
                    //tmplines.Add("");
                    var bd = BuildBody(new List<IStatement>() { new FunctionCall() { Arguments = new List<IExpression>(), Function = new StringLiteral() { Value = "lua_if_" + IFsfound } } }, tmplines);
                    bd[bd.Count - 1] = bd[bd.Count - 1].TrimStart('/');// 1 tick repeater
                    Lines.Add(bd[bd.Count - 1]);
                    IFsfound++;
                }

                if (i is FunctionCall)
                {
                    // /setblock ~ ~ ~ minecraft:redstone_block
                    var x = i as FunctionCall;
                    if (Methods.ContainsKey(TypeToString(x.Function)))
                    {
                        // note: how there is no /, this is to say we need a delay on the repleater
                        var ins = 0;
                        if ((Methods.Keys.ToList().IndexOf(TypeToString(x.Function)) == 0))
                        {
                            ins = Methods.Count * 2 + (Methods.Keys.ToList().IndexOf(TypeToString(x.Function))) * 2;
                        }
                        else
                        {

                            var index = (Methods.Keys.ToList().IndexOf(TypeToString(x.Function)));
                            ins = (Methods.Count - index) * 2;


                        }
                        Lines.Add("setblock ~-" + (ins) + " ~ ~-" + ((Lines.Count * 2) + 2) + " minecraft:redstone_block");
                    }
                    else
                    {
                        var y = CommandManager.GetCommand(TypeToString(x.Function));
                        Lines.Add(y.Parse(TypeToString(x.Function), ArgBuilder(x.Arguments)));
                    }
                }
                if (i is Assignment)
                {
                    var x = i as Assignment;

                    if (x.Expressions[0] is FunctionDefinition)
                    {
                        var fnc = x.Expressions[0] as FunctionDefinition;
                        Methods.Add(TypeToString((x.Variables[0] as Variable)), BuildBody(fnc.Body.Statements));
                    }
                    else
                    {
                        if (x.Expressions[0] is BinaryExpression)
                        {
                            var name = TypeToString((x.Variables[0] as Variable));
                            var j = x.Expressions[0] as BinaryExpression;
                            iterateBinOp(j, name , Lines);

                        }
                        else
                        {
                            var name = TypeToString((x.Variables[0] as Variable));
                            //Lines.Add("/scoreboard objectives remove " + name);
                            Lines.Add("/scoreboard objectives add " + name + " dummy");
                            Lines.Add("/scoreboard players set lua " + name + " " + TypeToString(x.Expressions[0]));
                        }
                    }
                }
            }

            return Lines;
        }


        public int optc = 0;

        public string iterateBinOp(BinaryExpression bb, string name, List<string> Lines)
        {
            var line = "";
            string a = "";
            string b = "";

            if (bb.Left is BinaryExpression)
            {
                a = name + "_opt_" + optc;
                optc++;
                //Lines.Add("/scoreboard objectives remove " + a);
                Lines.Add("/scoreboard objectives add " + a + " dummy");
                var ln =  iterateBinOp(bb.Left as BinaryExpression, a , Lines);
                Lines.Add("/scoreboard players operation lua " + a + " " + "+=" + " lua " + ln);
            }
            else
            {
                if(char.IsDigit(TypeToString(bb.Left)[0]))
                {
                    //and new var
                    //Lines.Add("/scoreboard objectives remove con_" + ConstantCount);
                    Lines.Add("/scoreboard objectives add con_" + ConstantCount + " dummy");
                    Lines.Add("/scoreboard players set lua con_" + ConstantCount + " " + TypeToString(bb.Left));
                    a = "con_" + ConstantCount;
                    ConstantCount++;
                }
                else
                {
                    a = TypeToString(bb.Left);
                }
            }

            if (bb.Right is BinaryExpression)
            {
                b = name + "_opt_" + optc;
                optc++;
               // Lines.Add("/scoreboard objectives remove " + b);
                Lines.Add("/scoreboard objectives add " + b + " dummy");
                var ln = iterateBinOp(bb.Right as BinaryExpression, b, Lines);
                Lines.Add("/scoreboard players operation lua " + b + " " + "+=" + " lua " + ln);
            }
            else
            {
                if (char.IsDigit(TypeToString(bb.Right)[0]))
                {
                    //and new var
                   // Lines.Add("/scoreboard objectives remove con_" + ConstantCount);
                    Lines.Add("/scoreboard objectives add con_" + ConstantCount + " dummy");
                    Lines.Add("/scoreboard players set lua con_" + ConstantCount + " " + TypeToString(bb.Right));
                    b = "con_" + ConstantCount;
                    ConstantCount++;
                }
                else
                {
                    b = TypeToString(bb.Right);
                }
            }
            

            Lines.Add("/scoreboard players operation lua " + a + " " + BuildBOp(bb.Operation) + " lua " + b );
            line = a;
            return line;
        }

        public string BuildBOp(BinaryOp bo)
        {
            switch(bo)
            {
                case BinaryOp.Addition:
                    return "+=";
                case BinaryOp.Subtraction:
                    return "-=";
                case BinaryOp.Division:
                    return "/=";
                case BinaryOp.Multiplication:
                    return "*=";
            }
            return "+=";
        }

        public List<string> ArgBuilder(List<IExpression> perams)
        {
            var re = new List<string>();
            foreach(var i in perams)
            {
                re.Add(TypeToString(i));
            }
            return re;
        }

        private int getIntFromBitArray(BitArray bitArray)
        {

            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];

        }

        public void BuildSchematic(string schmaticpath)
        {
            int maxl = 0;
            int maxlw = 0;
            foreach (var i in Methods)
            {
                if (i.Value.Count > maxl)
                {
                    maxl = i.Value.Count;
                }
                foreach (var iyy in i.Value)
                {
                    var iz = iyy;
                    if (iz.StartsWith("|"))
                    {
                        iz = iz.TrimStart('|');
                        maxlw += int.Parse(iz);
                    }
                }
            }




            Schematic s = new Schematic();
            s.Height = 1;
            s.Width = (short)((Methods.Count * 2) + maxlw + 5) ;
            s.Length = (short)(maxl * 2 + 2 + 5); 
            s.Fill();
            int x = 0;
            foreach (var ii in Methods)
            {
                int z = 1;
                s.SetBlock(x, 0, 0, new Block() { ID = 93, Metta = 2 });
                foreach (var iyy in ii.Value)
                {
                    var i = iyy;

                    if (i.StartsWith("|"))
                    {
                        i = i.TrimStart('|');
                        for (int iu = 0; iu < int.Parse(i); iu++)
                        {
                            s.SetBlock(x, 0, z, new Block() { ID = 93, Metta = 14 }); // full tic repeater
                            z++;
                        }

                    }
                    else
                    {
                        int b = 2;
                        if (!i.StartsWith("/"))
                        {
                            b = 6;
                        }
                        if (i.StartsWith("#"))
                        {
                            i = i.TrimStart('#');
                            s.SetBlock(x, 0, z + 1, new Block() { ID = 149, Metta = 2 });
                        }
                        else
                        {
                            s.SetBlock(x, 0, z + 1, new Block() { ID = 93, Metta = b });
                        }
                        s.SetBlock(x, 0, z, new Block() { ID = 137, Metta = 0, Command = i });
                        z += 2;
                    }
                }
                s.SetBlock(x, 0, z, new Block() { ID = 137, Metta = 0, Command = "/setblock ~ ~ ~-" + (z + 1) + " minecraft:air" });
                x += 2;
            }
            s.Save(schmaticpath);
        }
           
           
        

        private string TypeToString(IExpression e)
        {
            //this feels so cheety(whatt ever you do dont show this too kenny! xD)
           
            dynamic z = e;
            try
            {
                try
                {
                    return z.Value.ToString();
                }
                catch
                {
                    return z.Name.ToString();
                }
            }
            catch
            {
                return "";
            }
        }
    }
}
