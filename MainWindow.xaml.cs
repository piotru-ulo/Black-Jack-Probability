using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Black_Jack_Probability
{
    public static class Extensions
    {
        private static Random rand = new Random();

        public static void Shuffle<T>(this IList<T> values)
        {
            for (int i = values.Count - 1; i > 0; i--)
            {
                int k = rand.Next(i + 1);
                T value = values[k];
                values[k] = values[i];
                values[i] = value;
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public List<Tuple<int,int>> allDecks = new List<Tuple<int, int>>();
        public int playerPoints = 0;
        public bool playerAce = false;
        public int pcoor = 30; // display coords player
        public int wins = 0;
        public int losses = 0;
        public int draws = 0;
        public int pBJ = 0;


        public int dealerPoints = 0;
        public bool dealerAce = false;
        public int dcoor = 15; // display coords dealer
        public int dBJ = 0;

        public int deals = 6;

        public MainWindow()
        {
            // CARDS: 167*x, 243*y
            InitializeComponent();

            CroppedBitmap cb = new CroppedBitmap(
                (BitmapSource)this.Resources["cards"],
            new Int32Rect(167*2,243*4, 167, 243));
            deck.Source = cb;
            dealStart();

        }

        public void dealStart()
        {
            if (deals == 6)
            {
                allDecks = new List<Tuple<int, int>>();
                for (int i = 0; i <= 12; i++)
                {
                    allDecks.Add(new Tuple<int, int>(i, 0));
                    allDecks.Add(new Tuple<int, int>(i, 1));
                    allDecks.Add(new Tuple<int, int>(i, 2));
                    allDecks.Add(new Tuple<int, int>(i, 3));
                }
                allDecks.Shuffle();
                deals = 0;
            }
            display(true, 0, 0, 0, 0);
            display(true, 15, 0, 0, 15);
            pBJ = 2;
            display(false, 0, 0, 0, 0);
            dBJ = 1;
            if(playerPoints==21)
            {
                dealersTurn();
                hitBt.IsEnabled = false;
                holdBt.IsEnabled = false;
            }

        }

        public void display(bool player, int a, int b, int c, int d)
        {
            Image pcard = new Image();
            pcard.Source = new CroppedBitmap(
                (BitmapSource)this.Resources["cards"],
            new Int32Rect(167 * allDecks.LastOrDefault().Item1, 243 * allDecks.LastOrDefault().Item2, 167, 243));
            pcard.Margin = new Thickness(a, b, c, d);
            pcard.Height = 120;
            pcard.Width = 85;
            if (player)
            {
                if (allDecks.LastOrDefault().Item1 == 0)
                {
                    playerPoints += 11;
                    playerAce = true;
                }
                else if (allDecks.LastOrDefault().Item1 < 9)
                    playerPoints += allDecks.LastOrDefault().Item1 + 1;
                else
                    playerPoints += 10;

                if (playerPoints > 21 && playerAce)
                {
                    playerAce = false;
                    playerPoints -= 10;
                }

                if (playerAce)
                    pPointsLabel.Content = (playerPoints-10).ToString() + "/" + playerPoints.ToString();
                else
                    pPointsLabel.Content = playerPoints;

                pcard.VerticalAlignment = VerticalAlignment.Bottom;
                pcard.HorizontalAlignment = HorizontalAlignment.Left;
                playerCards.Children.Add(pcard);
            }
            else
            {
                if (allDecks.LastOrDefault().Item1 == 0)
                {
                    dealerPoints += 11;
                    dealerAce = true;
                }
                else if (allDecks.LastOrDefault().Item1 < 10)
                    dealerPoints += allDecks.LastOrDefault().Item1 + 1;
                else
                    dealerPoints += 10;

                if (dealerPoints > 21 && dealerAce)
                {
                    dealerAce = false;
                    dealerPoints -= 10;
                }

                if (dealerAce)
                    dPointsLabel.Content = (dealerPoints - 10).ToString() + "/" + dealerPoints.ToString();
                else
                    dPointsLabel.Content = dealerPoints;
                pcard.VerticalAlignment = VerticalAlignment.Top;
                pcard.HorizontalAlignment = HorizontalAlignment.Right;
                dealerCards.Children.Add(pcard);

            }
            allDecks.RemoveAt(allDecks.Count - 1);

        }

        public void dealersTurn()
        {
            if (playerPoints > 21)
            {
                resultLabel.Content = "You Lose";
                losses++;
            }
            else if(playerPoints == 21 && pBJ == 2)
            {
                resultLabel.Content = "Bjack Jack!";
                wins++;
            }
            else
            {
                while (dealerPoints <= 16)
                {
                    display(false, 0, dcoor, dcoor, 0);
                    dBJ++;
                    dcoor += 15;
                }
                if (dealerPoints > playerPoints && dealerPoints <= 21 || (dealerPoints==21 && dBJ==2))
                {
                    losses++;
                    resultLabel.Content = "You Lose";
                }
                else if (dealerPoints == playerPoints)
                {
                    resultLabel.Content = "Draw";
                    draws++;
                }
                else
                {
                    resultLabel.Content = "You Win";
                    wins++;
                }
            }
            winsLabel.Content = "Wins: " + wins.ToString();
            lossesLabel.Content = "Losses: " + losses.ToString();
            drawsLabel.Content = "Draws: " + draws.ToString();

            resultLabel.Visibility = Visibility.Visible;
            continueBt.Visibility = Visibility.Visible;
        }

        private void hitBt_Click(object sender, RoutedEventArgs e)
        {
            display(true, pcoor, 0, 0, pcoor);
            pBJ++;
            pcoor += 15;
            if (playerPoints >= 21)
            {
                dealersTurn();
                hitBt.IsEnabled = false;
                holdBt.IsEnabled = false;
            }
        }

        private void holdBt_Click(object sender, RoutedEventArgs e)
        {
            hitBt.IsEnabled = false;
            holdBt.IsEnabled = false;
            dealersTurn();

        }

        private void continueBt_Click(object sender, RoutedEventArgs e)
        {
            continueBt.Visibility = Visibility.Hidden;
            deals++;
            playerAce = false;
            playerPoints = 0;
            dealerAce = false;
            dealerPoints = 0;
            pcoor = 30;
            dcoor = 15;
            playerCards.Children.Clear();
            dealerCards.Children.Clear();
            hitBt.IsEnabled = true;
            holdBt.IsEnabled = true;
            resultLabel.Visibility = Visibility.Hidden;

            dealStart();

        }
    }


}
