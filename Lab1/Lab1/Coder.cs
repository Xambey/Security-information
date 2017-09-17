﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Lab1
{
    class Coder
    {
        private Dictionary<char, double> _commonFrequencyTable;

        /// <summary>
        /// создает кодировщик
        /// </summary>
        public Coder()
        {
            this.LoadFavoriteFrequencyTable("..//..//tolstoy.txt");
        }


        /// <summary>
        /// шифрует файл методом замены с использованием кодового слова
        /// </summary>
        /// <param name="path">путь к файлу для шифрования</param>
        public List<string> Encrypt(List<string> text, string key)
        {
            Console.WriteLine("\nЗапуск шифрования");

            if (text.Count() == 0)
                return null;
            var alphabet = "";
            for (var i = 'А'; i <= 'я'; i++)
                alphabet += i;


            Console.WriteLine("ключ для шифрования: " + key);

            string keyalphabet = key + string.Concat(alphabet.Where(a => !key.Contains(a)));
            
            var alphabetsMatch = new Dictionary<char, char>();
            for(int i = 0; i < alphabet.Length; i++)
            {
                if (!alphabetsMatch.ContainsKey(alphabet[i]))
                    alphabetsMatch.Add(alphabet[i], keyalphabet[i]);
            }

            List<string> encrypted = new List<string>();
            
            StringBuilder str = new StringBuilder();
            foreach(var item in text)
            {
                str.Clear();
                for (int i = 0; i < item.Length; i++)
                {
                    if (alphabetsMatch.ContainsKey(item[i]) && Char.IsLetter(item[i]))
                    {
                        str.Append(alphabetsMatch[item[i]]);
                    }
                    else
                        str.Append(item[i]);
                }
                encrypted.Add(str.ToString());
            }

            

            //Console.WriteLine("\nЗашифрованый текст: ");
            //for(int i = 0; i < encrypted.Count; i++)
            //{
            //    Console.WriteLine(encrypted[i]);
            //}

            return encrypted;
        }

        public List<string> FrequencyHack(List<string> text)
        {
            Console.WriteLine("\nЗапуск атаки через частотный анализ");

            if (text.Count() == 0)
                return null;

            var frencrypt = this.GetFrequencyTable(text, true).OrderBy(a => a.Value).ToDictionary(a => a.Key, a => a.Value);

            var decrypted = new List<string>();

            var str = new StringBuilder();
            foreach (var item in text)
            {
                str.Clear();
                for (int i = 0; i < item.Length; i++)
                {

                    if (Char.IsLetter(item[i]) && frencrypt.ContainsKey(item[i]))
                    {
                        double val = frencrypt[item[i]];
                        for (int j = 0; j < this._commonFrequencyTable.Count - 1; j++)
                        {
                            var left = this._commonFrequencyTable.ElementAtOrDefault(j);
                            var right = this._commonFrequencyTable.ElementAtOrDefault(j + 1);
                            if (left.Value <= val && right.Value >= val)
                            {
                                str.Append(((double)(val - left.Value) < (double)(right.Value - val) ? left.Key : right.Key));
                                break;
                            }
                        }
                    }
                    else
                        str.Append(item[i]);
                }
                decrypted.Add(str.ToString());
            }

            Console.WriteLine("\nРасшифрованный текст: ");
            for (int i = 0; i < decrypted.Count; i++)
            {
                Console.WriteLine(decrypted[i]);
            }

            return decrypted;
        }


        public double GetPercentCompliance(List<string> textA, List<string> textB, bool ignoreSpecialSymbols = false)
        {
            if (textA.Count == 0 || textB.Count == 0)
                return 0;
            int miss;


            if (textA.Count > textB.Count)
                miss = ignoreSpecialSymbols ? textA.Skip(textB.Count - 1).Sum(a => a.Sum(b => Char.IsLetter(b) ? 1 : 0)) : textA.Skip(textB.Count - 1).Sum(a => a.Length);
            else if (textA.Count < textB.Count)
                miss = ignoreSpecialSymbols ? textB.Skip(textA.Count - 1).Sum(a => a.Sum(b => Char.IsLetter(b) ? 1 : 0)) : textB.Skip(textA.Count - 1).Sum(a => a.Length);
            else
                miss = 0;

            for(int i = 0; i < textA.Count && i < textB.Count; i++)
            {
                var left = textA[i];
                var right = textB[i];

                if (left.Length > right.Length)
                    miss += left.Length - right.Length;
                else if (left.Length < right.Length)
                    miss += right.Length - left.Length;
                if(ignoreSpecialSymbols)
                    for (int j = 0; j < left.Length && j < right.Length; j++)
                    {
                        if (left[j] != right[j] && Char.IsLetter(left[j]) && Char.IsLetter(right[j]))
                            miss++;
                    }
                else
                    for (int j = 0; j < left.Length && j < right.Length; j++)
                    {
                        if (left[j] != right[j])
                            miss++;
                    }
            }
            if (miss == 0)
                return 100;
            int total = ignoreSpecialSymbols ? Math.Max(textA.Sum(a => a.Sum(b => Char.IsLetter(b) ? 1 : 0)), textB.Sum(a => a.Sum(b => Char.IsLetter(b) ? 1 : 0))) : Math.Max(textA.Sum(a => a.Length), textB.Sum(a => a.Length));

            return (double)miss * 100 / total;
        }


        /// <summary>
        /// Дешифрует файл, зашифрованный методом простой замены по ключу
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> Decrypt(List<string> text, string key)
        {
            Console.WriteLine("\nЗапуск дешифровки");

            if (text.Count() == 0)
                return null;

            var alphabet = "";
            for (var i = 'А'; i <= 'я'; i++)
                alphabet += i;


            Console.WriteLine("ключ для дешифрования: " + key);

            string keyalphabet = key + string.Concat(alphabet.Where(a => !key.Contains(a)));

            var alphabetsMatch = new Dictionary<char, char>();
            for (int i = 0; i < alphabet.Length; i++)
            {
                if (!alphabetsMatch.ContainsKey(alphabet[i]))
                    alphabetsMatch.Add(alphabet[i], keyalphabet[i]);
            }

            var result = new List<string>();

            var buf = new StringBuilder();
            for(int i = 0; i < text.Count; i++)
            {
                buf.Clear();
                var current = text[i];
                for (int j = 0; j < current.Length; j++) {
                    var item = alphabetsMatch.FirstOrDefault(a => a.Value == current[j]);
                    if (Char.IsLetter(current[j]) && !item.Equals(default(KeyValuePair<char, char>)))
                        buf.Append(item.Key);
                    else
                        buf.Append(current[j]);
                }
                result.Add(buf.ToString());
            }

            //Console.WriteLine("\nРасшифрованный текст: ");
            //for (int i = 0; i < result.Count; i++)
            //{
            //    Console.WriteLine(result[i]);
            //}

            return result;
        }


        /// <summary>
        /// Возвращает частотную таблицу для текста
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ignoreSigns">игнорировать знаки препинания и спец. символы, по умолчанию учитывает все символы</param>
        /// <returns></returns>
        private Dictionary<char, double> GetFrequencyTable(IList<string> text, bool ignoreSigns = false)
        {
            var dic = new Dictionary<char, double>();
            var count = new Dictionary<char, int>();

            if(ignoreSigns)
                for (int i = 0; i < text.Count(); i++)
                {
                    var str = text[i];
                    for (int j = 0; j < str.Length; j++)
                    {
                        if (!Char.IsLetter(str[j]))
                            continue;
                        if (count.ContainsKey(str[j]))
                            count[str[j]]++;
                        else
                            count.Add(str[j], 1);
                    }
                }
            else
                for(int i = 0; i < text.Count(); i++)
                {
                    var str = text[i];
                    for(int j = 0; j < str.Length; j++)
                    {
                        if (count.ContainsKey(str[j]))
                            count[str[j]]++;
                        else
                            count.Add(str[j], 1);
                    }
                }

            var sum = count.Sum(a => a.Value);

            foreach (var item in count)
            {
                dic.Add(item.Key, (double) item.Value / sum);
            }
            return dic;
        }

        /// <summary>
        /// Создает эталонную таблицу частот букв
        /// </summary>
        /// <param name="path">имя файла - эталона </param>
        private void LoadFavoriteFrequencyTable(string path)
        {
            var text = File.ReadAllLines(path,Encoding.UTF8);
            this._commonFrequencyTable = this.GetFrequencyTable(text.ToList(), true).OrderBy(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
        }

    };
}
