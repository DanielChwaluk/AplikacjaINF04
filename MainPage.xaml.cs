using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace mamciedosc
{
    public partial class MainPage : ContentPage
    {
        static Random rnd = new Random();
        //Lista pytań
        static List<Pytanie> Pytania = new List<Pytanie>();
        //Kolejność wylosowanych pytań po id
        static List<int> wylosowane = new List<int>();
        //Licznik, które pytanie z rzędu
        static int Licznik;
        //Zdobyte punkty
        static int Punkty;
        //Ilość pytań
        static int MaxPytan;
        //Pytan w bazie
        static int PytaniaBazy = 0;
        // połączenie z bazą
        static string connStr = "server=localhost;user=root;database=pytaniaegz;port=3306;password=";
        static MySqlConnection conn = new MySqlConnection(connStr);
        class Pytanie
        {
            public int idPyt {  get; set; }
            public string pytanie { get; set; }
            public string zdjecie {  get; set; }
            public string odp1 { get; set; }
            public string odp2 { get; set; }
            public string odp3 { get; set; }
            public string odp4 { get; set; }
            public string poprOdp { get; set; }
            public Pytanie(int idPyt, string pytanie, string odp1, string odp2, string odp3, string odp4, string poprOdp,string zdjecie)
            {
                this.idPyt = idPyt;
                this.pytanie = pytanie;
                this.zdjecie = zdjecie;
                this.odp1 = odp1;
                this.odp2 = odp2;
                this.odp3 = odp3;
                this.odp4 = odp4;
                this.poprOdp = poprOdp;
            }
        }

        public MainPage()
        {
            InitializeComponent();
            PobierzMaxPytan();
        }

        private void PobierzMaxPytan()
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT COUNT(id) FROM pytania;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                            if (rdr.Read())
                                PytaniaBazy = Convert.ToInt32(rdr[0]);
                }
                MaxPytanLbl.Text = $"Ustaw Liczbę pytań: 2 - {PytaniaBazy}";
                DefaultUstawPytaniaBtn.IsEnabled = PytaniaBazy >= 40;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd połączenia z bazą: {ex.Message}");
            }
            finally
            {
                conn.Close();
            }
        }

        private void UstawLosowosc()
        {
            do
            {
                int wylosowana = rnd.Next(1, MaxPytan+1);
                if (!wylosowane.Contains(wylosowana) == true)
                    wylosowane.Add(wylosowana);
            } while (wylosowane.Count < MaxPytan);
        }
        private void UtworzPytania()
        {
            UstawLosowosc();

            try
            {
                conn.Open();
                for (int i = 0; i < MaxPytan; i++)
                {
                    Debug.WriteLine($"PETLA W UTWORZ PYTANIA OD {i}");
                    string query = $"SELECT * FROM pytania WHERE id={wylosowane[i]}";
                    MySqlCommand wynik = new MySqlCommand(query, conn);
                    MySqlDataReader rdr = wynik.ExecuteReader();
                    
                    string odp1,odp2,odp3,odp4;

                    if (rdr.Read())
                    {
                        int idPytania = Int32.Parse(rdr[0].ToString());
                        string pyt = rdr[1].ToString();
                        string poprodp = rdr[6].ToString();
                        string zdj = rdr[7].ToString();
                        odp1 = rdr[2].ToString();
                        odp2 = rdr[3].ToString();
                        odp3 = rdr[4].ToString();
                        odp4 = rdr[5].ToString();
                        List<string> odpowiedzi = new List<string> { odp1,odp2,odp3, odp4};
                        odpowiedzi = odpowiedzi.OrderBy(x => rnd.Next()).ToList();

                        Pytania.Add(new Pytanie(idPytania, pyt,
                            odpowiedzi[0], odpowiedzi[1], odpowiedzi[2], odpowiedzi[3], poprodp, zdj));
                        rdr.Close();
                    }
                    rdr.Close(); 
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd połączenia z bazą: {ex.Message}");
            }
            finally
            {
                conn.Close();
            }
        }
        private void UstawPytania()
        {
            LblYourPoints.IsVisible = true;
            CaloscLayout.IsVisible = true;
            PrzyciskiLayout.IsVisible = true;
            LblYourPoints.IsVisible = true;
            ConfirmBtn.IsEnabled = true;
            NextBtn.IsEnabled = false;
            LblPyt.Text = Pytania[Licznik].pytanie;
            if (!string.IsNullOrEmpty(Pytania[Licznik].zdjecie))
            {
                ImgPyt.IsVisible = true;
                ImgPyt.Source = Pytania[Licznik].zdjecie;
            }
            LblOdp1.Text = Pytania[Licznik].odp1;
            LblOdp2.Text = Pytania[Licznik].odp2;
            LblOdp3.Text = Pytania[Licznik].odp3;
            LblOdp4.Text = Pytania[Licznik].odp4;
        }
        private void UstawPytaniaClicked(object sender, EventArgs e)
        {
            MaxPytan = 40;
            UtworzPytania();
            UstawPytania();
            FirstLayout.IsVisible = false;
        }
        private void UstawPytania2Clicked(object sender, EventArgs e)
        {
            MaxPytan = Int32.Parse(EntryIloscPytan.Text);
            UtworzPytania();
            UstawPytania();
            FirstLayout.IsVisible = false;
        }

        private async void EntryChanged(object sender, EventArgs e)
        {
            if (EntryIloscPytan == null || string.IsNullOrEmpty(EntryIloscPytan.Text))
            {
                await DisplayAlert("Błąd", "Brak podanej ilości pytań", "OK");
                EntryIloscPytan.Text = "2";
                return;
            }
            if(Int32.TryParse(EntryIloscPytan.Text, out int ilosc))
                if(ilosc<2 || ilosc > PytaniaBazy)
                    {
                        await DisplayAlert("Błąd", "Zła podana liczba pytań", "OK");
                    EntryIloscPytan.Text = "2";
                    return;
                    } 
                    else CustomPytaniaBtn.Text = $"Ustaw {ilosc} Pytań";
            else
            {
                await DisplayAlert("Błąd", "Nie podano liczby", "OK");
                EntryIloscPytan.Text = "2";
                return;
            }
        }

        private void OnConfirmClicked(object sender, EventArgs e)
        {
            string TwojaOdp = "";
            if (RadioOdp1.IsChecked) TwojaOdp = LblOdp1.Text;
            if (RadioOdp2.IsChecked) TwojaOdp = LblOdp2.Text;
            if (RadioOdp3.IsChecked) TwojaOdp = LblOdp3.Text;
            if (RadioOdp4.IsChecked) TwojaOdp = LblOdp4.Text; 


            if (TwojaOdp == Pytania[Licznik].poprOdp)
            {
                Punkty++;
                LblKomunikat.TextColor = Colors.Green;
                LblKomunikat.Text = $"✅ POPRAWNA ODPOWIEDŹ!\n Prawidłowa: {Pytania[Licznik].poprOdp}";
            }
            else
            {
                LblKomunikat.TextColor = Colors.Red;
                LblKomunikat.Text = $"❌ ZŁA ODPOWIEDŹ!\n Prawidłowa: {Pytania[Licznik].poprOdp}";
            }

            Licznik++;
            LblYourPoints.Text = $"Twoje punkty: {Punkty}";

            ConfirmBtn.IsEnabled = false;
            NextBtn.IsEnabled = true;

            RadioOdp1.IsEnabled = false;
            RadioOdp2.IsEnabled = false;
            RadioOdp3.IsEnabled = false;
            RadioOdp4.IsEnabled = false;
            ImgPyt.IsVisible = false;
            if (Licznik >= Pytania.Count)
            {
                NextBtn.Text = "Zakończ";
            }
        }
        private void OnNextClicked(object sender, EventArgs e)
        {
            if (Licznik >= Pytania.Count)
            {
                double procenty = Math.Round((double)Punkty / MaxPytan * 100, 2);
                PrzyciskiLayout.IsVisible = false;
                CaloscLayout.IsVisible = false;
                LblKomunikat.HorizontalTextAlignment = TextAlignment.Center;
                LblKomunikat.FontSize = 26;
                ResetBtn.IsVisible = true;
                if (procenty>=50)
                {
                    LblKomunikat.Text = $"Gratulacje, zdałeś! Uzyskałeś {procenty} %!";
                    LblKomunikat.TextColor = Colors.Green;
                    LblYourPoints.Text = $"Twoje punkty: {Punkty} / {MaxPytan}";
                }
                else
                {
                    LblKomunikat.Text = $"Niestety nie zdałeś, spróbuj następnym razem :(, Uzyskałeś {procenty} %!";
                    LblKomunikat.TextColor = Colors.Red;
                    LblYourPoints.Text = $"Twoje punkty: {Punkty} / {MaxPytan}";
                }
                return;
            }

            RadioOdp1.IsChecked = false;
            RadioOdp2.IsChecked = false;
            RadioOdp3.IsChecked = false;
            RadioOdp4.IsChecked = false;

            LblKomunikat.Text = "";
            UstawPytania();

            NextBtn.IsEnabled = false;
            ConfirmBtn.IsEnabled = true;
            RadioOdp1.IsEnabled = true;
            RadioOdp2.IsEnabled = true;
            RadioOdp3.IsEnabled = true;
            RadioOdp4.IsEnabled = true;
        }
        private void OnResetButton(object sender, EventArgs e)
        {
            Pytania.Clear();
            wylosowane.Clear();
            Licznik = 0;
            Punkty = 0;
            MaxPytan = 0;
            NextBtn.Text = "Kolejne Pytanie";

            FirstLayout.IsVisible = true;
            ResetBtn.IsVisible = false;
            RadioOdp1.IsEnabled = true;
            RadioOdp2.IsEnabled = true;
            RadioOdp3.IsEnabled = true;
            RadioOdp4.IsEnabled = true;
            LblKomunikat.Text = "";
            LblYourPoints.Text = "Twoje punkty 0";
            LblYourPoints.IsVisible = false;
        }
    }
}
