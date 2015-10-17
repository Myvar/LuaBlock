using luablock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
          //  Schematic s1 = new Schematic();
           // s1.Load("tt.schematic");
       


          

            Compiler c = new Compiler();
            c.Convert(File.ReadAllText(args[0]), args[1]);
        }
    }
}
