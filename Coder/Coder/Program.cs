using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;


namespace Coder
{
    class Program
    {
        static void Main(string[] args)
        {

            Coder coder = new Coder();

            string path = GetFileName("Введите путь к файлу для шифрования: ");
            coder.AddToLog(path);

            var text = File.ReadAllLines(path, Encoding.UTF8).ToList();

            var encrypted = coder.EncryptReplacementWithCodeWord(text, "лаба");
            var decrypted = coder.DecryptReplacementWithCodeWord(encrypted, "лаба");

            coder.WriteLineToConsoleAndLog($"\nПроцент соответствия: {Math.Round(coder.GetPercentCompliance(text, decrypted, true))}%");
            var hacked = coder.SimpleFrequencyHack(encrypted);
            coder.WriteLineToConsoleAndLog($"\nПроцент соответствия(simple) {Math.Round(coder.GetPercentCompliance(text, hacked, true))}%");
            //hacked = coder.HardFrequencyHack(encrypted);
            //coder.WriteLineToConsoleAndLog($"\nПроцент соответствия(hard) {Math.Round(coder.GetPercentCompliance(text, hacked, true))}%");

            var t = coder.EncryptOneTimeNotepad(text);
            decrypted = coder.DecryptOneTimeNotepad(t.Item1, t.Item2);
            coder.WriteLineToConsoleAndLog($"\nПроцент соответствия: {Math.Round(coder.GetPercentCompliance(text, decrypted, true))}%");

            hacked = coder.SimpleFrequencyHack(t.Item1);
            coder.WriteLineToConsoleAndLog($"\nПроцент соответствия: {Math.Round(coder.GetPercentCompliance(text, hacked, true))}%");

            coder.SaveLog("../../");
        }

        private static string GetFileName(string message)
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
