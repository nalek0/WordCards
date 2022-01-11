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
        // Events:
        public event EventHandler CurrentCardChanged;

        // Properties:
        int index = 0;
        int CurrentCardIndex {
            get
            {
                if (IsEmpty())
                    return 0;
                return index % Cards.Count;
            } 
            set
            {
                if (IsEmpty())
                    index = 0;
                else
                    index = value;
                if (CurrentCardChanged != null)
                    CurrentCardChanged(this, new EventArgs());
            }
        }
        List<WordCard> Cards { get; set; }
        public WordCard CurrentCard { 
            get
            {
                return Cards[CurrentCardIndex];
            } 
        }

        // Constructors:
        CardsList()
        {
            Cards = new List<WordCard>();
        }

        CardsList(List<WordCard> cards) : this()
        {
            Cards = cards;
        }


        public static async Task<CardsList> Import()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("words.xml", CreationCollisionOption.OpenIfExists);
            Stream input = (await file.OpenReadAsync()).AsStreamForRead();
            XmlTextReader xmlIn = new XmlTextReader(input);

            try
            {
                xmlIn.WhitespaceHandling = WhitespaceHandling.None;

                xmlIn.MoveToContent();

                if (xmlIn.Name != "CardsList")
                    throw new ArgumentException("Incorrect file format.");

                List<WordCard> cards = new List<WordCard>();

                do
                {
                    if (!xmlIn.Read())
                        throw new ArgumentException("Error during reading node.");

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

                xmlIn.Close();
                input.Close();

                return new CardsList(cards);
            }
            catch (Exception ex)
            {
                xmlIn.Close();
                input.Close();

                if (ex is XmlException || ex is ArgumentException)
                    return new CardsList();
                else
                    throw ex;
            }
        }

        // Methods:
        // List methods
        public void AddCard(WordCard card)
        {
            Cards.Add(card);
            UpdateCardsInFile();
            CurrentCardIndex = 0;
        }
        
        public void Shuffle()
        {
            foreach (var card in Cards)
                card.Status = CardStatus.Normal;

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

            CurrentCardIndex = 0;
        }

        public void KnowCurrent()
        {
            if (IsEmpty())
                return;
            CurrentCard.Points--;
            if (CurrentCard.Points < 0)
            {
                Cards.Remove(CurrentCard);
                CurrentCardIndex = CurrentCardIndex;
                UpdateCardsInFile();
            }
            else
                CurrentCardIndex++;
        }

        public void DontKnowCurrent()
        {
            if (IsEmpty())
                return;
            CurrentCard.Points++;
            CurrentCardIndex++;
            UpdateCardsInFile();
        }
        
        public bool IsEmpty()
        {
            return Cards.Count == 0;
        }

        public void ReverseCurrentCard()
        {
            if (IsEmpty())
                return;

            if (CurrentCard.Status == CardStatus.Reversed)
            {
                CurrentCard.Status = CardStatus.Normal;
                CurrentCardIndex++;
            }
            else
            {
                CurrentCard.Status = CardStatus.Reversed;

                if (CurrentCardChanged != null)
                    CurrentCardChanged(null, new EventArgs());
            }
        }

        // Event methods:
        async void UpdateCardsInFile()
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
    }
}
