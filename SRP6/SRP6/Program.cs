using System;
using System.Text;

namespace SRP6
{
    class Program
    {
        private const int Generator = 0x0A;
        private const int SafePrimeLength = 256;
        private const int SaltBitLength = 512;
        private const int ScramblerBitLength = 256;
	    
	    private static string Username = "User";
	    private static string Password = "Passw0rd";

        static void Main(string[] args)
        {

	        string modulus;

			Console.Write("Генерируем ключ безопасности");
			modulus = GenerateSafePrime();
			Console.WriteLine();
			Console.Write($"Ключ безопасности: {modulus}");
	        
            Console.WriteLine(Environment.NewLine);

			int amount = GetIterationCount();
			for (int i = 0; i < amount; i++)
			{
				Username = GenerateRandomString();
				Password = GenerateRandomString();
				if (!Test(modulus))
				{
					Console.WriteLine("Тест #" + (i+1) + " Failed!!!");
					return;
				}
				Console.WriteLine("Тест #" + (i+1) + " Passed.");
			}
			Console.WriteLine("Оно, что не сдохло? LUL !");
	        
	        Console.ReadLine();
        }

		private static ConsoleKey GetKey()
		{
			ConsoleKeyInfo result = Console.ReadKey(true);
			if (result.Key != ConsoleKey.Enter)
				Console.Write(result.KeyChar);
			return result.Key;
		}

        private static int GetIterationCount()
        {
            Console.WriteLine();
            while (true)
			{
                Console.Write("Введите кол-во итераций для теста: ");
                string textAmount = Console.ReadLine();
                try
				{                   
                    int count = Convert.ToInt32(textAmount);
                    Console.WriteLine();
                    return count;
                }
                catch { }
            }
        }

        private static string GenerateRandomString(int length = 128)
        {
            byte[] bytes = new byte[length];
            Random rand = new Random();
            for (int i = 0; i < length; i++)
            {
                byte b = (byte)rand.Next(32, 126);
                if (b == 58)
                    i--;
                else
                    bytes[i] = b;
            }
            return Encoding.ASCII.GetString(bytes);
        }

		private static bool Test(String modulus)
		{
            byte[] identityHash = Encoding.Unicode.GetBytes((Username + ":" + Password)).Sha1Hash();

			Console.WriteLine("Сервер генерирует и отправляет публичный ключ, скрамлер и соль...");
			var server = new Srp6(identityHash, modulus, Generator, SaltBitLength, ScramblerBitLength);

			Console.WriteLine("Клиент генерирует и отправляет серверу публичный ключ...");
			var client = new Srp6(identityHash, modulus, Generator, server.Salt.ToHexString());

			client.SetSessionKey(server.PublicKey.ToHexString(), server.Scrambler.ToHexString());
			Console.WriteLine($"Клиент устанавливает ключ сессии");

			server.SetSessionKey(client.PublicKey.ToHexString());
			Console.WriteLine($"Сервер получает публичный ключ клиента и устанавливаeт ключ сессии");

            const string startingText = "YOU MUST WORK, SUFFER!";
            string encrypted = server.Encrypt(startingText);
            string decrypted = client.Decrypt(encrypted);

			Console.WriteLine(Environment.NewLine);
			Console.WriteLine($"Модуль: {server.Modulus.ToHexString()}");
			Console.WriteLine($"Множитель: {server.Multiplier.ToHexString()}");
			Console.WriteLine($"Генератор: {server.Generator.ToHexString()}");
			Console.WriteLine($"Соль: {server.Salt.ToHexString()}");
			Console.WriteLine($"Хеш идентификации сервера: {server.IdentityHash.ToHexString()}");
			Console.WriteLine($"Верификатор: {server.Verifier.ToHexString()}");
			Console.WriteLine();
			Console.WriteLine($"Приватный ключ сервера(b): {server.PrivateKey.ToHexString()}");
			Console.WriteLine($"Публичный ключ сервера(B): {server.PublicKey.ToHexString()}");
			Console.WriteLine($"Скрембер(u): {server.Scrambler.ToHexString()}");
			Console.WriteLine();
			Console.WriteLine($"Приватный ключ клиента(a): {client.PrivateKey.ToHexString()}");
			Console.WriteLine($"Публичный ключ клиента(A): {client.PublicKey.ToHexString()}");
			Console.WriteLine($"Хеш индетификации клиента(x): {client.IdentityHash.ToHexString()}");
			Console.WriteLine();
			Console.WriteLine($"Сессионный ключ сервера: {server.SessionKey.ToHexString()}");
			Console.WriteLine($"Сессионный ключ клиента: {client.SessionKey.ToHexString()}");
			Console.WriteLine();
			Console.WriteLine($"{startingText}");
			Console.WriteLine($"Зашифрованный текст: {encrypted}");
			Console.WriteLine($"Дешифрованный текст: {decrypted}");
			
			return server.SessionKey.Equals(client.SessionKey);
		}

		private static string GenerateSafePrime()
		{
			var random = new Random();
			var bitInt = BigIntegerExtensions.GenerateSafePrime(SafePrimeLength, 1, random);
 
			while (!bitInt.IsSafePrime(100))
			{
				bitInt = BigIntegerExtensions.GenerateSafePrime(SafePrimeLength, 1, random);
			}
			return bitInt.ToString("X2");
		}

    }
}
