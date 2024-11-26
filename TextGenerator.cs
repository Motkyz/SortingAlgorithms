using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingAlgorithms
{
    public class TextGenerator
    {
        private static Random random = new Random();
        public static string GenerateText(int count)
        {
            StringBuilder text = new StringBuilder();
            string alf = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
            
            for (int i = 0; i < count; i++) //count - количество слов
            {
                long wordLen = random.Next(0, 15);
                for (int j = 0; j < wordLen; j++) //wordLen - длина слова
                {
                    text.Append(alf[random.Next(0, alf.Length)]);
                }
                text.Append(' ');
            }

            return text.ToString();
        }
    }
}
