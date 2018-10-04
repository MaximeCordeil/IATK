using System.Collections;
using UnityEngine;
using System;

public delegate void DelegateFor(int i);
public delegate void DelegateProcess(int from, int to);


public class Parallel : MonoBehaviour
{

    public static void For(int from, int to, DelegateFor delFor)
    {
        DelegateProcess process = delegate(int chunkStart, int chunkEnd)
        {
            for (int i = chunkStart; i < chunkEnd; ++i)
                delFor(i);
        };

        int cores = Environment.ProcessorCount;
        int chunks = (to - from) / cores;

        IAsyncResult[] asyncResults = new IAsyncResult[cores];

        int end = 0;
        for (int i = 0; i < cores; ++i)
        {
            int start = i * chunks;
            end = Math.Min(start + chunks, to);
            asyncResults[i] = process.BeginInvoke(start, end, null, null);
        }

        for (int i = end; i < to; ++i)
            delFor(i);

        for (int i = 0; i < cores; ++i)
            process.EndInvoke(asyncResults[i]);
    }

}