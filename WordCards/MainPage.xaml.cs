using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using WordCards.Model;

namespace WordCards
{
    public sealed partial class MainPage : Page
    {
        // Properties:
        CardsList ListOfCards;

        // Initialising:
        public MainPage()
        {
            this.InitializeComponent();

            initWindow();
            initCards();
        }

        void initWindow()
        {
            ApplicationView.GetForCurrentView()
                .SetPreferredMinSize(new Size(700, 500));
        }

        async void initCards()
        {
            ListOfCards = await CardsList.Import();
            ListOfCards.CurrentCardChanged += new EventHandler(ShowCurrentCard);
            ListOfCards.Shuffle();
        }

        // XAML Events:
        void ClickedShuffle(object sender, RoutedEventArgs e)
        {
            ListOfCards.Shuffle();
        }

        void ClickedIKnowIt(object sender, RoutedEventArgs e)
        {
            ListOfCards.KnowCurrent();
        }

        void ClickedIDontKnowIt(object sender, RoutedEventArgs e)
        {
            ListOfCards.DontKnowCurrent();
        }

        void ClickedCard(object sender, PointerRoutedEventArgs e)
        {
            ListOfCards.ReverseCurrentCard();
        }

        async void ClickedAddWord(object sender, RoutedEventArgs e)
        {
            WordCard newCard;
            string text = NewWord.Text;
            NewWord.Text = "";
            newCard = new WordCard(text);
            var result = await newCard.ApiResults;
            if (result == null)
                return; // It meanis there was an error during loading

            ListOfCards.AddCard(newCard);
        }

        // Methods:
        async void ShowCurrentCard(object sender, EventArgs args)
        {
            MeaningsPanel.Children.Clear();
            WordPanel.Text = "";
            WordPoints.Text = "";
            WordCounter.Text = "";
            if (ListOfCards.IsEmpty())
                WordPanel.Text = "There are no words in database!";
            else
            {
                WordPanel.Text = ListOfCards.CurrentCard.Word;
                WordPoints.Text = String.Format("({0} points)", ListOfCards.CurrentCard.Points);
                WordCounter.Text = String.Format("{0}/{1}", ListOfCards.CurrentCardIndex + 1, ListOfCards.NumberOfCards);
                
                if (ListOfCards.CurrentCard.Status == CardStatus.Reversed)
                {
                    MeaningsPanel.Children.Add(new TextBlock() { Text = "Word's meaning is loading..." });
                    var apiResult = await ListOfCards.CurrentCard.ApiResults;
                    MeaningsPanel.Children.Clear();

                    foreach (var result in apiResult)
                    {
                        StackPanel sp = new StackPanel() { Orientation = Orientation.Vertical, Style = (Style)Resources["ResultStyle"] };
                        foreach (var meaning in result.meanings)
                        {
                            if (meaning.partOfSpeech != null)
                                sp.Children.Add(new TextBlock() { Text = String.Format("[{0}]", meaning.partOfSpeech), FontWeight = FontWeights.Bold });
                            for (int index  = 0; index < meaning.definitions.Count; index++)
                            {
                                var defenition = meaning.definitions[index];
                                sp.Children.Add(
                                    new TextBlock() { 
                                        Text = String.Format("{0}) {1}", index + 1, defenition.definition), 
                                        Style = (Style) Resources["ResultDefenitionStyle"] 
                                    }
                                );
                                if (defenition.example != null && defenition.example.Length > 0)
                                    sp.Children.Add(
                                        new TextBlock() { 
                                            Text = String.Format("\"{0}\"", defenition.example), 
                                            Style = (Style) Resources["ResultExamleStyle"] 
                                        }
                                    );
                            }
                        }
                        MeaningsPanel.Children.Add(sp);
                    }
                }
            }
        }
    }
}
