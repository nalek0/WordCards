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

        void ClickedAddWord(object sender, RoutedEventArgs e)
        {
            WordCard newCard;
            try
            {
                string text = NewWord.Text;
                NewWord.Text = "";
                newCard = new WordCard(text);
            }
            catch { return; }

            ListOfCards.AddCard(newCard);
        }

        // Methods:
        void ShowCurrentCard(object sender, EventArgs args)
        {
            MeaningsPanel.Children.Clear();
            if (ListOfCards.IsEmpty())
                WordPanel.Text = "There are no words in database!";
            else
            {
                WordPanel.Text = ListOfCards.CurrentCard.Word;
                if (ListOfCards.CurrentCard.Status == CardStatus.Reversed)
                {
                    foreach (var result in ListOfCards.CurrentCard.ApiResults)
                    {
                        StackPanel sp = new StackPanel() { Orientation = Orientation.Vertical, Style = (Style) Resources["ResultStyle"] };
                        foreach (var meaning in result.meanings)
                        {
                            sp.Children.Add(new TextBlock() { Text = meaning.partOfSpeech, FontWeight = FontWeights.Bold });
                            foreach (var defenition in meaning.definitions)
                                sp.Children.Add(new TextBlock() { Text = defenition.definition, Style = (Style) Resources["ResultDefenitionStyle"] });
                        }
                        MeaningsPanel.Children.Add(sp);
                    }
                }
            }
        }
    }
}
