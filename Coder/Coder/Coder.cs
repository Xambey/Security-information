using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;

namespace Coder
{
    class Coder
    { 

        private Dictionary<char, double> _commonFrequencyTable;
        private Dictionary<string, double> _commonBigrameFrequencyTable;
        private Dictionary<string, double> _commonThreegrameFrequencyTable;
        private Dictionary<string, double> _commonFourgrameFrequencyTable;

        private NormalRandom rand;
        private StringBuilder logger;

        /// <summary>
        /// создает кодировщик
        /// </summary>
        public Coder()
        {
            logger = new StringBuilder();
            this.LoadFavoriteFrequencyTable("..//..//tolstoy.txt");
        }


        /// <summary>
        /// шифрует текст методом замены с использованием кодового слова
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> EncryptReplacementWithCodeWord(List<string> text, string key)
        {
            WriteLineToConsoleAndLog("\nЗапуск шифрования методом простой замены с использованием кодового слова");

            if (text.Count() == 0)
                return null;
            var alphabet = "";
            for (var i = 'А'; i <= 'я'; i++)
                alphabet += i;


            WriteLineToConsoleAndLog("ключ для шифрования: " + key);

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

            return encrypted;
        }


        /// <summary>
        /// Взлом шифра методом частотного анализа
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<string> SimpleFrequencyHack(List<string> text)
        {
            WriteLineToConsoleAndLog("\nЗапуск атаки через Простой частотный анализ");

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

            WriteLineToConsoleAndLog("\nРасшифрованный текст: ");
            for (int i = 0; i < decrypted.Count; i++)
            {
                WriteLineToConsoleAndLog(decrypted[i]);
            }

            return decrypted;
        }

        public List<string> HardFrequencyHack(List<string> text)
        {
            var alphabetBigrams = new Dictionary<string, string>();

            //сбор gramm зашифрованного текста
            var bigrams = this.GetFrequencyGramsTable(text, Grame.Two).OrderBy(a => a.Value).ToDictionary(a=> a.Key, a=> a.Value);
            var threegrams = this.GetFrequencyGramsTable(text, Grame.Three).OrderBy(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var fourgrams = this.GetFrequencyGramsTable(text, Grame.Four).OrderBy(a => a.Value).ToDictionary(a => a.Key, a => a.Value);

            //заполнение алфавита биграммами
            for (int i = 0; i < bigrams.Count; i++)
            {
                for(int j = 0; j < this._commonBigrameFrequencyTable.Count - 1; j++)
                {
                    var bi = bigrams.ElementAt(i);
                    var left = this._commonBigrameFrequencyTable.ElementAt(j);
                    var right = this._commonBigrameFrequencyTable.ElementAt(j + 1);
                    if (left.Value <= bi.Value && bi.Value <= right.Value) {
                        var a = bi.Value - left.Value;
                        var b = right.Value - bi.Value;
                        alphabetBigrams.Add(bi.Key, a < b ? left.Key : right.Key);  
                    }
                }
            }

            var alphabetThreegrams = new Dictionary<string, string>();
            //заполнение алфавита триграммами
            for (int i = 0; i < threegrams.Count; i++)
            {
                for (int j = 0; j < this._commonThreegrameFrequencyTable.Count - 1; j++)
                {
                    var bi = threegrams.ElementAt(i);
                    var left = this._commonThreegrameFrequencyTable.ElementAt(j);
                    var right = this._commonThreegrameFrequencyTable.ElementAt(j + 1);
                    if (left.Value <= bi.Value && bi.Value <= right.Value)
                    {
                        var a = bi.Value - left.Value;
                        var b = right.Value - bi.Value;
                        alphabetThreegrams.Add(bi.Key, a < b ? left.Key : right.Key);
                    }
                }
            }

            var alphabetFourgrams = new Dictionary<string, string>();
            //заполнение алфавита четырехграмм
            for (int i = 0; i < fourgrams.Count; i++)
            {
                for (int j = 0; j < this._commonFourgrameFrequencyTable.Count - 1; j++)
                {
                    var bi = fourgrams.ElementAt(i);
                    var left = this._commonFourgrameFrequencyTable.ElementAt(j);
                    var right = this._commonFourgrameFrequencyTable.ElementAt(j + 1);
                    if (left.Value <= bi.Value && bi.Value <= right.Value)
                    {
                        var a = bi.Value - left.Value;
                        var b = right.Value - bi.Value;
                        alphabetFourgrams.Add(bi.Key, a < b ? left.Key : right.Key);
                    }
                }
            }

            

            var words = new List<string>();
            var decryptedWords = new List<string>();

            //составляем список строк
            for(int i = 0; i < text.Count; i++)
            {
                var row = Regex.Matches(text[i], $"[A-Za-zА-Яа-я]+").OfType<Match>().Select(m => m.Groups[0].Value).ToArray();
                foreach (var item in row)
                {
                    if (!words.Contains(item))
                        words.Add(item);
                }
            }

            var buf = new StringBuilder();

            //Дешифровка
            foreach (var item in words)
            {
                if (item.Length > 2)
                {
                    int n = item.Length / 2;
                    int i = 0;
                    buf.Clear();
                    for (int j = 0; j < n; j++, i++)
                    {
                        var t = item[i].ToString() + item[i + 1];
                        if (alphabetBigrams.ContainsKey(t))
                            buf.Append(alphabetBigrams[t]);
                        else
                            buf.Append(item[i] + item[i + 1]);
                    }
                    decryptedWords.Add(buf.ToString());
                }
                else
                    decryptedWords.Add(item);
            }

            var result = new List<string>(text);

            for(int i = 0; i < words.Count; i++)
            {
                for(int j = 0; j < text.Count; j++)
                {
                    result[j] = text[j].Replace(words[i], decryptedWords[i]);
                }
            }

            return result;
        }


        /// <summary>
        /// Возвращает процент совпадения двух текстов
        /// </summary>
        /// <param name="textA"></param>
        /// <param name="textB"></param>
        /// <param name="ignoreSpecialSymbols">учитывать ли спец. символы</param>
        /// <returns></returns>
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
                        if (!Char.Equals(left[j], right[j]) && Char.IsLetter(left[j]) && Char.IsLetter(right[j]))
                            miss++;
                    }
                else 
                    for (int j = 0; j < left.Length && j < right.Length; j++)
                    {
                        if (!Char.Equals(left[j], right[j]))
                            miss++;
                    }
            }
            if (miss == 0)
                return 100;
            int total = ignoreSpecialSymbols ? Math.Max(textA.Sum(a => a.Sum(b => Char.IsLetter(b) ? 1 : 0)), textB.Sum(a => a.Sum(b => Char.IsLetter(b) ? 1 : 0))) : Math.Max(textA.Sum(a => a.Length), textB.Sum(a => a.Length));

            return 100 - (double)miss * 100 / total;
        }


        /// <summary>
        /// Дешифрует файл, зашифрованный методом простой замены по ключу
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> DecryptReplacementWithCodeWord(List<string> text, string key)
        {
            WriteLineToConsoleAndLog("\nЗапуск дешифровки по ключу");

            if (text.Count() == 0)
                return null;

            var alphabet = "";
            for (var i = 'А'; i <= 'я'; i++)
                alphabet += i;


            WriteLineToConsoleAndLog("ключ для дешифрования: " + key);

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

            return result;
        }

        private Dictionary<string, double> GetFrequencyGramsTable(IList<string> text, Grame gram)
        {
            Dictionary<string, double> res = new Dictionary<string, double>();

            switch (gram)
            {
                case Grame.Two:
                    for (int i = 0; i < text.Count; i++)
                    {
                        var row = text[i];
                        for (int j = 0; j < row.Length - 1; j++)
                        {
                            if (Char.IsLetter(row[j]) && Char.IsLetter(row[j + 1]))
                            {
                                string key = row[j].ToString() + row[j + 1];
                                var obj = res.FirstOrDefault(a => a.Key == key);
                                if (Object.Equals(obj, default(KeyValuePair<string, double>)))
                                    res.Add(key, 1);
                                else
                                    res[key]++;
                            }

                        }
                    }
                    break;
                case Grame.Three:
                    for (int i = 0; i < text.Count; i++)
                    {
                        var row = text[i];
                        for (int j = 0; j < row.Length - 2; j++)
                        {
                            if (Char.IsLetter(row[j]) && Char.IsLetter(row[j + 1]) && Char.IsLetter(row[j+2]))
                            {
                                string key = row[j].ToString() + row[j + 1] + row[j + 2];
                                var obj = res.FirstOrDefault(a => a.Key == key);
                                if (Object.Equals(obj, default(KeyValuePair<string, double>)))
                                    res.Add(key, 1);
                                else
                                    res[key]++;
                            }

                        }
                    }
                    break;
                case Grame.Four:
                    for (int i = 0; i < text.Count; i++)
                    {
                        var row = text[i];
                        for (int j = 0; j < row.Length - 3; j++)
                        {
                            if (Char.IsLetter(row[j]) && Char.IsLetter(row[j + 1]) && Char.IsLetter(row[j + 2]) && Char.IsLetter(row[j + 3]))
                            {
                                string key = row[j].ToString() + row[j + 1] + row[j + 2] + row[j + 3];
                                var obj = res.FirstOrDefault(a => a.Key == key);
                                if (Object.Equals(obj, default(KeyValuePair<string, double>)))
                                    res.Add(key, 1);
                                else
                                    res[key]++;
                            }

                        }
                    }
                    break;
            }

            var count = res.Sum(a => a.Value);

            res = res.ToDictionary(a => a.Key, a => a.Value * 100 / count);
            return res;
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
            var text = File.ReadAllLines(path,Encoding.UTF8).ToList();


            this._commonBigrameFrequencyTable = this.GetFrequencyGramsTable(text, Grame.Two).OrderBy(a => a.Value).ToDictionary(a => a.Key, a => a.Value); 
            this._commonThreegrameFrequencyTable = this.GetFrequencyGramsTable(text, Grame.Three).OrderBy(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            this._commonFourgrameFrequencyTable = this.GetFrequencyGramsTable(text, Grame.Four).OrderBy(a => a.Value).ToDictionary(a => a.Key, a => a.Value);

            this._commonFrequencyTable = this.GetFrequencyTable(text, true).OrderBy(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
        }

        /// <summary>
        /// Шифрует текст методом одноразового блокнота.
        /// В качестве ключа используется случайная последовательность букв (utf8)
        /// </summary>
        /// <param name="text">исходный текст</param>
        /// <returns>Кортеж(зашифрованный текст, ключ)</returns>
        public Tuple<List<string>,List<string>> EncryptOneTimeNotepad(List<string> text)
        {
            WriteLineToConsoleAndLog("\nЗапуск шифрования методом одноразового блокнота");
            if (rand == null)
                rand = new NormalRandom();
            var key = new List<string>();

            StringBuilder buf = new StringBuilder();

            for (int i = 0; i < text.Count; i++)
            {
                for (int j = 0; j < text[i].Length; j++)
                {
                    buf.Append(Encoding.Unicode.GetString(BitConverter.GetBytes(rand.Next())));
                }
                
                key.Add(buf.ToString());
                buf.Clear();
            }

            WriteLineToConsoleAndLog("\nКлюч: ");
            for (int i = 0; i < key.Count; i++)
                WriteLineToConsoleAndLog(key[i]);

            var result = new List<string>(text.Count);
            for (int i = 0; i < text.Count; i++)
            {
                for (int j = 0; j < text[i].Length; j++)
                    buf.Append(BitConverter.ToChar(BitConverter.GetBytes(text[i][j] ^ key[i][j]), 0));
                result.Add(buf.ToString());
                buf.Clear();
            }

            return new Tuple<List<string>, List<string>>(result, key);
        }

        /// <summary>
        /// Дешифрует текст, зашифрованный методом одноразового блокнота
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> DecryptOneTimeNotepad(List<string> text, List<string> key)
        {
            WriteLineToConsoleAndLog("\nЗапуск дешифрования методом одноразового блокнота");
            StringBuilder buf = new StringBuilder();
            var result = new List<string>(text.Count);
            for (int i = 0; i < text.Count; i++)
            {
                for (int j = 0; j < text[i].Length; j++)
                    buf.Append(BitConverter.ToChar(BitConverter.GetBytes(text[i][j] ^ key[i][j]), 0));
                result.Add(buf.ToString());
                buf.Clear();
            }

            WriteLineToConsoleAndLog("\nРасшифрованный текст: ");
            for (int i = 0; i < result.Count; i++)
                WriteLineToConsoleAndLog(result[i]);
            return result;
        }

        /// <summary>
        /// Сохраняет лог кодировщика
        /// </summary>
        /// <param name="path">путь к каталогу </param>
        /// <param name="filename">имя файла</param>
        public void SaveLog(string path, string filename = null)
        {
            Directory.CreateDirectory(path);
            using (var stream = new StreamWriter(File.Create(path + (filename ?? "log") + ".txt"),Encoding.Unicode))
            {
                stream.WriteLine(logger.ToString());
                stream.Flush();
            }
        }

        public void WriteLineToConsoleAndLog(string value)
        {
            Console.WriteLine(value);
            logger.Append(value);
        }

        public void WriteToConsoleAndLog(string value)
        {
            Console.Write(value);
            logger.Append(value);
        }

        public void AddToLog(string value)
        {
            logger.Append(value);
        }

    };
}
