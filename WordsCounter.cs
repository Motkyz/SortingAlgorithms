using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingAlgorithms
{
    public class WordsCounter
    {
        public static Dictionary<string, int> CountWords(string[] text)
        {
            Dictionary<string, int> countOfWords = new Dictionary<string, int>();
            foreach (string word in text) 
            {
                if (countOfWords.ContainsKey(word))
                {
                    countOfWords[word]++;
                }
                else
                {
                    countOfWords.Add(word, 1);
                }
            }

            return countOfWords;
        }
    }
}
