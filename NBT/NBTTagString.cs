using java.io;

namespace betareborn.NBT
{
    public class NBTTagString : NBTBase
    {

        public string stringValue;

        public NBTTagString()
        {
        }

        public NBTTagString(string var1)
        {
<<<<<<< HEAD
            stringValue = value;
=======
            stringValue = var1;
            if (var1 == null)
            {
                throw new IllegalArgumentException("Empty string not allowed");
            }
>>>>>>> parent of 96cef13 (Merge pull request #38 from TheVeryStarlk/nbt-refactor)
        }

        public override void writeTagContents(DataOutput var1)
        {
            var1.writeUTF(stringValue);
        }

        public override void readTagContents(DataInput var1)
        {
            stringValue = var1.readUTF();
        }

        public override byte getType()
        {
            return (byte)8;
        }

        public override string toString()
        {
            return "" + stringValue;
        }
    }

}