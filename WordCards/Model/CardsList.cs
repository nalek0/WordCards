using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using Windows.Storage;

namespace WordCards.Model
{
    internal class CardsList
    {
        private int CurrentCardIndex { get; set; } = 0;
        private List<WordCard> Cards;

        public WordCard CurrentCard { 
            get
            {
                return Cards[CurrentCardIndex];
            } 
        }
        
        private void RotateToNormal()
        {
            foreach (var card in Cards)
            {
                card.Status = CardStatus.Normal;
            }
        }

        public void Shuffle()
        {
            RotateToNormal();

            List<WordCard> listCopy = new List<WordCard>();
            Cards.ForEach(card => listCopy.Add(card));
            listCopy.Sort(WordCardPointsComparator.getInstance());

            Cards = new List<WordCard>();
            Random rnd = new Random();
            while (listCopy.Count != 0)
            {
                int index = (int) Math.Sqrt(rnd.Next(0, listCopy.Count * listCopy.Count));
                Cards.Add(listCopy[index]);
                listCopy.RemoveAt(index);
            }
        }

        public void KnowCurrent()
        {
            CurrentCard.Points--;
            UpdateCards();
        }

        public void DontKnowCurrent()
        {
            CurrentCard.Points++;
            UpdateCards();
        }

        private void UpdateCards()
        {
            Cards = Cards.Where(card => card.Points >= 0).ToList();
            UpdateFile();
        }

        private async void UpdateFile()
        {
            Stream output = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync("words.xml", CreationCollisionOption.ReplaceExisting);
            XmlTextWriter xmlOut = new XmlTextWriter(output, Encoding.Unicode);

            try
            {
                xmlOut.Formatting = Formatting.Indented;

                xmlOut.WriteStartDocument();
                xmlOut.WriteComment("In this file WordsCards application saves words");

                xmlOut.WriteStartElement("CardsList");
                xmlOut.WriteAttributeString("Version", "1");

                foreach (WordCard word in Cards)
                {
                    xmlOut.WriteStartElement("Word");
                    xmlOut.WriteAttributeString("word", word.Word);
                    xmlOut.WriteAttributeString("points", word.Points.ToString());
                    xmlOut.WriteEndElement();
                }
                xmlOut.WriteEndElement();
            }
            finally
            {
                xmlOut.Close();
                output.Close();
            }
        }

        public void Next()
        {
            CurrentCardIndex = (CurrentCardIndex + 1) % Cards.Count;
        }

        public CardsList()
        {
            Cards = new List<WordCard>();
        }

        public CardsList(List<WordCard> cards)
        {
            Cards = cards;
        }

        public void ClearCards()
        {
            Cards.Clear();
        }

        public void AddCard(WordCard card)
        {
            Cards.Add(card);
            UpdateCards();
        }

        public bool Empty()
        {
            return Cards.Count == 0;
        }

        public static async Task<CardsList> Import()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("words.xml", CreationCollisionOption.OpenIfExists);
            Stream input = (await file.OpenReadAsync()).AsStreamForRead();
            XmlTextReader xmlIn = new XmlTextReader(input);

            CardsList result;

            try
            {
                xmlIn.WhitespaceHandling = WhitespaceHandling.None;

                xmlIn.MoveToContent();

                if (xmlIn.Name != "CardsList")
                    throw new ArgumentException("Incorrect file format.");

                List<WordCard> cards = new List<WordCard>();

                string version = xmlIn.GetAttribute(0);
                do
                {
                    if (!xmlIn.Read())
                        throw new ArgumentException("???");

                    if ((xmlIn.NodeType == XmlNodeType.EndElement) && (xmlIn.Name == "CardsList"))
                        break;
                    if (xmlIn.NodeType == XmlNodeType.EndElement)
                        continue;

                    if (xmlIn.Name == "Word")
                    {
                        try
                        {
                            string word = xmlIn.GetAttribute("word");
                            int points = Convert.ToInt32(xmlIn.GetAttribute("points"));
                            cards.Add(new WordCard(word, points));
                        }
                        catch { }
                    }

                } while (!xmlIn.EOF);

                result = new CardsList(cards);
            }
            catch (XmlException)
            {
                result = new CardsList();
            }
            catch (ArgumentException)
            {
                result = new CardsList();
            }
            finally
            {
                xmlIn.Close();
                input.Close();
            }

            return result;
        }
    }
}
