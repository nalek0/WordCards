using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using WordCards.Model;

namespace WordCards
{
    public sealed partial class MainPage : Page
    {
        CardsList ListOfCards;

        public MainPage()
        {
            this.InitializeComponent();

            initWindow();
            initCards();
        }

        private void initWindow()
        {
            ApplicationView.GetForCurrentView()
                .SetPreferredMinSize(new Size(400, 200));
        }

        private async void initCards()
        {
            ListOfCards = await CardsList.Import();
            ListOfCards.Shuffle();
            ShowCurrentCard();
        }

        private void ClickedShuffle(object sender, RoutedEventArgs e)
        {
            ListOfCards.Shuffle();
            ShowCurrentCard();
        }

        private void ClickedIKnowIt(object sender, RoutedEventArgs e)
        {
            if (ListOfCards.Empty())
                return;
            ListOfCards.KnowCurrent();
        }

        private void ClickedIDontKnowIt(object sender, RoutedEventArgs e)
        {
            if (ListOfCards.Empty())
                return;
            ListOfCards.DontKnowCurrent();
        }

        private void ShowCurrentCard()
        {
            if (ListOfCards.Empty())
            {
                CardText.Text = "No cards in database!";
            }
            else
            {
                CardText.Text = ListOfCards.CurrentCard.ToString();
            }
        }

        private void ClickedCard(object sender, PointerRoutedEventArgs e)
        {
            if (ListOfCards.Empty())
                return;
            if (ListOfCards.CurrentCard.Status == CardStatus.Reversed)
            {
                ListOfCards.CurrentCard.Status = CardStatus.Normal;
                ListOfCards.Next();
                ShowCurrentCard();
            }
            else
            {
                ListOfCards.CurrentCard.Status = CardStatus.Reversed;
                ShowCurrentCard();
            }
        }

        private void ClickedAddWord(object sender, RoutedEventArgs e)
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
    }
}
