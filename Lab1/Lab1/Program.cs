using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;


namespace Lab1
{
    class Program
    {
        static void Main(string[] args)
        {

            Coder coder = new Coder();

            string path = null;
            while (true)
            {
                Console.Write("Введите путь к файлу для шифрования: ");
                path = Console.ReadLine();

                if (File.Exists(path))
                    break;
                else
                    Console.WriteLine("Файл не найден!");
            }
            var text = File.ReadAllLines(path, Encoding.UTF8);

            var encrypted = coder.Encrypt(text.ToList(), "лаба");
            var decrypted = coder.Decrypt(encrypted, "лаба");
            Console.WriteLine($"\nПроцент соответствия: {coder.GetPercentCompliance(encrypted, decrypted)}%");
            var hacked = coder.FrequencyHack(encrypted);
            Console.WriteLine($"\nПроцент соответствия: {coder.GetPercentCompliance(decrypted, hacked)}%");
        }

        private string GetFileName(string message)
        {
            string path = null;
            while (true)
            {
                Console.Write(message);
                path = Console.ReadLine();

                if (File.Exists(path))
                    return path;
                else
                    Console.WriteLine("Файл не найден!");
            }
        }
    }

}
