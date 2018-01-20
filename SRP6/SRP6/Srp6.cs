using System;
using System.Numerics;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace SRP6
{
    public class Srp6
    {
        public BigInteger Modulus
        {
            get;
            private set;
        }

        public BigInteger Salt
        {
            get;
            private set;
        }

        public BigInteger Generator
        {
            get;
            private set;
        }

        public BigInteger Multiplier
        {
            get;
            private set;
        }

        public byte[] IdentityHash
        {
            get;
            private set;
        }

        //***************************************************
        
        public BigInteger SaltedIdentityHash
        {
            get;
            private set;
        }

        public BigInteger Scrambler
        {
            get;
            private set;
        }

        public BigInteger Verifier
        {
            get;
            private set;
        }

        public BigInteger SessionKey
        {
            get;
            private set;
        }

        public BigInteger PrivateKey
        {
            get;
            private set;
        }

        public BigInteger PublicKey
        {
            get;
            private set;
        }

        public string InitialVector
        {
            get;
            private set;
        }
        
        public string HashAlgorithm
        {
            get;
            private set;
        }
        
        public int PasswordIterations
        {
            get;
            private set;
        }

        public int KeySize
        {
            get;
            private set;
        }
        
        public bool IsServerInstance
        {
            get;
            private set;
        }

        //инициализация клиента
        private Srp6(string initialVector, string modulus, int generator, byte[] identityHash)
        {
            IsServerInstance = true;
            HashAlgorithm = "SHA1";
            PasswordIterations = 2;
            KeySize = 256;
            InitialVector = initialVector;

            IdentityHash = identityHash;
            Modulus = BigIntegerExtensions.CreateBigInteger(modulus, 16);
            Generator = BigIntegerExtensions.CreateBigInteger("" + generator, 10);
            Multiplier = BigIntegerExtensions.CreateBigInteger("3", 10);
        }

        //сервер
        public Srp6(byte[] identityHash, string modulus, int generator, int saltBits,
            int scramblerBits, string initialVector = "OFRna73m*aze01xY")
            : this(initialVector, modulus, generator, identityHash)
        {
            IsServerInstance = true;
            
            Salt = BigIntegerExtensions.CreateBigInteger(saltBits, new Random());
            Scrambler = BigIntegerExtensions.CreateBigInteger(scramblerBits, new Random());
            SaltedIdentityHash = Salt.CreateSaltedIdentityHash(identityHash);
            Verifier = Generator.ModPow(SaltedIdentityHash, Modulus);

            // 128 бит
            PrivateKey = BigIntegerExtensions.GeneratePseudoPrime(128, 100, new Random());

            // kv + g^b  (mod N)
            PublicKey = Multiplier.Multiply(Verifier).Add(Generator.ModPow(PrivateKey, Modulus));
        }

        //Клиент без соли
        public Srp6(byte[] identityHash, String modulus, int generator,
            String salt, string initialVector = "OFRna73m*aze01xY")
            : this(initialVector, modulus, generator, identityHash)
        {
            IsServerInstance = false;
            Salt = BigIntegerExtensions.CreateBigInteger(salt, 16);
            SaltedIdentityHash = Salt.CreateSaltedIdentityHash(identityHash);                
            PrivateKey = BigIntegerExtensions.GeneratePseudoPrime(128, 100, new Random());
            // g^a (mod N)
            PublicKey = Generator.ModPow(PrivateKey, Modulus);
        }

        public void SetSessionKey(String pubKeyString, String scrambler = null)
        {
            BigInteger pubKey = BigIntegerExtensions.CreateBigInteger(pubKeyString, 16);
                // (Av^u) ^ b (mod N)
            if(IsServerInstance)
                SessionKey = pubKey.Multiply(Verifier.ModPow(Scrambler, Modulus)).ModPow(PrivateKey, Modulus);
            else                        // Client SessionKey
            {
                Scrambler = BigIntegerExtensions.CreateBigInteger(scrambler, 16);
                BigInteger temp = PrivateKey.Add(Scrambler.Multiply(SaltedIdentityHash));
                SessionKey = pubKey.Subtract((Generator.ModPow(SaltedIdentityHash, Modulus))
                                             .Multiply(Multiplier)).ModPow(temp, Modulus);
            }
        }

        public string Encrypt(string plainText)
        {
            if(string.IsNullOrEmpty(plainText))
                return "";
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherTextBytes = Encrypt(plainTextBytes);
            return Convert.ToBase64String(cipherTextBytes);
        }

        public Stream Encrypt(Stream stream)
        {
            var results = new byte[stream.Length];
            stream.Read(results, 0, results.Length);
            byte[] encryptedBytes = Encrypt(results);
            return new MemoryStream(encryptedBytes, false);
        }

        public byte[] Encrypt(byte[] plainTextBytes)
        {
            string password = SessionKey.ToHexString();
            string salt = Salt.ToHexString();
            if(plainTextBytes == null)
                return null;
            byte[] initialVectorBytes = Encoding.ASCII.GetBytes(InitialVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(salt);
            var derivedPassword = new PasswordDeriveBytes(password, saltValueBytes, HashAlgorithm, 2);
            byte[] keyBytes = derivedPassword.GetBytes(KeySize / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            byte[] cipherTextBytes;
            using(var encryptor = symmetricKey.CreateEncryptor(keyBytes, initialVectorBytes))
            {
                using(var memStream = new MemoryStream())
                {
                    using(var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        cipherTextBytes = memStream.ToArray();
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }
            symmetricKey.Clear();
            return cipherTextBytes;
        }
        
        public string Decrypt(string cipherText)
        {
            if(string.IsNullOrEmpty(cipherText))
                return "";
            int byteCount;
            byte[] plainTextBytes = Decrypt(Convert.FromBase64String(cipherText), out byteCount);
            return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
        }

        public Stream Decrypt(Stream stream)
        {
            var results = new byte[stream.Length];
            stream.Read(results, 0, results.Length);
            byte[] decryptedBytes = Decrypt(results);
            return new MemoryStream(decryptedBytes, false);
        }

        public byte[] Decrypt(byte[] cipherTextBytes)
        {
            if(cipherTextBytes == null)
                return null;
            int byteCount;
            byte[] decryptedArray = Decrypt(cipherTextBytes, out byteCount);
            return decryptedArray.SubArray(0, byteCount);
        }

        private byte[] Decrypt(byte[] cipherTextBytes, out int byteCount)
        {
            string password = SessionKey.ToHexString();
            string salt = Salt.ToHexString();
            if(cipherTextBytes == null)
            {
                byteCount = 0;
                return null;
            }
            byte[] initialVectorBytes = Encoding.ASCII.GetBytes(InitialVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(salt);
            var derivedPassword = new PasswordDeriveBytes(password, saltValueBytes, HashAlgorithm, PasswordIterations);
            var keyBytes = derivedPassword.GetBytes(KeySize / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var plainTextBytes = new byte[cipherTextBytes.Length];
            using(var decryptor = symmetricKey.CreateDecryptor(keyBytes, initialVectorBytes))
            {
                using(var memStream = new MemoryStream(cipherTextBytes))
                {
                    using(var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read))
                    {

                        byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }
            symmetricKey.Clear();
            return plainTextBytes;
        }
    }
}

