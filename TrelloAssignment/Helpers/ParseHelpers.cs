﻿using System;
using System.ComponentModel;

namespace TrelloAssignment.Helpers
{
    public static class ParseHelpers
    {
        public static T As<T>(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return default;
            }

            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    return (T)converter.ConvertFromString(input);
                }
                return default;
            }
            catch (NotSupportedException)
            {
                return default;
            }

        }
    }
}
