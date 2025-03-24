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
        static int Licznik = 0;
        //Zdobyte punkty
        static int Punkty = 0;
        //Ilość pytań
        static int MaxPytan = 4;
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
            UtworzPytania();
            UstawPytania();
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
            CaloscLayout.IsVisible = true;
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
            if (Licznik >= Pytania.Count) { 
                CaloscLayout.IsVisible = false;
                LblKomunikat.HorizontalTextAlignment = TextAlignment.Center;
                LblKomunikat.FontSize = 26;
                if (Punkty >= (MaxPytan / 2))
                {
                    LblKomunikat.Text = "Gratulacje, zdałeś!";
                    LblKomunikat.TextColor = Colors.Green;
                    LblYourPoints.Text = $"Twoje punkty: {Punkty} / {MaxPytan}";
                }
                else
                {
                    LblKomunikat.Text = "Niestety nie zdałeś, spróbuj następnym razem :(";
                    LblKomunikat.TextColor= Colors.Red;
                    LblYourPoints.Text = $"Twoje punkty: {Punkty} / {MaxPytan}";
                }
                    return;
            } // Zapobiega błędom poza zakresem

            LblKomunikat.Text = "";
            UstawPytania();

            NextBtn.IsEnabled = false;
            ConfirmBtn.IsEnabled = true;
            RadioOdp1.IsEnabled = true;
            RadioOdp2.IsEnabled = true;
            RadioOdp3.IsEnabled = true;
            RadioOdp4.IsEnabled = true;
        }


    }
}
