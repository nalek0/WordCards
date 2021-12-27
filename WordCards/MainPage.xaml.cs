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
        const string WordsFilePath = "Assets/Data/Words.txt";
        CardsList CardsList;

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

        private void initCards()
        {
            CardsList = CardsList.ParseFromFile(WordsFilePath);
            CardsList.Shuffle();
            ShowCard(CardsList.CurrentCard);
        }

        private void ClickedShuffle(object sender, RoutedEventArgs e)
        {
            CardsList.Shuffle();
            ShowCard(CardsList.CurrentCard);
        }

        private void ShowCard(WordCard card)
        {
            CardText.Text = card.ToString();
        }

        private void ClickedCard(object sender, PointerRoutedEventArgs e)
        {
            if (CardsList.CurrentCard.Status == CardStatus.Reversed)
            {
                CardsList.CurrentCard.Status = CardStatus.Normal;
                ShowCard(CardsList.Next());
            }
            else
            {
                CardsList.CurrentCard.Status = CardStatus.Reversed;
                ShowCard(CardsList.CurrentCard);
            }
        }
    }
}
