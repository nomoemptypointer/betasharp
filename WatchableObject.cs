namespace betareborn
{

    public class WatchableObject : java.lang.Object
    {
        private readonly int objectType;
        private readonly int dataValueId;
        private java.lang.Object watchedObject;
        private bool isWatching;

        public WatchableObject(int var1, int var2, java.lang.Object var3)
        {
            this.dataValueId = var2;
            this.watchedObject = var3;
            this.objectType = var1;
            this.isWatching = true;
        }

        public int getDataValueId()
        {
            return this.dataValueId;
        }

        public void setObject(java.lang.Object var1)
        {
            this.watchedObject = var1;
        }

        public java.lang.Object getObject()
        {
            return this.watchedObject;
        }

        public int getObjectType()
        {
            return this.objectType;
        }

        public bool getWatching()
        {
            return this.isWatching;
        }

        public void setWatching(bool var1)
        {
            this.isWatching = var1;
        }
    }
}