﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ws.winx.utils
{
    public static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }
}
