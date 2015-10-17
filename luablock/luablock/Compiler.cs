﻿using luablock.Commands;
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

            BuildSchematic(schmaticpath);

        }

        public List<string> BuildBody(List<IStatement> Statments)
        {
            List<string> Lines = new List<string>();
            foreach (var i in Statments)
            {
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
                            ins =  (Methods.Count - index ) * 2;


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
                }
            }

            return Lines;
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
            foreach (var i in Methods)
            {
                if (i.Value.Count > maxl)
                {
                    maxl = i.Value.Count;
                }
            }

            Schematic s = new Schematic();
            s.Height = 1;
            s.Width = (short)(Methods.Count * 2);
            s.Length = (short)(maxl * 2 + 2);
            s.Fill();
            int x = 0;
            foreach (var ii in Methods)
            {
                int z = 1;
                s.SetBlock(x, 0, 0, new Block() { ID = 93, Metta = 2 });
                foreach (var i in ii.Value)
                {


                    int b = 2;
                    if (!i.StartsWith("/"))
                    {
                        b = 6;
                    }
                    s.SetBlock(x, 0, z + 1, new Block() { ID = 93, Metta = b});
                    s.SetBlock(x, 0, z, new Block() { ID = 137, Metta = 0, Command = i });
                    z += 2;
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