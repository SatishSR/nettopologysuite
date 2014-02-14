﻿using System;

namespace NetTopologySuite.Utilities
{
    public static class EnumUtility
    {
        public static object Parse(Type type, string value)
        {
#if SILVERLIGHT || PCL
            return Enum.Parse(type, value, false);
#else
            return Enum.Parse(type, value);
#endif
        }

        public static string Format(Type type,object value, string formatString)
        {
#if SILVERLIGHT || PCL
            throw new NotImplementedException();
#else
            return Enum.Format(type, value, formatString);
#endif
        }
    }
}