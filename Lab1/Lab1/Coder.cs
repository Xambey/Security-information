using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
            Console.WriteLine("\n Запуск шифрования");

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
            
            foreach(var item in text)
            {
                var str = "";
                for (int i = 0; i < item.Length; i++)
                {
                    if (alphabetsMatch.ContainsKey(item[i]) && this.IsLetter(item[i]))
                    {
                        str += alphabetsMatch[item[i]];
                    }
                    else
                        str += item[i];
                }
                encrypted.Add(str);
            }

            

            Console.WriteLine("\nЗашифрованый текст: ");
            for(int i = 0; i < encrypted.Count; i++)
            {
                Console.WriteLine(encrypted[i]);
            }

            return encrypted;
        }

        public List<string> FrequencyHack(List<string> text)
        {
            Console.WriteLine("\n Запуск атаки через частотный анализ");

            if (text.Count() == 0)
                return null;

            var frencrypt = this.GetFrequencyTable(text, true).OrderBy(a => a.Value).ToDictionary(a => a.Key, a => a.Value);

            var decrypted = new List<string>();

            foreach (var item in text)
            {
                var str = "";
                for (int i = 0; i < item.Length; i++)
                {

                    if (frencrypt.ContainsKey(item[i]) && this.IsLetter(item[i]))
                    {
                        double val = frencrypt[item[i]];
                        for (int j = 0; j < this._commonFrequencyTable.Count - 1; j++)
                        {
                            var left = this._commonFrequencyTable.ElementAtOrDefault(j);
                            var right = this._commonFrequencyTable.ElementAtOrDefault(j + 1);
                            if (left.Value < val && right.Value > val)
                            {
                                str += ((double)(val - left.Value) < (double)(right.Value - val) ? left.Key : right.Key);
                                break;
                            }
                        }
                    }
                    else
                        str += item[i];
                }
                decrypted.Add(str);
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

            var textACopy = textA.ToList();
            var textBCopy = textB.ToList();

            if(ignoreSpecialSymbols)
            {
                textACopy.ForEach(a => a = string.Concat(a.Where(b => this.IsLetter(b))));
                textBCopy.ForEach(a => a = string.Concat(a.Where(b => this.IsLetter(b))));
            }

            if (textACopy.Count > textBCopy.Count)
                miss = textACopy.Skip(textBCopy.Count - 1).Sum(a => a.Length);
            else if (textACopy.Count < textBCopy.Count)
                miss = textBCopy.Skip(textACopy.Count - 1).Sum(a => a.Length);
            else
                miss = 0;

            for(int i = 0; i < textACopy.Count && i < textBCopy.Count; i++)
            {
                var left = textACopy[i];
                var right = textBCopy[i];

                if (left.Length > right.Length)
                    miss += left.Length - right.Length;
                else if (left.Length < right.Length)
                    miss += right.Length - left.Length;

                for (int j = 0; j < left.Length && j < right.Length; j++)
                {
                    if (left[j] != right[j])
                        miss++;
                }
            }
            if (miss == 0)
                return 100;

            return (double)miss * 100 / Math.Max(textACopy.Count, textBCopy.Count);
        }


        /// <summary>
        /// Дешифрует файл, зашифрованный методом простой замены по ключу
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> Decrypt(List<string> text, string key)
        {
            Console.WriteLine("\n Запуск дешифровки");

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

            for(int i = 0; i < text.Count; i++)
            {
                var current = text[i];
                var buf = "";
                for (int j = 0; j < current.Length; j++) {
                    var item = alphabetsMatch.FirstOrDefault(a => a.Value == current[j]);
                    if (this.IsLetter(current[j]) && !item.Equals(default(KeyValuePair<char, char>)))
                        buf += item.Key;
                    else
                        buf += current[j];
                }
                result.Add(buf);
            }

            Console.WriteLine("\nРасшифрованный текст: ");
            for (int i = 0; i < result.Count; i++)
            {
                Console.WriteLine(result[i]);
            }

            return result;
        }


        private bool IsLetter(char c)
        {
            return ('A' <= c && 'z' >= c) || ('А' <= c && 'я' >= c);
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
                        if (!this.IsLetter(str[j]))
                            continue;
                        if (count.ContainsKey(str[j]))
                            count[str[j]]++;
                        else
                            count.Add(str[j], 0);
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
                            count.Add(str[j], 0);
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
