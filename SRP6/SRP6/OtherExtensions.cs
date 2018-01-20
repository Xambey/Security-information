using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace SRP6
{
    public static class OtherExtensions
    {

        public static BigInteger CreateSaltedIdentityHash(this BigInteger salt, byte[] identityHash)
        {
            return BigIntegerExtensions.CreateBigInteger(
                Concatenate(salt.ToByteArray(), identityHash).Sha1Hash().ToHexString(), 16);
        }

        public static byte[] Sha1Hash(this byte[] arg1)
        {
            try
            {
                if ((arg1 == null) || (arg1.Length == 0))
                    throw new InvalidOperationException("arg1 can not be null or have a length of zero.");
                SHA1 sha = new SHA1CryptoServiceProvider();
                sha.ComputeHash(arg1);
                return sha.Hash;
            }
            catch(Exception e)
            {
                Console.WriteLine("Sha1Hash: " + e.Message);
                return null;
            }
        }

        private static byte[] Concatenate(params byte[][] arrays)
        {
            var length = arrays.Where(array => array != null).Sum(array => array.Length) + 1;
            var result = new byte[length];
            length = 0;
            foreach (var array in arrays.Where(array => array != null))
            {
                Array.Copy(array, 0, result, length, array.Length);
                length += array.Length;
            }
            return result;
        }

        public static T[] SubArray<T>(this T[] sourceArray, int offset, int length)
        {
            var result = new T[length];
            Array.Copy(sourceArray, offset, result, 0, length);
            return result;
        }

        public static string ToHexString(this byte[] byteArray)
        {
            if (byteArray == null)
                return "";
            string result = "";
            for (int i = byteArray.Length - 1; i >= 0; i--)
                result += byteArray[i].ToString("X2");
            return result;
        }

        public static string ToString(this byte[] totalArray, int offset, int length)
        {
            byte[] array = SubArray(totalArray, offset, length);
            return Encoding.ASCII.GetString(array);
        }

    }
}

