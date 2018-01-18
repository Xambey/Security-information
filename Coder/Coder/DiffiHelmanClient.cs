using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Coder
{
    class DiffiHelmanClient
    {
        private static Random generator = new Random();
        public string Name { get; }
        private int p = -1;
        private int g = -1;
        private int key = -1;
        private int commonKey = -1;
        private int secretKey = -1;

        //Ключи клиентов в сети, по имени
        private int otherPublicKey = -1;

        //пулл клиентов
        private static Dictionary<string, DiffiHelmanClient> poolClients = new Dictionary<string, DiffiHelmanClient>();

        //Создает именованный клиент
        public DiffiHelmanClient(string name)
        {
            this.Name = name;
            bool error = false;
            do
            {
                try
                {
                    if (poolClients.ContainsKey(this.Name))
                        throw new ClientCreationFailException(this.Name, "Попытка создать клиент с уже существующем именем");
                    poolClients.Add(this.Name, this);
                    error = false;
                }
                catch (ClientCreationFailException e)
                {
                    Console.WriteLine($"Имя: {e.Name}." + e.Message + "\nВведите другое имя: ");
                    this.Name = Console.ReadLine();
                    error = true;
                }
            } while (error);
        }

        public long GeneratePublicKey()
        {
            if(this.p == -1)
                throw new ClientCreationFailException(this.Name, "Попытка сгенерировать открытый ключ без сгенерированного простого числа p");
            if (this.g == -1)
                throw new ClientCreationFailException(this.Name, "Попытка сгенерировать открытый ключ без сгенерированного простого числа g");
            
            
            //генерируем публичный ключ
            do
            {
                //генерируем личный ключ
                this.key = generator.Next(3, 100);
                this.commonKey = (int)(Math.Pow(this.g, this.key)) % this.p;
            } while (this.commonKey == 1 && this.commonKey == 0);
            return this.commonKey;
        }

        public long GenerateNumberP()
        {
            //do
            //{
            this.p = GeneratePrimeNumber(1000);
            //} while (!IsSimpleNumber((((int)p - 1) / 2), 2000, 5));

            return this.p;
        }

        
        private static int GeneratePrimeNumber(int limit)
        {

            int nechet;
            do
            {
                do
                {
                    nechet = generator.Next(3, limit);
                } while (!is_prime(nechet));
            } while (!is_prime((nechet - 1) / 2));
            return nechet;
        }
        
        static bool is_prime(long n)
        {
            if (n < 0)
                return false;
            for( long i = 3; i * i <= n; ++i)
                if((n % i) == 0) return false;
            return true;
        }
        
        /// <summary>
        /// Возвращает случайное простое число, сгенерированное при помощи метода тестирования пустоты, слишком долгий 
        /// </summary>
        /// <param name="limit">Верхняя граница для проверки</param>
        /// <param name="k">Кол-во проверок</param>
        /// <returns></returns>
        private static int GeneratePrimeByVoid(int limit, int k)
        {
            int number;

            do {
                number = generator.Next(3, 128);//int.MaxValue);
            } while(!IsPrime(number, limit, k));
            return number;
        }
        //Метод тестирования простоты
        private static bool IsPrime(int number,int limit, int k)
        {
            bool firstCondition = true; // условие делимости

            firstCondition = true;
            for (int i = 3; i < limit; i += 2)
            {
                if (number % i == 0)
                {
                    firstCondition = false;
                    break;
                }
            }
            if (!firstCondition || !MillerRabinTest(number, k))
                return false;

            return true;
        }

        private static bool MillerRabinTest(int n, int k)
        {
            //Представить n − 1 в виде 2s·t, где t нечётно, можно сделать последовательным делением n - 1 на 2.
            //цикл А: повторить k раз:
            //                Выбрать случайное целое число a в отрезке[2, n − 2]
            //   x ← a^t mod n, вычисляется с помощью алгоритма возведения в степень по модулю
            //   если x = 1 или x = n − 1, то перейти на следующую итерацию цикла А
            //   цикл B: повторить s − 1 раз
            //      x ← x2 mod n
            //      если x = 1, то вернуть составное
            //      если x = n − 1, то перейти на следующую итерацию цикла A
            //   вернуть составное
            //вернуть вероятно простое

            if (n <= 1)
                return false;
            if (n == 2)
                return true;
            if (n % 2 == 0)
                return false;
            int s = 0, d = n - 1;
            while (d % 2 == 0)
            {
                d /= 2;
                s++;
            }

            for (int i = 0; i < k; i++)
            {
                long a = generator.Next(2, n - 1);
                int x = (int)System.Numerics.BigInteger.ModPow(a, d, n);
                if (x == 1 || x == n - 1)
                    continue;
                for (int j = 0; j < s - 1; j++)
                {
                    x = (x * x) % n;
                    if (x == 1)
                        return false;
                    if (x == n - 1)
                        break;
                }
                if (x != n - 1)
                    return false;
            }
            return true;
        }

        public long GenerateNumberG()
        {
            if (this.p == -1)
                throw new ClientCreationFailException(this.Name, "Невозможно сгенерировать случайное число g, т.к не сгенерировано число p");

            this.g = GetPRoot(this.p) ?? -1;
            if(this.g == -1)
                throw new Exception("Невозможно получить корень p, требуются другие ключи");
            return this.g;
        }

        private static int? GetPRoot(int p)
        {
            for (int i = 0; i < p; i++)
                if (IsPRoot(p, i))
                    return i;
            return null;
        }

        private static bool IsPRoot(long p, long a)
        { 
            if (a == 0 || a == 1)
                return false;
            long last = 1;
            var set = new HashSet<long>();
            for (long i = 0; i < p - 1; i++)
            {
                last = (last * a) % p;
                if (set.Contains(last)) // Если повтор
                    return false;
                set.Add(last);
            }
            return true;
        }

        public void TryCalculateSecretKey()
        {
            if (this.key == -1)
                this.key = generator.Next(3, 100); //throw new ClientCreationFailException(this.Name, "Попытка сгенерировать общий ключ, не сгенерирован закрытый ключ");
            if (this.p == -1)
                throw new ClientCreationFailException(this.Name, "Попытка сгенерировать общий ключ, не установлено p");
            if(this.otherPublicKey == -1)
                throw new ClientCreationFailException(this.Name, "Немозможно вычислить секретный ключ");
            this.secretKey = (int)Math.Pow(this.otherPublicKey, this.key) % this.p;
        }

        public void ReadMessage(string message)
        {
            var name = message.Substring(0, message.IndexOf(':'));
            
            if (message.Contains(" g="))
                this.g = int.Parse(Regex.Match(message, @" g=(?<g>\w+)").Groups["g"].Value);
            if (message.Contains(" p="))
                this.p = int.Parse(Regex.Match(message, @" p=(?<p>\w+)").Groups["p"].Value);
            if(message.Contains(" key="))
            {
                this.otherPublicKey = int.Parse(Regex.Match(message, @" key=(?<key>[0-9-]+)").Groups["key"].Value);
                TryCalculateSecretKey();
            }
                
            var mes = this.CryptMessage(message);   
            Console.WriteLine($"{Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(mes)))}");
        }

        private string CryptMessage(string mes)
        {
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < mes.Length; i++)
            {
                if (char.IsLetter(mes[i]))
                    builder.Append((char)(this.secretKey ^ mes[i]));
                else
                    builder.Append(mes[i]);
            }
            return builder.ToString();
        }
      
        public void SendBroadcastMessage(string name, string message, bool crypt = false)
        {
            var mes = crypt ? this.CryptMessage($"{name}: {message}") : $"{name}: {message}";
            Console.WriteLine($"send {name}: {message}");
            foreach (var item in poolClients)
            {
                if(item.Value != this)
                    item.Value.ReadMessage(mes);
            }
        }
    }
}
