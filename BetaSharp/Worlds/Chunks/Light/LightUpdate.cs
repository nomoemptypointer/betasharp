using BetaSharp.Blocks;
using java.lang;

namespace BetaSharp.Worlds.Chunks.Light;

public struct LightUpdate(LightType lightType, int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
{
    public readonly LightType lightType = lightType;
    public int minX = minX;
    public int minY = minY;
    public int minZ = minZ;
    public int maxX = maxX;
    public int maxY = maxY;
    public int maxZ = maxZ;

    public void UpdateLight(World world)
    {
        int sizeX = maxX - minX + 1;
        int sizeY = maxY - minY + 1;
        int sizeZ = maxZ - minZ + 1;
        int volume = sizeX * sizeY * sizeZ;

        if (volume > -Short.MIN_VALUE)
        {
            Console.WriteLine("Light too large, skipping!");
        }
        else
        {
            int cachedChunkX = 0;
            int cachedChunkZ = 0;
            bool hasCachedChunk = false;
            bool cachedChunkLoaded = false;

            for (int x = minX; x <= maxX; ++x)
            {
                for (int z = minZ; z <= maxZ; ++z)
                {
                    int chunkX = x >> 4;
                    int chunkZ = z >> 4;
                    bool regionLoaded = false;

                    if (hasCachedChunk && chunkX == cachedChunkX && chunkZ == cachedChunkZ)
                    {
                        regionLoaded = cachedChunkLoaded;
                    }
                    else
                    {
                        regionLoaded = world.isRegionLoaded(x, 0, z, 1);
                        if (regionLoaded)
                        {
                            Chunk chunk = world.getChunk(chunkX, chunkZ);
                            if (chunk.isEmpty())
                            {
                                regionLoaded = false;
                            }
                        }

                        cachedChunkLoaded = regionLoaded;
                        cachedChunkX = chunkX;
                        cachedChunkZ = chunkZ;
                        hasCachedChunk = true;
                    }

                    if (regionLoaded)
                    {
                        if (minY < 0)
                            minY = 0;

                        if (maxY >= 128)
                            maxY = 127;

                        for (int y = minY; y <= maxY; ++y)
                        {
                            int currentBrightness = world.getBrightness(lightType, x, y, z);
                            int blockId = world.getBlockId(x, y, z);
                            int opacity = Block.BLOCK_LIGHT_OPACITY[blockId];

                            if (opacity == 0)
                                opacity = 1;

                            int emittedLight = 0;

                            if (lightType == LightType.Sky)
                            {
                                if (world.isTopY(x, y, z))
                                    emittedLight = 15;
                            }
                            else if (lightType == LightType.Block)
                            {
                                emittedLight = Block.BLOCKS_LIGHT_LUMINANCE[blockId];
                            }

                            int newBrightness;

                            if (opacity >= 15 && emittedLight == 0)
                            {
                                newBrightness = 0;
                            }
                            else
                            {
                                int west = world.getBrightness(lightType, x - 1, y, z);
                                int east = world.getBrightness(lightType, x + 1, y, z);
                                int down = world.getBrightness(lightType, x, y - 1, z);
                                int up = world.getBrightness(lightType, x, y + 1, z);
                                int north = world.getBrightness(lightType, x, y, z - 1);
                                int south = world.getBrightness(lightType, x, y, z + 1);

                                newBrightness = west;
                                if (east > newBrightness) newBrightness = east;
                                if (down > newBrightness) newBrightness = down;
                                if (up > newBrightness) newBrightness = up;
                                if (north > newBrightness) newBrightness = north;
                                if (south > newBrightness) newBrightness = south;

                                newBrightness -= opacity;
                                if (newBrightness < 0)
                                    newBrightness = 0;

                                if (emittedLight > newBrightness)
                                    newBrightness = emittedLight;
                            }

                            if (currentBrightness != newBrightness)
                            {
                                world.setLight(lightType, x, y, z, newBrightness);

                                int propagatedLight = newBrightness - 1;
                                if (propagatedLight < 0)
                                    propagatedLight = 0;

                                world.updateLight(lightType, x - 1, y, z, propagatedLight);
                                world.updateLight(lightType, x, y - 1, z, propagatedLight);
                                world.updateLight(lightType, x, y, z - 1, propagatedLight);

                                if (x + 1 >= maxX)
                                    world.updateLight(lightType, x + 1, y, z, propagatedLight);

                                if (y + 1 >= maxY)
                                    world.updateLight(lightType, x, y + 1, z, propagatedLight);

                                if (z + 1 >= maxZ)
                                    world.updateLight(lightType, x, y, z + 1, propagatedLight);
                            }
                        }
                    }
                }
            }
        }
    }

    public bool Expand(int newMinX, int newMinY, int newMinZ, int newMaxX, int newMaxY, int newMaxZ)
    {
        if (newMinX >= minX && newMinY >= minY && newMinZ >= minZ && newMaxX <= maxX && newMaxY <= maxY && newMaxZ <= maxZ)
        {
            return true;
        }
        else
        {
            byte expansionMargin = 1;

            if (newMinX >= minX - expansionMargin &&
                newMinY >= minY - expansionMargin &&
                newMinZ >= minZ - expansionMargin &&
                newMaxX <= maxX + expansionMargin &&
                newMaxY <= maxY + expansionMargin &&
                newMaxZ <= maxZ + expansionMargin)
            {
                int currentSizeX = maxX - minX;
                int currentSizeY = maxY - minY;
                int currentSizeZ = maxZ - minZ;

                if (newMinX > minX) newMinX = minX;
                if (newMinY > minY) newMinY = minY;
                if (newMinZ > minZ) newMinZ = minZ;

                if (newMaxX < maxX) newMaxX = maxX;
                if (newMaxY < maxY) newMaxY = maxY;
                if (newMaxZ < maxZ) newMaxZ = maxZ;

                int newSizeX = newMaxX - newMinX;
                int newSizeY = newMaxY - newMinY;
                int newSizeZ = newMaxZ - newMinZ;

                int currentVolume = currentSizeX * currentSizeY * currentSizeZ;
                int newVolume = newSizeX * newSizeY * newSizeZ;

                if (newVolume - currentVolume <= 2)
                {
                    minX = newMinX;
                    minY = newMinY;
                    minZ = newMinZ;
                    maxX = newMaxX;
                    maxY = newMaxY;
                    maxZ = newMaxZ;
                    return true;
                }
            }
            return false;
        }
    }

}