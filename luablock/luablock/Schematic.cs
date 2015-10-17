using fNbt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luablock
{
    public class Schematic
    {


        public short Width { get; set; }
        public short Height { get; set; }
        public short Length { get; set; }

        private byte[] _data { get; set; }
        private byte[] _biomes { get; set; }
        private byte[] _blocks { get; set; }

        public List<Block> Blocks { get; set; } = new List<Block>();


        public void Fill()
        {
            var size = Width * Height * Length;

            for (int i = 0; i < size; i++)
            {
                Blocks.Add(new Block());
            }

        }


        public void SetBlock(int x, int y, int z, Block b)
        {
            //(y * Length + z)* Width + x
            Blocks[(y * Length + z) * Width + x] = b;
        }

        public Block GetBlock(int x, int y, int z)
        {
            //(y * Length + z)* Width + x
            return Blocks[(y * Length + z) * Width + x];
        }

        public void Load(string p)
        {
            var myFile = new NbtFile();
            myFile.LoadFromFile(p);
            var root = myFile.RootTag;

            Width = root.Get("Width").ShortValue;
            Height = root.Get("Height").ShortValue;
            Length = root.Get("Length").ShortValue;

            _data = root.Get("Data").ByteArrayValue;
            _biomes = root.Get("Biomes").ByteArrayValue;
            _blocks = root.Get("Blocks").ByteArrayValue;

            Fill();



            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Length; z++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        SetBlock(x, y, z, new Block() { ID = _blocks[(y * Length + z) * Width + x], Metta = _data[(y * Length + z) * Width + x] & 0x0F });
                    }
                }
            }
          
        
        }

        public void Save(string p)
        {
            var myFile = new NbtFile();
       
            var root = myFile.RootTag;
            root.Name = "Schematic"; 
            
            root.Add(new NbtShort("Height", Height));
            root.Add(new NbtShort("Length", Length));
            root.Add(new NbtShort("Width", Width));


            var nbttile = new NbtList("TileEntities", NbtTagType.Compound);
            var size = Width * Height * Length;
            _data = new byte[size];
            _blocks = new byte[size];

            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Length; z++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var d = GetBlock(x, y, z);
                        _blocks[(y * Length + z) * Width + x] = (byte)d.ID;
                        _data[(y * Length + z) * Width + x] = (byte)(d.Metta & 0x0F);

                        if (d.Command != null)
                        {
                            var nbtc = new NbtCompound();
                            nbtc.Add(new NbtString("CustomName", "LuaBlock"));
                            nbtc.Add(new NbtString("Command", d.Command));
                            nbtc.Add(new NbtString("id", "Control"));
                            nbtc.Add(new NbtInt("x", x));
                            nbtc.Add(new NbtInt("y", y));
                            nbtc.Add(new NbtInt("z", z));

                            nbttile.Add(nbtc);
                        }
                    }
                }
            }
            root.Add(new NbtList("Entities", NbtTagType.Byte));
            root.Add(nbttile);
            root.Add(new NbtList("TileTicks", NbtTagType.Byte));

            root.Add(new NbtString("Materials", "Alpha"));

           

            root.Add(new NbtByteArray("Data", _data));
         //   root.Add(new NbtByteArray("Biomes", new byte[Width * Length]));
            root.Add(new NbtByteArray("Blocks", _blocks));
            
           
            myFile.SaveToFile(p, NbtCompression.None);
        }
    }

    public class Block
    {
        public int ID { get; set; } = 0;
        public int Metta { get; set; } = 0;
        public string Command { get; set; } = null;
    }
}
