using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetScanner
{
    public class IPRange
    {
        public IPAddress From, To;
        public static IPRange CreateFromIpAndMask(IPAddress ip, IPAddress mask)
        {
            return new IPRange()
            {
                From = new IPAddress(Operate(ip.GetAddressBytes(), mask.GetAddressBytes(), (x, y) => (byte)(x & y))),
                To = new IPAddress(Operate(ip.GetAddressBytes(), mask.GetAddressBytes(), (x, y) => (byte)(x | ~y)))
            };
        }

        public static IPRange CreateFromIpAndMaskBits(IPAddress ip, int maskBits)
        {
            return CreateFromIpAndMask(ip, MaskBitsToMask(maskBits));
        }

        public static IEnumerable<IPRange> Parse(string text)
        {
            foreach (string s in text.Split(','))
            {
                if (s.Contains("-"))
                {
                    string[] splitted = s.Split('-');
                    yield return new IPRange()
                    {
                        From = IPAddress.Parse(splitted[0]),
                        To = IPAddress.Parse(splitted[1])
                    };
                }
                else if (s.Contains("/"))
                {
                    string[] splitted = s.Split('/');
                    yield return CreateFromIpAndMaskBits(IPAddress.Parse(splitted[0]), int.Parse(splitted[1]));
                }
                else
                    yield return CreateFromIpAndMaskBits(IPAddress.Parse(s), 32);
            }
        }

        public unsafe static IPAddress MaskBitsToMask(int bits)
        {
            byte[] buffer = new byte[4];
            fixed (byte* ptr = buffer)
                *((uint*)ptr) = ((uint)1 << bits) - 1;

            return new IPAddress(buffer);
        }

        static T[] Operate<T>(T[] arg1, T[] arg2, Func<T, T, T> func)
        {
            if (arg1 == null || arg2 == null)
                throw new ArgumentException("Null argument exception");

            if (arg1.Length != arg2.Length)
                throw new ArgumentException("Size not equal");

            T[] arr = new T[arg1.Length];

            for (int i = 0; i < arg1.Length; i++)
                arr[i] = func(arg1[i], arg2[i]);

            return arr;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", From, To);
        }
    }

    public class ScannerHost
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public string OS { get; set; }
        public string ComputerName { get; set; }
    }
}
