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
using System.Threading;


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
        public double money = 1000;
        public double bet = 0;


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

        }

        public void dealStart()
        {
            doubleBt.IsEnabled = true;
            hitBt.IsEnabled = true;
            holdBt.IsEnabled = true;
            if (deals == 6)
            {
                allDecks = new List<Tuple<int, int>>();
                for (int j = 0; j < 3; j++)
                {
                    for (int i = 0; i <= 12; i++)
                    {
                        allDecks.Add(new Tuple<int, int>(i, 0));
                        allDecks.Add(new Tuple<int, int>(i, 1));
                        allDecks.Add(new Tuple<int, int>(i, 2));
                        allDecks.Add(new Tuple<int, int>(i, 3));
                    }
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

            bustProbability();
            bjProbability();
            winProbability();

        }

        public void display(bool player, int a, int b, int c, int d)
        {
            Image pcard = new Image();
            pcard.Source = new CroppedBitmap(
                (BitmapSource)this.Resources["cards"],
            new Int32Rect(167 * allDecks.LastOrDefault().Item1 + fixDisp(allDecks.LastOrDefault().Item1), 243 * allDecks.LastOrDefault().Item2, 167, 243));
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


            bustProbability();
            bjProbability();

        }

        public void dealersTurn()
        {
            if (playerPoints > 21)
            {
                resultLabel.Content = "You Lose";
                resultLabel.Foreground = Brushes.Red;
                bet = 0;
                losses++;
            }
            else if(playerPoints == 21 && pBJ == 2)
            {
                resultLabel.Content = "Bjack Jack!";
                resultLabel.Foreground = Brushes.Gold;
                money = money + bet * 2.5;
                bet = 0;
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
                    bet = 0;
                    resultLabel.Foreground = Brushes.Red;
                }
                else if (dealerPoints == playerPoints)
                {
                    resultLabel.Content = "Draw";
                    money = money + bet;
                    bet = 0;
                    resultLabel.Foreground = Brushes.White;
                    draws++;
                }
                else
                {
                    resultLabel.Content = "You Win";
                    money = money + bet * 2;
                    bet = 0;
                    resultLabel.Foreground = Brushes.LightGreen;
                    wins++;
                }
            }
            changeBet();
            winsLabel.Content = "Wins: " + wins.ToString();
            lossesLabel.Content = "Losses: " + losses.ToString();
            drawsLabel.Content = "Draws: " + draws.ToString();

            resultLabel.Visibility = Visibility.Visible;
            continueBt.Visibility = Visibility.Visible;
        }

        private void hitBt_Click(object sender, RoutedEventArgs e)
        {
            display(true, pcoor, 0, 0, pcoor);
            doubleBt.IsEnabled = false;
            pBJ++;
            pcoor += 15;
            if (playerPoints >= 21)
            {
                dealersTurn();
                hitBt.IsEnabled = false;
                holdBt.IsEnabled = false;
            }
            else
            {
                winProbability();
            }
        }

        private void holdBt_Click(object sender, RoutedEventArgs e)
        {
            hitBt.IsEnabled = false;
            holdBt.IsEnabled = false;
            doubleBt.IsEnabled = false;
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
            resultLabel.Visibility = Visibility.Hidden;
            b5.IsEnabled = true; b20.IsEnabled = true; b50.IsEnabled = true; b100.IsEnabled = true; clearBt.IsEnabled = true; betBt.IsEnabled = true;


            pPointsLabel.Content = 0;
            dPointsLabel.Content = 0;
            pbLabel.Content = "0%";
            dpLabel.Content = "0%";
            pwLabel.Content = "0%";
        }

        private void betBt_Click(object sender, RoutedEventArgs e)
        {
            if(bet>0)
            {
                dealStart();
                b5.IsEnabled = false; b20.IsEnabled = false; b50.IsEnabled = false; b100.IsEnabled = false; clearBt.IsEnabled = false; betBt.IsEnabled = false;
            }

        }

        private void b5_Click(object sender, RoutedEventArgs e)
        {
            if(money>=5)
            {
                money -= 5;
                bet += 5;
                changeBet();
            }
        }

        private void b20_Click(object sender, RoutedEventArgs e)
        {
            if (money >= 20)
            {
                money -= 20;
                bet += 20;
                changeBet();
            }

        }

        private void b50_Click(object sender, RoutedEventArgs e)
        {
            if (money >= 50)
            {
                money -= 50;
                bet += 50;
                changeBet();
            }

        }

        private void b100_Click(object sender, RoutedEventArgs e)
        {
            if (money >= 100)
            {
                money -= 100;
                bet += 100;
                changeBet();
            }

        }
        private void clearBt_Click(object sender, RoutedEventArgs e)
        {
            money = money + bet;
            bet = 0;
            changeBet();
        }

        public void changeBet()
        {
            moneyLabel.Content = money + " $";
            betLabel.Content = bet + " $";
        }

        public int fixDisp(int x)
        {
            if (x < 6)
                return 0;
            else if (x < 11)
                return 3;
            else
                return 4;
        }

        private void doubleBt_Click(object sender, RoutedEventArgs e)
        {
            if(money>=bet)
            {
                money -= bet;
                bet  = bet*2;
                changeBet();
                hitBt_Click(new object(), new RoutedEventArgs());
                holdBt_Click(new object(), new RoutedEventArgs());
            }
        }

        // Probabilities

        public void bjProbability()
        {
            List<Tuple<int, int>> list = new List<Tuple<int, int>>();
            if (deals == 5)
            {
                list = new List<Tuple<int, int>>();
                for (int j = 0; j < 3; j++)
                {
                    for (int i = 0; i <= 12; i++)
                    {
                        list.Add(new Tuple<int, int>(i, 0));
                        list.Add(new Tuple<int, int>(i, 1));
                        list.Add(new Tuple<int, int>(i, 2));
                        list.Add(new Tuple<int, int>(i, 3));
                    }
                }
                list.Shuffle();
            }
            else
            {
                list = allDecks;
            }

            double tens = 0.0, aces = 0.0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Item1>=9)
                    tens += 1.0;
                if (list[i].Item1 == 0)
                    aces += 1.0;
            }

            double pr = 100*((tens / list.Count) * (aces / (list.Count - 1)) + (aces / list.Count) * (tens / (list.Count - 1)));
            bjprLablel.Content = Math.Round(pr, 2) + "%";



        }

        public void bustProbability()
        {
            double sum = 0.0;
            for(int i=0; i<allDecks.Count; i++)
            {
                if(playerAce)
                {
                    if (playerPoints-10 + Math.Min(allDecks[i].Item1 + 1, 10) > 21)
                        sum += 1.0;
                }
                else
                {
                    if (playerPoints + Math.Min(allDecks[i].Item1 + 1, 10) > 21)
                        sum += 1.0;
                }
            }

            double pr = (sum / allDecks.Count)*100;
            pbLabel.Content = Math.Round(pr, 2) + "%";

        }

        public void winProbability()
        {
            double l = 0.0;
            double d = 0.0;

            for(int i=0; i<130000; i++)
            {
                List<Tuple<int, int>> tmp = new List<Tuple<int, int>>(allDecks);
                tmp.Shuffle();
                int dp = dealerPoints;
                bool da = dealerAce;
                while(dp<=16)
                {
                    if(tmp[tmp.Count-1].Item1 == 0)
                    {
                        if (dp + 11 <= 21)
                            dp += 11;
                        else
                            dp += 1;
                    }
                    else
                    {
                        dp += Math.Min(tmp.LastOrDefault().Item1 + 1, 10);
                    }
                    if(dp>21 && da)
                    {
                        da = false;
                        dp -= 10;
                    }
                    tmp.RemoveAt(tmp.Count - 1);
                }

                if(dp>playerPoints && dp<=21)
                {
                    l += 1.0;

                }
                else if(dp == playerPoints)
                {
                    d += 1.0;
                }
            }

            double pr = (100.0 - ((l+d) / 130000) * 100);
            pwLabel.Content = Math.Round(pr, 2) + "%";
            pr =  (d / 130000) * 100;
            dpLabel.Content = Math.Round(pr, 2) + "%";


        }


    }


}
