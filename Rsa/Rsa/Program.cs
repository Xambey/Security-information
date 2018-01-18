using System;
using System.Text;

namespace RsaEncryption
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            //Я устал + мне лень, так что клиента не будет Kappa
            Console.OutputEncoding = Encoding.Unicode;
            
            var primeGen = new Primes();
            var key = primeGen.GetKey();
            const string message = "Спать хочу!!!";
            Console.WriteLine("Источник: \"" + message + "\"");
            var cypherText = key.Encrypt(message);
            Console.Write("Зашифрованный текст: ");
            var isFirstLetter = true;
            foreach (var place in cypherText)
            {
                if (isFirstLetter)
                {
                    isFirstLetter = false;
                    Console.Write(place);
                    continue;
                }
                Console.Write(", " + place);
            }
            Console.WriteLine();
            var decryptedText = key.Decrypt(cypherText);
            Console.WriteLine("Расшифрованный текст: \"" + decryptedText + "\"");
            Console.ReadLine();
        }
    }
}
