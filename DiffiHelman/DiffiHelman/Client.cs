using System;
using System.Dynamic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace DiffiHelman
{
    public class Client
    {
        public int P { get; set; } = 0;
        public int G { get; set; } = 0;
        public int PubCommonKey { get; set; }
        public int OtherPubKey { get; set; }
        private int SecretKey { get; set; }
        private int PrivateKey { get; set; }
        public string Name { get; }

        public Client(string name)
        {
            Name = name;
        }
        
        public Client(string name, int pubOtherKey, int p, int g)
        {
            Name = name;
            OtherPubKey = pubOtherKey;
            P = p;
            G = g;
            Console.WriteLine($"{Name}: получил числа P, G и чужой ключ");
        }

        public void SetOtherPubKey(int key)
        {
            OtherPubKey = key;
            Console.WriteLine($"{Name}: получил чужой ключ");
        }

        public void GeneratePrivateKey()
        {
            PrivateKey = NumberGenerator.NaturalNumber();
            Console.WriteLine($"{Name}: сгенерировал приватный ключ {PrivateKey}");
        }

        public void GeneratePublicKey()
        {
            PubCommonKey = Convert.ToInt32(Math.Pow(G, PrivateKey)) % P;
        }

        public void GenerateSecretKey()
        {
            SecretKey = Convert.ToInt32(Math.Pow(OtherPubKey, PrivateKey)) % P;
            Console.WriteLine($"{Name}: сгенерировал секретный ключ {SecretKey}");
        }

        public void GeneratePandG()
        {
            long? g = null;
            do
            {
                P = NumberGenerator.SimpleNumber();
                if(!NumberGenerator.IsPrime((P-1)/2))
                    continue;
                g = NumberGenerator.GetRoot(P);
            }while(g == null);
            G = (int)g;
            Console.WriteLine($"{Name}: сгенерировал P = {P} и G = {G}");
        }

        public string CryptCalculate(string message)
        {
            var crypt = string.Concat(message.Select(x => (char) (x ^ SecretKey)));
            Console.WriteLine($"{Name}: транслировал '{message}' в '{crypt}'");
            return crypt;
        }

        public void ReceiveMessage(string message)
        {
            Console.WriteLine($"{Name}: получил сообщение '{CryptCalculate(message)}'");
        }
    }
}