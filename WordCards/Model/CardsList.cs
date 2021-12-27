using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

namespace WordCards.Model
{
    internal class CardsList
    {
        int CurrentCardIndex { get; set; }
        List<WordCard> Cards;
        public WordCard CurrentCard { 
            get
            {
                return Cards[CurrentCardIndex];
            } 
        }
        string FilePath;

        private void RotateNormal()
        {
            foreach (var card in Cards)
            {
                card.Status = CardStatus.Normal;
            }
        }

        public void Shuffle()
        {
            RotateNormal();

            List<WordCard> listCopy = new List<WordCard>();
            Cards.ForEach(card => listCopy.Add(card));
            Cards = new List<WordCard>();
            Random rnd = new Random();
            while (listCopy.Count != 0)
            {
                int index = rnd.Next(0, listCopy.Count);
                Cards.Add(listCopy[index]);
                listCopy.RemoveAt(index);
            }
        }

        public WordCard Next()
        {
            CurrentCardIndex = (CurrentCardIndex + 1) % Cards.Count;
            return Cards[CurrentCardIndex];
        }

        public CardsList(List<WordCard> list, string filePath)
        {
            CurrentCardIndex = 0;
            Cards = list;
            FilePath = filePath;
        }

        static public CardsList ParseFromFile(string filePath)
        {
            List<WordCard> resultList = new List<WordCard>();
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                while (reader.Peek() != -1)
                {
                    string[] splittedLine = Regex.Split(reader.ReadLine(), "\\s+");
                    if (splittedLine.Length != 2)
                    {
                        continue;
                    }
                    resultList.Add(new WordCard(splittedLine[0], splittedLine[1]));
                }
            }
            return new CardsList(resultList, filePath);
        }
    }
}
