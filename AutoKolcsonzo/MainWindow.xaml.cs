using System.Windows;
using AutoKolcsonzo.Ablakok;

namespace AutoKolcsonzo
{
    /// A főablak (MainWindow) mögöttes kódja.
    /// Ez az osztály kezeli a menüsor és az eszköztár eseményeit,
    /// és megnyitja a megfelelő kezelő ablakokat.
    /// A főablak az alkalmazás kiindulópontja, ahonnan az összes
    /// funkció elérhető.
    public partial class MainWindow : Window
    {
        /// A főablak konstruktora.
        /// Inicializálja a XAML-ben definiált vezérlőelemeket.
        public MainWindow()
        {
            InitializeComponent();
        }

        /// Az "Autók kezelése" menüpont és eszköztárgomb eseménykezelője.
        /// Megnyitja az AutoKezelo ablakot, ahol az autók CRUD műveletei
        /// végezhetők el. Az állapotsor szövegét is frissíti.
        private void AutokMenupont_Click(object sender, RoutedEventArgs e)
        {
            // Állapotsor frissítése
            allapotSzoveg.Text = "Autók kezelése...";

            // Az autókezelő ablak megnyitása dialógusablakként
            var autoKezelo = new AutoKezelo();
            autoKezelo.Owner = this;
            autoKezelo.ShowDialog();

            // Állapotsor visszaállítása
            allapotSzoveg.Text = "Készenlét";
        }

        /// Az "Ügyfelek kezelése" menüpont és eszköztárgomb eseménykezelője.
        /// Megnyitja az UgyfelKezelo ablakot, ahol az ügyfelek CRUD műveletei
        /// végezhetők el.
        private void UgyfelekMenupont_Click(object sender, RoutedEventArgs e)
        {
            allapotSzoveg.Text = "Ügyfelek kezelése...";

            var ugyfelKezelo = new UgyfelKezelo();
            ugyfelKezelo.Owner = this;
            ugyfelKezelo.ShowDialog();

            allapotSzoveg.Text = "Készenlét";
        }

        /// A "Kölcsönzések kezelése" menüpont és eszköztárgomb eseménykezelője.
        /// Megnyitja a KolcsonzesKezelo ablakot, ahol a kölcsönzések CRUD műveletei
        /// végezhetők el.
        private void KolcsonzesekMenupont_Click(object sender, RoutedEventArgs e)
        {
            allapotSzoveg.Text = "Kölcsönzések kezelése...";

            var kolcsonzesKezelo = new KolcsonzesKezelo();
            kolcsonzesKezelo.Owner = this;
            kolcsonzesKezelo.ShowDialog();

            allapotSzoveg.Text = "Készenlét";
        }

        /// A "Névjegy" menüpont eseménykezelője.
        /// Megjeleníti az alkalmazás névjegy információit egy üzenetablakban.
        private void NevjegyMenupont_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Autokölcsönzö Rendszer\n" +
                "Verzió: 1.0\n\n" +
                "WPF alapú grafikus adatbázis-kezelő alkalmazás.\n" +
                "Adatbázis: MS SQL Server Express\n\n" +
                "Vizuális programozás projektfeladat",
                "Névjegy",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// A "Kilépés" menüpont és eszköztárgomb eseménykezelője.
        /// Megerősítés után bezárja az alkalmazást.
        private void KilepesMenupont_Click(object sender, RoutedEventArgs e)
        {
            // Megerősítő kérdés megjelenítése a véletlen bezárás elkerülése érdekében
            var eredmeny = MessageBox.Show(
                "Biztosan ki szeretne lépni az alkalmazásból?",
                "Kilépés megerősítése",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            // Ha a felhasználó az "Igen" gombra kattintott, bezárjuk az alkalmazást
            if (eredmeny == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
