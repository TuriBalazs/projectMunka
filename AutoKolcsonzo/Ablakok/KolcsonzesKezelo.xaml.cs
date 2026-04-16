using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AutoKolcsonzo.AdatEleres;
using AutoKolcsonzo.Modellek;

namespace AutoKolcsonzo.Ablakok
{
    /// <summary>
    /// A kölcsönzések kezelésére szolgáló ablak mögöttes kódja.
    /// Megvalósítja a kölcsönzések teljes CRUD funkcionalitását.
    /// A kölcsönzés az Autok és Ugyfelek táblákhoz kapcsolódik,
    /// ezért az űrlapon ComboBox-okat használ az autó és ügyfél kiválasztásához.
    /// Az összeg automatikusan kiszámítható a napok száma és a napi ár alapján.
    /// </summary>
    public partial class KolcsonzesKezelo : Window
    {
        /// <summary>
        /// Az autók listája a ComboBox feltöltéséhez.
        /// Az ablak megnyitásakor töltődik be az adatbázisból.
        /// </summary>
        private List<Auto> autokLista = new List<Auto>();

        /// <summary>
        /// Az ügyfelek listája a ComboBox feltöltéséhez.
        /// Az ablak megnyitásakor töltődik be az adatbázisból.
        /// </summary>
        private List<Ugyfel> ugyfelekLista = new List<Ugyfel>();

        /// <summary>
        /// Az ablak konstruktora. Inicializálja a vezérlőelemeket,
        /// betölti a ComboBox-ok tartalmát és a kölcsönzések listáját.
        /// </summary>
        public KolcsonzesKezelo()
        {
            InitializeComponent();
            ComboBoxokFeltoltese();
            AdatokBetoltese();
        }

        /// <summary>
        /// Az autó és ügyfél ComboBox-ok feltöltése az adatbázis adataival.
        /// Az autó ComboBox a ToString() metódus által formázott szöveget jeleníti meg
        /// (Márka Típus (Rendszám)), az ügyfél ComboBox pedig a nevet és a személyi ig. számot.
        /// </summary>
        private void ComboBoxokFeltoltese()
        {
            try
            {
                // Az autók betöltése a ComboBox-ba
                autokLista = AdatbazisKezelo.AutokLekerdezese();
                autoComboBox.ItemsSource = autokLista;
                autoComboBox.DisplayMemberPath = ""; // ToString() metódus használata

                // Az ügyfelek betöltése a ComboBox-ba
                ugyfelekLista = AdatbazisKezelo.UgyfelekLekerdezese();
                ugyfelComboBox.ItemsSource = ugyfelekLista;
                ugyfelComboBox.DisplayMemberPath = ""; // ToString() metódus használata
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Hiba a legördülő listák feltöltésekor:\n{ex.Message}",
                    "Adatbázis hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// A kölcsönzések adatainak betöltése az adatbázisból a DataGrid-be.
        /// A lekérdezés JOIN-t használ az autó és ügyfél neveinek megjelenítéséhez.
        /// </summary>
        private void AdatokBetoltese()
        {
            try
            {
                kolcsonzesekDataGrid.ItemsSource = AdatbazisKezelo.KolcsonzesekLekerdezese();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Hiba az adatok betöltésekor:\n{ex.Message}",
                    "Adatbázis hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// A DataGrid kiválasztás-változás eseménykezelője.
        /// A kiválasztott kölcsönzés adatait betölti az űrlap mezőibe.
        /// A ComboBox-okban a megfelelő autót és ügyfelet választja ki
        /// az AutoId és UgyfelId alapján.
        /// </summary>
        private void KolcsonzesekDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (kolcsonzesekDataGrid.SelectedItem is Kolcsonzes kivalasztottKolcsonzes)
            {
                // Az autó ComboBox-ban a megfelelő elem kiválasztása az AutoId alapján
                for (int i = 0; i < autokLista.Count; i++)
                {
                    if (autokLista[i].AutoId == kivalasztottKolcsonzes.AutoId)
                    {
                        autoComboBox.SelectedIndex = i;
                        break;
                    }
                }

                // Az ügyfél ComboBox-ban a megfelelő elem kiválasztása az UgyfelId alapján
                for (int i = 0; i < ugyfelekLista.Count; i++)
                {
                    if (ugyfelekLista[i].UgyfelId == kivalasztottKolcsonzes.UgyfelId)
                    {
                        ugyfelComboBox.SelectedIndex = i;
                        break;
                    }
                }

                // Dátumok beállítása a DatePicker vezérlőkben
                kezdoDatumPicker.SelectedDate = kivalasztottKolcsonzes.KezdoDatum;
                vegDatumPicker.SelectedDate = kivalasztottKolcsonzes.VegDatum;

                // Összeg és állapot beállítása
                osszegTextBox.Text = kivalasztottKolcsonzes.Osszeg.ToString();

                // Az állapot ComboBox megfelelő elemének kiválasztása
                for (int i = 0; i < allapotComboBox.Items.Count; i++)
                {
                    var elem = allapotComboBox.Items[i] as ComboBoxItem;
                    if (elem?.Content?.ToString() == kivalasztottKolcsonzes.Allapot)
                    {
                        allapotComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Az autó ComboBox kiválasztás-változás eseménykezelője.
        /// Az összeg automatikus újraszámolásához használatos,
        /// amikor a felhasználó másik autót választ ki.
        /// </summary>
        private void AutoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OsszegSzamitasa();
        }

        /// <summary>
        /// A dátumválasztók változás eseménykezelője.
        /// Mindkét dátumválasztóhoz (kezdő és vég) ugyanez az eseménykezelő tartozik.
        /// Amikor bármelyik dátum megváltozik, újraszámolja az összeget.
        /// </summary>
        private void DatumValtozas(object sender, SelectionChangedEventArgs e)
        {
            OsszegSzamitasa();
        }

        /// <summary>
        /// A kölcsönzés összegének automatikus kiszámítása.
        /// Képlet: napok száma * autó napi ára.
        /// Csak akkor számol, ha van kiválasztott autó és mindkét dátum meg van adva.
        /// Ha a vég dátum korábbi, mint a kezdő dátum, nem számol.
        /// </summary>
        private void OsszegSzamitasa()
        {
            // Ellenőrizzük, hogy van-e kiválasztott autó és mindkét dátum meg van-e adva
            if (autoComboBox.SelectedItem is Auto kivalasztottAuto &&
                kezdoDatumPicker.SelectedDate.HasValue &&
                vegDatumPicker.SelectedDate.HasValue)
            {
                // A napok számának kiszámítása
                var napokSzama = (vegDatumPicker.SelectedDate.Value - kezdoDatumPicker.SelectedDate.Value).Days;

                // Csak pozitív napszám esetén számolunk
                if (napokSzama > 0)
                {
                    // Összeg = napok száma * napi ár
                    int osszeg = napokSzama * kivalasztottAuto.NapiAr;
                    osszegTextBox.Text = osszeg.ToString();
                }
            }
        }

        /// <summary>
        /// Az adatbeviteli mezők ellenőrzése (validáció).
        /// Ellenőrzi, hogy van-e kiválasztott autó és ügyfél,
        /// a dátumok érvényesek-e, és az összeg pozitív szám-e.
        /// </summary>
        /// <returns>true, ha minden adat érvényes; false, ha van hiba.</returns>
        private bool AdatokEllenorzese()
        {
            // Autó kiválasztás: kötelező
            if (autoComboBox.SelectedItem == null)
            {
                MessageBox.Show("Válasszon ki egy autót!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Ügyfél kiválasztás: kötelező
            if (ugyfelComboBox.SelectedItem == null)
            {
                MessageBox.Show("Válasszon ki egy ügyfelet!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Kezdő dátum: kötelező
            if (!kezdoDatumPicker.SelectedDate.HasValue)
            {
                MessageBox.Show("A kezdő dátum megadása kötelező!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Vég dátum: kötelező
            if (!vegDatumPicker.SelectedDate.HasValue)
            {
                MessageBox.Show("A vég dátum megadása kötelező!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // A vég dátumnak későbbinek kell lennie, mint a kezdő dátum
            if (vegDatumPicker.SelectedDate.Value <= kezdoDatumPicker.SelectedDate.Value)
            {
                MessageBox.Show("A vég dátumnak későbbinek kell lennie, mint a kezdő dátum!",
                    "Érvénytelen dátum", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Összeg: kötelező, pozitív egész szám
            if (!int.TryParse(osszegTextBox.Text, out int osszeg) || osszeg <= 0)
            {
                MessageBox.Show("Az összeg pozitív egész szám kell legyen!",
                    "Érvénytelen adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                osszegTextBox.Focus();
                return false;
            }

            // Állapot: kötelező
            if (allapotComboBox.SelectedItem == null)
            {
                MessageBox.Show("Válassza ki a kölcsönzés állapotát!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Az űrlap mezőiből Kolcsonzes objektum létrehozása.
        /// A ComboBox-okból az AutoId-t és UgyfelId-t olvassa ki,
        /// a DatePicker-ekből a dátumokat, és az állapot ComboBox-ból az állapotot.
        /// </summary>
        /// <returns>Az űrlap adataiból létrehozott Kolcsonzes objektum.</returns>
        private Kolcsonzes UrlapbolKolcsonzes()
        {
            var kivalasztottAuto = autoComboBox.SelectedItem as Auto;
            var kivalasztottUgyfel = ugyfelComboBox.SelectedItem as Ugyfel;
            var kivalasztottAllapot = allapotComboBox.SelectedItem as ComboBoxItem;

            return new Kolcsonzes
            {
                AutoId = kivalasztottAuto!.AutoId,
                UgyfelId = kivalasztottUgyfel!.UgyfelId,
                KezdoDatum = kezdoDatumPicker.SelectedDate!.Value,
                VegDatum = vegDatumPicker.SelectedDate!.Value,
                Osszeg = int.Parse(osszegTextBox.Text),
                Allapot = kivalasztottAllapot?.Content?.ToString() ?? "Aktív"
            };
        }

        /// <summary>
        /// Az "Új hozzáadása" gomb eseménykezelője.
        /// Ellenőrzi az adatokat, majd hozzáadja az új kölcsönzést az adatbázishoz.
        /// </summary>
        private void HozzaadasGomb_Click(object sender, RoutedEventArgs e)
        {
            if (!AdatokEllenorzese()) return;

            try
            {
                var ujKolcsonzes = UrlapbolKolcsonzes();
                AdatbazisKezelo.KolcsonzesHozzaadasa(ujKolcsonzes);
                AdatokBetoltese();
                MezokTorlese();

                MessageBox.Show("A kölcsönzés sikeresen hozzáadva!",
                    "Sikeres művelet", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a hozzáadáskor:\n{ex.Message}",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// A "Módosítás" gomb eseménykezelője.
        /// A kiválasztott kölcsönzés adatait módosítja az űrlapon megadott értékekkel.
        /// </summary>
        private void ModositasGomb_Click(object sender, RoutedEventArgs e)
        {
            if (kolcsonzesekDataGrid.SelectedItem is not Kolcsonzes kivalasztottKolcsonzes)
            {
                MessageBox.Show("Válasszon ki egy kölcsönzést a táblázatból a módosításhoz!",
                    "Nincs kiválasztva", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!AdatokEllenorzese()) return;

            try
            {
                var modositottKolcsonzes = UrlapbolKolcsonzes();
                modositottKolcsonzes.KolcsonzesId = kivalasztottKolcsonzes.KolcsonzesId;

                AdatbazisKezelo.KolcsonzesModositasa(modositottKolcsonzes);
                AdatokBetoltese();

                MessageBox.Show("A kölcsönzés adatai sikeresen módosítva!",
                    "Sikeres művelet", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a módosításkor:\n{ex.Message}",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// A "Törlés" gomb eseménykezelője.
        /// Megerősítés után törli a kiválasztott kölcsönzést az adatbázisból.
        /// </summary>
        private void TorlesGomb_Click(object sender, RoutedEventArgs e)
        {
            if (kolcsonzesekDataGrid.SelectedItem is not Kolcsonzes kivalasztottKolcsonzes)
            {
                MessageBox.Show("Válasszon ki egy kölcsönzést a táblázatból a törléshez!",
                    "Nincs kiválasztva", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var eredmeny = MessageBox.Show(
                $"Biztosan törölni szeretné a kiválasztott kölcsönzést?\n\n" +
                $"Autó: {kivalasztottKolcsonzes.AutoNev}\n" +
                $"Ügyfél: {kivalasztottKolcsonzes.UgyfelNev}\n" +
                $"Időszak: {kivalasztottKolcsonzes.KezdoDatum:yyyy.MM.dd} - {kivalasztottKolcsonzes.VegDatum:yyyy.MM.dd}",
                "Törlés megerősítése",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (eredmeny == MessageBoxResult.Yes)
            {
                try
                {
                    AdatbazisKezelo.KolcsonzesTorlese(kivalasztottKolcsonzes.KolcsonzesId);
                    AdatokBetoltese();
                    MezokTorlese();

                    MessageBox.Show("A kölcsönzés sikeresen törölve!",
                        "Sikeres művelet", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba a törléskor:\n{ex.Message}",
                        "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// A "Mezők törlése" gomb eseménykezelője.
        /// </summary>
        private void MezokTorleseGomb_Click(object sender, RoutedEventArgs e)
        {
            MezokTorlese();
        }

        /// <summary>
        /// Segédmetódus: törli az összes beviteli mező tartalmát,
        /// visszaállítja az alapértelmezett értékeket és frissíti a ComboBox-okat.
        /// </summary>
        private void MezokTorlese()
        {
            autoComboBox.SelectedIndex = -1;
            ugyfelComboBox.SelectedIndex = -1;
            kezdoDatumPicker.SelectedDate = null;
            vegDatumPicker.SelectedDate = null;
            osszegTextBox.Clear();
            allapotComboBox.SelectedIndex = 0; // Alapértelmezett: "Aktív"
            kolcsonzesekDataGrid.SelectedItem = null;

            // ComboBox-ok frissítése (ha közben új autó/ügyfél lett felvéve)
            ComboBoxokFeltoltese();
        }

        /// <summary>
        /// A "Keresés" gomb eseménykezelője.
        /// A kiválasztott mező és keresőszöveg alapján szűri a kölcsönzéseket.
        /// </summary>
        private void KeresesGomb_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(keresoszovegTextBox.Text))
            {
                MessageBox.Show("Adjon meg keresőszöveget!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                keresoszovegTextBox.Focus();
                return;
            }

            try
            {
                var kivalasztottElem = keresesiMezoComboBox.SelectedItem as ComboBoxItem;
                string mezo = kivalasztottElem?.Tag?.ToString() ?? "AutoNev";

                kolcsonzesekDataGrid.ItemsSource = AdatbazisKezelo.KolcsonzesekKeresese(
                    mezo, keresoszovegTextBox.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a kereséskor:\n{ex.Message}",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Az "Összes mutatása" gomb eseménykezelője.
        /// Visszaállítja a teljes kölcsönzéslistát.
        /// </summary>
        private void OsszesGomb_Click(object sender, RoutedEventArgs e)
        {
            keresoszovegTextBox.Clear();
            AdatokBetoltese();
        }
    }
}
