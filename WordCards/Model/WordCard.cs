using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCards.Model
{
    internal enum CardStatus
    {
        Normal, Reversed
    }
    internal class WordCard
    {
        public string EnglishTranslation { get; set; }
        public string RussianTranslation { get; set; }
        public CardStatus Status { get; set; }

        public WordCard(string eng, string rus)
        {
            EnglishTranslation = eng;
            RussianTranslation = rus;
            Status = CardStatus.Normal;
        }

        public override string ToString()
        {
            if (Status == CardStatus.Normal)
            {
                return EnglishTranslation;
            }
            else
            {
                return RussianTranslation;
            }
        }
    }
}
