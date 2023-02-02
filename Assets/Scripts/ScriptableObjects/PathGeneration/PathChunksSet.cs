using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PathChunksSet : RuntimeSet<PathChunk>
{
    public void Remove(int chunkIndex)
    {
        foreach(PathChunk chunk in this.Items)
        {
            if(chunk.Index == chunkIndex)
            {
                this.Items.Remove(chunk);
                return;
            }
        }
    }

    public PathChunk Get(int chunkIndex)
    {
        PathChunk _tmp = null;
        foreach (PathChunk chunk in this.Items)
        {
            if (chunk.Index == chunkIndex)
            {
                _tmp = chunk;
                break;
            }
        }
        return _tmp;
    }

    public void Destroy()
    {
        foreach (PathChunk chunk in this.Items)
        {
            chunk.Destroy();
        }
        Items.Clear();

    }
}


