using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IATK
{
    public enum DataType
    {
        Undefined,
        Float,
        Int,
        Bool,
        String,
        Date,
        Time,
        Graph
    }

    public static class DataTypeExtension
    {
        public static DataType inferFromString(string data)
        {
            if (isBool(data))
            {
                return DataType.Bool;
            }
            else if (isDate(data))
            {
                return DataType.Date;
            }
            else if (isTime(data))
            {
                return DataType.Time;
            }
            else if (isInt(data))
            {
                return DataType.Int;
            }
            else if (isFloat(data))
            {
                return DataType.Float;
            }
            else if(isGraph(data))
            {
                return DataType.Graph;
            }
            else if (!String.IsNullOrEmpty(data))
            {
                return DataType.String;
            }
            else
            {
                return DataType.Undefined;
            }
        }

        private static bool isGraph(string data)
        {
            return data.Contains("|");
        }

        private static bool isBool(string value)
        {
            bool res = false;
            return bool.TryParse(value, out res);
        }

        private static bool isInt(string value)
        {
            int res = 0;
            return int.TryParse(value, out res);
        }

        private static bool isFloat(string value)
        {
            float res = 0.0f;
            return float.TryParse(value, out res);
        }

        private static bool isDate(string value)
        {
            return value.Contains(@"\");
        }

        private static bool isTime(string value)
        {
            return false;// value.Contains(":");
        }
    }

}   // Namespace