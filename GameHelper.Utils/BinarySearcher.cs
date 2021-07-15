﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using GameHelper.Interfaces.LowLevel;

[assembly:InternalsVisibleTo("GameHelper.Tests")]

namespace GameHelper.Utils
{
    public class BinarySearcher: IBinarySearcher
    {
        private static readonly BinarySearchMatch[] NoMatches = new BinarySearchMatch[0];

        public IReadOnlyCollection<BinarySearchMatch> Search<T>(IReadOnlyCollection<byte[]> items, T value)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var result = new List<BinarySearchMatch>();
            foreach (var data in items)
                result.AddRange(Search(data, value));
            return result;
        }

        internal static IReadOnlyCollection<BinarySearchMatch> Search<T>(byte[] data, T value)
        {
            if (typeof(T) == typeof(string))
                if (value is string s)
                    return SearchString(data, s);

            if (typeof(T) == typeof(byte))
                if (value is byte b)
                    return SearchByte(data, b);

            if (typeof(T) == typeof(ushort))
                if (value is ushort us)
                    return SearchUShort(data, us);

            throw new NotImplementedException();
        }

        private static IReadOnlyCollection<BinarySearchMatch> SearchString(byte[] data, string value)
        {
            if (string.IsNullOrEmpty(value))
                return NoMatches;

            if (data.Length < value.Length)
                return NoMatches;

            var result = new List<BinarySearchMatch>();
            foreach (var encodingInfo in Encoding.GetEncodings())
            {
                var s = encodingInfo.GetEncoding().GetString(data);
                if (string.IsNullOrEmpty(s))
                    continue;

                for (var index = 0; ; index += value.Length)
                {
                    index = s.IndexOf(value, index);
                    if (index == -1)
                        break;

                    if (result.Any(m => m.Position == index && m.Length == value.Length))
                        continue;

                    result.Add(new BinarySearchMatch(data, index, value.Length));
                }
            }
            return result;
        }

        private static IReadOnlyCollection<BinarySearchMatch> SearchByte(byte[] data, byte value)
        {
            if (data.Length == 0)
                return NoMatches;

            var result = new List<BinarySearchMatch>();
            for (var i = 0; i < data.Length; i++)
                if (data[i] == value)
                    result.Add(new BinarySearchMatch(data, i, 1));
            return result;
        }

        private static IReadOnlyCollection<BinarySearchMatch> SearchUShort(byte[] data, ushort value)
        {
            if (data.Length == 0)
                return NoMatches;

            var result = new List<BinarySearchMatch>();
            for (var i = 0; i < data.Length -1; i++)
            {
                var b1 = data[i];
                var b2 = data[i + 1];
                var u = (ushort)(b1 * 256 + b2);
                if (u == value)
                    result.Add(new BinarySearchMatch(data, i, 2));
            }

            return result;
        }
    }
}
