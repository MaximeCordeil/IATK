using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
namespace IATK
{

    public class UtilMath {

        public enum Axis
        {
            X,
            Y,
            Z
        }

    	//constants
        public static float FL_TO_M = 0.3048f/2f;

    	//Unity max vertices count per mesh
    	public static int MAXIMUM_VERTICES_COUNT = 65534; 

    	/* scale value from i0...i1 to j0...j1
         */
    	public static float normaliseValue(float value, float i0, float i1, float j0, float j1)
    	{
    		float L = (j0 - j1) / (i0 - i1);
    		return (j0 - (L * i0) + (L * value));
    	}

    	public static float animateSlowInSlowOut(float t)
    	{
    		if (t <= 0.5f)
    			return 2.0f * t * t;

    		else
    			return 1.0f - 2.0f * (1.0f - t) * (1.0f - t);            
    	}

    	//format fileName : @"C:\Users\maxc\Documents\Maxime\DATA FOR VISUALISATION\TEST.BIN"
    	public static void SerializeVector3(Vector3[] data, string fileName)
    	{
    		using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
    		{
    			BinaryFormatter bf = new BinaryFormatter();
    			bf.Serialize(fs, data);
    		}
    	}

    	//format fileName : @"C:\Users\maxc\Documents\Maxime\DATA FOR VISUALISATION\TEST.BIN"
    	public static Vector3[] DeserializeVector3(string fileName)
    	{
    		using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
    		{

    			BinaryFormatter bf = new BinaryFormatter();
    			Vector3[] result = (Vector3[])bf.Deserialize(fs);		
    			
    			return result;
    		}
    	}

    	//format fileName : @"C:\Users\maxc\Documents\Maxime\DATA FOR VISUALISATION\TEST.BIN"
    	public static void SerializeInt(int[] data, string fileName)
    	{
    		using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
    		{
    			BinaryFormatter bf = new BinaryFormatter();
    			bf.Serialize(fs, data);
    		}
    	}
    	
    	//format fileName : @"C:\Users\maxc\Documents\Maxime\DATA FOR VISUALISATION\TEST.BIN"
    	public static int[] DeserializeInt(string fileName)
    	{
    		using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
    		{
    			
    			BinaryFormatter bf = new BinaryFormatter();
    			int[] result = (int[])bf.Deserialize(fs);		
    			
    			return result;
    		}
    	}

    	/// <summary>
    	/// Projects a point on sphere.
    	/// </summary>
    	/// <returns>The on sphere.</returns>
    	/// <param name="center">Center.</param>
    	/// <param name="radius">Radius.</param>
    	/// <param name="x">The x coordinate.</param>
    	/// <param name="y">The y coordinate.</param>
        /// 
    	public static Vector3 projectOnSphere (Vector3 center, float radius, float x, float y)
    	{
    		float theta = 2f * Mathf.PI * x;
    		float phi = Mathf.Acos(2f * y - 1f);
    		
    		float xS = center.x + (radius * Mathf.Sin(theta) * Mathf.Cos(phi));
    		float yS = center.y + (radius * Mathf.Sin(phi) * Mathf.Sin(theta));
    		float zS = ( center.z + (radius * Mathf.Cos(theta)));
    		
    		return new Vector3(xS,yS,zS);
    	}

        public static Vector3 GPS_to_Spherical(Vector3 center, float _lat, float _lon, float earthRadius, float altitude)
        {

          //excentricity correction...
          // % WGS84 ellipsoid constants:
          //  a = 6378137;
          // e = 8.1819190842622e-2;

          // % intermediate calculation
          //% (prime vertical radius of curvature)
          //N = a ./ sqrt(1 - e^2 .* sin(lat).^2);

          /*  Vector3 spherical = new Vector3(((_lat) * Mathf.Deg2Rad), (_lon * Mathf.Deg2Rad - Mathf.PI / 2f), earthRadius);
              float xS = (earthRadius + altitude) * Mathf.Sin(spherical.x) * Mathf.Cos(spherical.y);
              float yS = (earthRadius + altitude) * Mathf.Sin(spherical.x) * Mathf.Sin(spherical.y);
              float zS = (earthRadius + altitude) * Mathf.Cos(spherical.x);
          */

            //Vector3 spherical = new Vector3(((_lat) * Mathf.Deg2Rad), (_lon * Mathf.Deg2Rad - Mathf.PI / 2f), earthRadius);
            var lat = Mathf.Deg2Rad*_lat;
            var lon = Mathf.Deg2Rad*_lon;

            float xS = (earthRadius + altitude) * Mathf.Cos(lat) * Mathf.Cos(lon);
            float yS = (earthRadius + altitude) * Mathf.Cos(lat) * Mathf.Sin(lon);
            float zS = (earthRadius + altitude) * Mathf.Sin(lat);

           /* float xS = (earthRadius + altitude) * Mathf.Cos(spherical.y) * Mathf.Sin(spherical.x);
            float yS = (earthRadius + altitude) * Mathf.Sin(spherical.y) * Mathf.Sin(spherical.x);
            float zS = (earthRadius + altitude) * Mathf.Cos(spherical.x);*/

           // Vector3 p;
            
            return new Vector3(xS,yS,zS);

        }

        public static float[] diffArray(float[] from, float[] to )
        {
            List<float> diff_ = new List<float>();

            for (int i = 0; i < from.Length; i++)
                diff_.Add(from[i] - to[i]);

            return diff_.ToArray();
        }

        public static string printPositionCSV(Vector3 position, int precision)
        {
            return position.x.ToString("G" + precision) + "," + position.y.ToString("G" + precision) + "," + position.z.ToString("G" + precision);
        }

        public static string printRotationCSV(Quaternion rotation, int precision)
        {
            return rotation.x.ToString("G" + precision) + "," + rotation.y.ToString("G" + precision) + "," + rotation.z.ToString("G" + precision) + "," + rotation.w.ToString("G" + precision);
        }

        public static Vector3 swizzle(Vector3 vec, string swizzleString)
        {
            if (swizzleString.Length == 3)
            {
                int xIndex = swizzleString.IndexOf("x");
                int yIndex = swizzleString.IndexOf("y");
                int zIndex = swizzleString.IndexOf("z");

                return new Vector3(vec[xIndex], vec[yIndex], vec[zIndex]);
            }

            return vec;
        }
    }

}   // Namespace