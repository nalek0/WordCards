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
    }

    internal class Meaning
    {
        public string partOfSpeech { get; set; }
        public List<Definition> definitions { get; set; }

        public override string ToString()
        {
            return "[" + partOfSpeech + "] " + String.Join("\n", definitions.ConvertAll(definition => definition.definition));
        }
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

        public string Word { get; set; }
        List<DictionaryApiResponceDisiarizer> ApiResult { get; set; }
        public string Meaning {
            get
            {
                return String.Join(
                    '\n',
                    ApiResult.ConvertAll(
                        element => String.Join(
                            '\n',
                            element.meanings
                                .ConvertAll(meaning => meaning.ToString())
                        )
                    )
                );
            }
        }
        public int Points;
        public CardStatus Status { get; set; }

        private static List<DictionaryApiResponceDisiarizer> LoadTranslation(string word)
        {
            WebRequest request = WebRequest.Create("https://api.dictionaryapi.dev/api/v2/entries/en/" + word);

            WebResponse response = request.GetResponse();

            if (((HttpWebResponse) response).StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Wrong word");
            }

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
        }

        public WordCard(string word)
        {
            Word = word;
            Status = CardStatus.Normal;
            ApiResult = LoadTranslation(word);

            Points = DefaultPoints;
        }

        public WordCard(string word, int points) : this(word)
        {
            Points = points;
        }

        public override string ToString()
        {
            if (Status == CardStatus.Normal)
            {
                return Word;
            }
            else
            {
                return Meaning;
            }
        }
    }
}
