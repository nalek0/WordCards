using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WordCards.Model
{
    internal class WordCardPointsComparator : IComparer<WordCard>
    {
        // Singleton:
        private static WordCardPointsComparator instance;

        public static WordCardPointsComparator getInstance()
        {
            if (instance == null)
                instance = new WordCardPointsComparator();
            return instance;
        }

        // IComparer:
        public int Compare(WordCard x, WordCard y)
        {
            return x.Points.CompareTo(y.Points);
        }
    }

    internal enum CardStatus
    {
        Normal, Reversed
    }

    internal class Definition
    {
        public string definition{ get; set; }
        public string example { get; set; }
        public List<string> antonyms { get; set; }
        public List<string> synonyms { get; set; }

        public override string ToString()
        {
            return definition;
        }
    }

    internal class Meaning
    {
        public string partOfSpeech { get; set; }
        public List<Definition> definitions { get; set; }
    }

    internal class DictionaryApiResponceDisiarizer
    {
        public List<Meaning> meanings { get; set; }
        public string origin { get; set; }
        public string phonetic { get; set; }
        public string word { get; set; }
    }

    internal class WordCard
    {
        public static int DefaultPoints = 5;

        // Properties:
        public string Word { get; set; }
        public int Points { get; set; }
        public CardStatus Status { get; set; }
        
        public Task<List<DictionaryApiResponceDisiarizer>> ApiResults { get;  set; }

        // Constructors:
        public WordCard(string word)
        {
            Word = word;
            Status = CardStatus.Normal;
            Points = DefaultPoints;
            ApiResults = LoadTranslation(word);
        }

        public WordCard(string word, int points) : this(word)
        {
            Points = points;
        }

        // Methods:

        static Task<List<DictionaryApiResponceDisiarizer>> LoadTranslation(string word)
        {
            return Task.Run(() =>
            {
                WebRequest request = WebRequest.Create("https://api.dictionaryapi.dev/api/v2/entries/en/" + word);

                WebResponse response = request.GetResponse();

                if (((HttpWebResponse)response).StatusCode != HttpStatusCode.OK)
                    return null;

                List<DictionaryApiResponceDisiarizer> DisiarizedResponce;
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    DisiarizedResponce =
                        JsonConvert.DeserializeObject<List<DictionaryApiResponceDisiarizer>>(responseFromServer);
                }

                response.Close();

                return DisiarizedResponce;
            });
        }
    }
}
