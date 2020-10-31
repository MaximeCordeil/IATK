using UnityEngine;

//From https://github.com/nicoptere/unity-xyz/blob/master/Assets/map/Mercator.cs

public class Mercator
{

    private int tileSize;
    private float earthRadius;
    private float initialResolution;
    private float originShift;

    public Mercator(int tile_size = 256, float earth_radius = 6378137f)
    {
        //Initialize the TMS Global Mercator pyramid
        tileSize = tile_size;
        earthRadius = earth_radius;

        // 1 / 156543.03392804062 for tileSize 256 pixels
        initialResolution = 2F * Mathf.PI * earthRadius / tileSize;

        // 20037508.342789244
        originShift = Mathf.PI * earthRadius;

    }


    //converts given lat/lon in WGS84 Datum to XY in Spherical Mercator EPSG:900913
    public float[] latLonToMeters(float lat, float lon)
    {
        float mx = lon * originShift / 180F;
        float my = Mathf.Log(Mathf.Tan((90f + lat) * Mathf.PI / 360F)) / (Mathf.PI / 180F);
        my = my * originShift / 180F;
        return new float[] { mx, my };
    }

    //converts XY point from Spherical Mercator EPSG:900913 to lat/lon in WGS84 Datum
    public float[] metersToLatLon(float mx, float my)
    {
        var lon = (mx / originShift) * 180F;
        var lat = (my / originShift) * 180F;
        lat = 180 / Mathf.PI * (2F * Mathf.Atan(Mathf.Exp(lat * Mathf.PI / 180F)) - Mathf.PI / 2F);
        return new float[] { lat, lon };
    }

    //converts pixel coordinates in given zoom level of pyramid to EPSG:900913
    public float[] pixelsToMeters(float px, float py, float zoom)
    {
        float res = resolution(zoom);
        float mx = px * res - originShift;
        float my = py * res - originShift;
        return new float[] { mx, my };
    }

    //converts EPSG:900913 to pyramid pixel coordinates in given zoom level
    public float[] metersToPixels(float mx, float my, float zoom)
    {
        float res = resolution(zoom);
        float px = (mx + originShift) / res;
        float py = (my + originShift) / res;
        return new float[] { px, py };
    }

    //returns tile for given mercator coordinates
    public int[] metersToTile(float mx, float my, float zoom)
    {
        float[] pxy = metersToPixels(mx, my, zoom);
        return pixelsToTile(pxy[0], pxy[1]);
    }

    //returns a tile covering region in given pixel coordinates
    public int[] pixelsToTile(float px, float py)
    {
        int tx = (int)(Mathf.Ceil(px / (float)tileSize) - 1f);
        int ty = (int)(Mathf.Ceil(py / (float)tileSize) - 1f);
        return new int[] { tx, ty };
    }

    //returns a tile covering region the given lat lng coordinates
    public int[] latLonToTile(float lat, float lng, float zoom)
    {
        float[] px = latLngToPixels(lat, lng, zoom);
        return pixelsToTile(px[0], px[1]);
    }

    //Move the origin of pixel coordinates to top-left corner
    public int[] pixelsToRaster(int px, int py, float zoom)
    {
        int mapSize = tileSize << (int)zoom;
        return new int[] { px, mapSize - py };
    }

    //returns bounds of the given tile in EPSG:900913 coordinates
    public float[] tileMetersBounds(float tx, float ty, float zoom)
    {
        float[] min = pixelsToMeters(tx * tileSize, ty * tileSize, zoom);
        float[] max = pixelsToMeters((tx + 1) * tileSize, (ty + 1) * tileSize, zoom);
        return new float[] { min[0], min[1], max[0], max[1] };
    }

    //returns bounds of the given tile in pixels
    public float[] tilePixelsBounds(float tx, float ty, float zoom)
    {
        float[] bounds = tileMetersBounds(tx, ty, zoom);
        float[] min = metersToPixels(bounds[0], bounds[1], zoom);
        float[] max = metersToPixels(bounds[2], bounds[3], zoom);
        return new float[] { min[0], min[1], max[0], max[1] };
    }

    //returns bounds of the given tile in latutude/longitude using WGS84 datum
    public float[] tileLatLngBounds(float tx, float ty, float zoom)
    {
        float[] bounds = tileMetersBounds(tx, ty, zoom);
        float[] min = metersToLatLon(bounds[0], bounds[1]);
        float[] max = metersToLatLon(bounds[2], bounds[3]);
        return new float[] { min[0], min[1], max[0], max[1] };
    }

    //resolution (meters/pixel) for given zoom level (measured at Equator)
    public float resolution(float zoom)
    {
        return initialResolution / Mathf.Pow(2, zoom);
    }

    //returns the lat lng of a pixel X/Y coordinates at given zoom level
    public float[] pixelsToLatLng(float px, float py, float zoom)
    {
        float[] meters = pixelsToMeters(px, py, zoom);
        return metersToLatLon(meters[0], meters[1]);
    }

    //returns the pixel X/Y coordinates at given zoom level from a given lat lng
    public float[] latLngToPixels(float lat, float lng, float zoom)
    {
        float[] meters = latLonToMeters(lat, lng);
        return metersToPixels(meters[0], meters[1], zoom);
    }

    //retrieves a given tile from a given lat lng
    public int[] latLngToTile(float lat, float lng, float zoom)
    {
        float[] meters = latLonToMeters(lat, lng);
        return metersToTile(meters[0], meters[1], zoom);
    }

    //encodes the tlie X / Y coordinates & zoom level into a quadkey
    public string tileXYToQuadKey(int tx, int ty, float zoom)
    {
        string quadKey = "";
        for (int i = (int)zoom; i > 0; i--)
        {
            var digit = 0;
            var mask = 1 << (i - 1);
            if ((tx & mask) != 0)
            {
                digit++;
            }
            if ((ty & mask) != 0)
            {
                digit++;
                digit++;
            }
            quadKey += digit.ToString();
        }
        //Debug.Log("-> " + quadKey);
        //Debug.Log( "<- " + quadKeyToTileXY( quadKey ) );
        return quadKey;
    }

    //decodes the tlie X / Y coordinates & zoom level into a quadkey
    public int[] quadKeyToTileXY(string quadKey)
    {
        int tileX = 0;
        int tileY = 0;
        int levelOfDetail = quadKey.Length;
        for (var i = levelOfDetail; i > 0; i--)
        {
            var mask = 1 << (i - 1);
            switch (quadKey[levelOfDetail - i])
            {
                case '0':
                    break;

                case '1':
                    tileX |= mask;
                    break;

                case '2':
                    tileY |= mask;
                    break;

                case '3':
                    tileX |= mask;
                    tileY |= mask;
                    break;

                default:
                    return null;
            }
        }
        //Debug.Log( "tx " + tileX.ToString() + " ty " + tileY.ToString() + " lod " + levelOfDetail );
        return new int[] { tileX, tileY, levelOfDetail };
    }

}
