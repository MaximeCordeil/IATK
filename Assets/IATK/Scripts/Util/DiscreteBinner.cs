using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IATK
{

    // code from https://jamesmccaffrey.wordpress.com/2014/06/23/equal-width-binning-using-c/
    public class DiscreteBinner {

    	float[] intervals;
    	public float[] bins;

    	public float[] MakeIntervals(float[] data, int numBins)
    	{
            if (data != null && data.Length > 0 && numBins > 0)
            {
        		float max = data[0]; // find min & max
        		float min = data[0];
        		for (int i = 0; i < data.Length; ++i)
        		{
        			if (data[i] < min) min = data[i];
        			if (data[i] > max) max = data[i];
        		}
        		float width = (max - min) / numBins; // compute width

        		intervals = new float[numBins * 2]; // intervals
        		intervals[0] = min;
        		intervals[1] = min + width;
        		for (int i = 2; i < intervals.Length - 1; i += 2)
        		{
        			intervals[i] = intervals[i - 1];
        			intervals[i + 1] = intervals[i] + width;
        		}
        		intervals[0] = float.MinValue; // outliers
        		intervals[intervals.Length - 1] = float.MaxValue;

        		bins = new float[numBins];

        		return intervals;
            }

            return null;
    	}

    	public int Bin(float x)
    	{
            if (intervals != null)
            {
          		for (int i = 0; i < intervals.Length - 1; i += 2)
          		{
            		if (x >= intervals[i] && x < intervals[i + 1]){
        				bins[i / 2] += 1;
              			return i / 2;
        			}
          		}
            }

      		return -1; // error
    	}

    }

}   // Namespace