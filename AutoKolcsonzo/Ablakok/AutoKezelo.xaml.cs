using System;
using System.Windows;
using System.Windows.Controls;
using AutoKolcsonzo.AdatEleres;
using AutoKolcsonzo.Modellek;

namespace AutoKolcsonzo.Ablakok
{
    /// <summary>
    /// Az autók kezelésére szolgáló ablak mögöttes kódja.
    /// Megvalósítja az autók teljes CRUD funkcionalitását:
    /// hozzáadás, megjelenítés, módosítás, törlés és keresés.
    /// Az adatbevitel ellenőrzött: a kötelező mezők kitöltése
    /// és az értékek formátuma validálva van.
    /// </summary>
    public partial class AutoKezelo : Window
    {
        /// <summary>
        /// Az ablak konstruktora. Inicializálja a vezérlőelemeket
        /// és betölti az autók adatait az adatbázisból.
        /// </summary>
        public AutoKezelo()
        {
            InitializeComponent();
            AdatokBetoltese();
        }

        /// <summary>
        /// Az autók adatainak betöltése az adatbázisból a DataGrid-be.
        /// Ez a metódus meghívódik az ablak megnyitásakor,
        /// valamint minden CRUD művelet után az adatok frissítéséhez.
        /// </summary>
        private void AdatokBetoltese()
        {
            try
            {
                // Az összes autó lekérdezése és megjelenítése a DataGrid-ben
                autokDataGrid.ItemsSource = AdatbazisKezelo.AutokLekerdezese();
            }
            catch (Exception ex)
            {
                // Hibaüzenet megjelenítése, ha az adatbázis-kapcsolat sikertelen
                MessageBox.Show(
                    $"Hiba az adatok betöltésekor:\n{ex.Message}",
                    "Adatbázis hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// A DataGrid kiválasztás-változás eseménykezelője.
        /// Amikor a felhasználó kiválaszt egy sort a táblázatban,
        /// az autó adatai automatikusan megjelennek az űrlap mezőiben.
        /// Ez lehetővé teszi a kiválasztott autó adatainak módosítását.
        /// </summary>
        private void AutokDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ellenőrizzük, hogy van-e kiválasztott elem és az Auto típusú-e
            if (autokDataGrid.SelectedItem is Auto kivalasztottAuto)
            {
                // Az űrlap mezőinek feltöltése a kiválasztott autó adataival
                rendszamTextBox.Text = kivalasztottAuto.Rendszam;
                markaTextBox.Text = kivalasztottAuto.Marka;
                tipusTextBox.Text = kivalasztottAuto.Tipus;
                evjaratTextBox.Text = kivalasztottAuto.Evjarat.ToString();
                napiArTextBox.Text = kivalasztottAuto.NapiAr.ToString();
                elerhetoCheckBox.IsChecked = kivalasztottAuto.Elerheto;
            }
        }

        /// <summary>
        /// Az adatbeviteli mezők ellenőrzése (validáció).
        /// Ellenőrzi, hogy minden kötelező mező ki van-e töltve,
        /// és az értékek megfelelő formátumúak-e.
        /// Hibás adat esetén figyelmeztető üzenetet jelenít meg
        /// és a hibás mezőre állítja a fókuszt.
        /// </summary>
        /// <returns>true, ha minden adat érvényes; false, ha van hiba.</returns>
        private bool AdatokEllenorzese()
        {
            // Rendszám: kötelező mező
            if (string.IsNullOrWhiteSpace(rendszamTextBox.Text))
            {
                MessageBox.Show("A rendszám megadása kötelező!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                rendszamTextBox.Focus();
                return false;
            }

            // Márka: kötelező mező
            if (string.IsNullOrWhiteSpace(markaTextBox.Text))
            {
                MessageBox.Show("A márka megadása kötelező!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                markaTextBox.Focus();
                return false;
            }

            // Típus: kötelező mező
            if (string.IsNullOrWhiteSpace(tipusTextBox.Text))
            {
                MessageBox.Show("A típus megadása kötelező!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                tipusTextBox.Focus();
                return false;
            }

            // Évjárat: kötelező, egész szám, ésszerű tartomány (1950 - jövő év)
            if (!int.TryParse(evjaratTextBox.Text, out int evjarat) ||
                evjarat < 1950 || evjarat > DateTime.Now.Year + 1)
            {
                MessageBox.Show(
                    $"Érvényes évjáratot adjon meg (1950 - {DateTime.Now.Year + 1})!",
                    "Érvénytelen adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                evjaratTextBox.Focus();
                return false;
            }

            // Napi ár: kötelező, pozitív egész szám
            if (!int.TryParse(napiArTextBox.Text, out int napiAr) || napiAr <= 0)
            {
                MessageBox.Show("A napi ár pozitív egész szám kell legyen!",
                    "Érvénytelen adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                napiArTextBox.Focus();
                return false;
            }

            // Minden ellenőrzés sikeres
            return true;
        }

        /// <summary>
        /// Az űrlap mezőiből Auto objektum létrehozása.
        /// A szöveges mezők értékeit a megfelelő típusra alakítja
        /// és egy új Auto objektumba csomagolja.
        /// </summary>
        /// <returns>Az űrlap adataiból létrehozott Auto objektum.</returns>
        private Auto UrlapbolAuto()
        {
            return new Auto
            {
                Rendszam = rendszamTextBox.Text.Trim(),
                Marka = markaTextBox.Text.Trim(),
                Tipus = tipusTextBox.Text.Trim(),
                Evjarat = int.Parse(evjaratTextBox.Text),
                NapiAr = int.Parse(napiArTextBox.Text),
                Elerheto = elerhetoCheckBox.IsChecked ?? true
            };
        }

        /// <summary>
        /// Az "Új hozzáadása" gomb eseménykezelője.
        /// Ellenőrzi az adatokat, majd hozzáadja az új autót az adatbázishoz.
        /// Sikeres hozzáadás után frissíti a táblázatot és törli a mezőket.
        /// </summary>
        private void HozzaadasGomb_Click(object sender, RoutedEventArgs e)
        {
            // Először ellenőrizzük az adatok helyességét
            if (!AdatokEllenorzese()) return;

            try
            {
                // Új Auto objektum létrehozása az űrlap adataiból
                var ujAuto = UrlapbolAuto();

                // Az autó mentése az adatbázisba
                AdatbazisKezelo.AutoHozzaadasa(ujAuto);

                // A táblázat frissítése és a mezők törlése
                AdatokBetoltese();
                MezokTorlese();

                MessageBox.Show("Az autó sikeresen hozzáadva!",
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
        /// A DataGrid-ben kiválasztott autó adatait módosítja
        /// az űrlapon megadott értékekkel.
        /// </summary>
        private void ModositasGomb_Click(object sender, RoutedEventArgs e)
        {
            // Ellenőrizzük, hogy van-e kiválasztott autó
            if (autokDataGrid.SelectedItem is not Auto kivalasztottAuto)
            {
                MessageBox.Show("Válasszon ki egy autót a táblázatból a módosításhoz!",
                    "Nincs kiválasztva", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ellenőrizzük az űrlap adatait
            if (!AdatokEllenorzese()) return;

            try
            {
                // A módosított adatokkal rendelkező Auto objektum létrehozása
                var modositottAuto = UrlapbolAuto();
                // Az eredeti azonosító megőrzése (ez alapján azonosítjuk a rekordot)
                modositottAuto.AutoId = kivalasztottAuto.AutoId;

                // A módosítás mentése az adatbázisba
                AdatbazisKezelo.AutoModositasa(modositottAuto);

                // A táblázat frissítése
                AdatokBetoltese();

                MessageBox.Show("Az autó adatai sikeresen módosítva!",
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
        /// Megerősítés után törli a kiválasztott autót az adatbázisból.
        /// Ha az autóhoz kölcsönzés tartozik, a törlés sikertelen lesz
        /// (idegen kulcs megszorítás).
        /// </summary>
        private void TorlesGomb_Click(object sender, RoutedEventArgs e)
        {
            // Ellenőrizzük, hogy van-e kiválasztott autó
            if (autokDataGrid.SelectedItem is not Auto kivalasztottAuto)
            {
                MessageBox.Show("Válasszon ki egy autót a táblázatból a törléshez!",
                    "Nincs kiválasztva", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Megerősítő kérdés megjelenítése
            var eredmeny = MessageBox.Show(
                $"Biztosan törölni szeretné a következő autót?\n\n{kivalasztottAuto}",
                "Törlés megerősítése",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (eredmeny == MessageBoxResult.Yes)
            {
                try
                {
                    // Az autó törlése az adatbázisból
                    AdatbazisKezelo.AutoTorlese(kivalasztottAuto.AutoId);

                    // A táblázat frissítése és a mezők törlése
                    AdatokBetoltese();
                    MezokTorlese();

                    MessageBox.Show("Az autó sikeresen törölve!",
                        "Sikeres művelet", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Hiba a törléskor:\n{ex.Message}\n\n" +
                        "Megjegyzés: Ha az autóhoz kölcsönzés tartozik, először azt kell törölni.",
                        "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// A "Mezők törlése" gomb eseménykezelője.
        /// Törli az összes beviteli mező tartalmát és
        /// megszünteti a DataGrid kijelölését.
        /// </summary>
        private void MezokTorleseGomb_Click(object sender, RoutedEventArgs e)
        {
            MezokTorlese();
        }

        /// <summary>
        /// Segédmetódus: törli az összes beviteli mező tartalmát
        /// és visszaállítja az alapértelmezett értékeket.
        /// </summary>
        private void MezokTorlese()
        {
            rendszamTextBox.Clear();
            markaTextBox.Clear();
            tipusTextBox.Clear();
            evjaratTextBox.Clear();
            napiArTextBox.Clear();
            elerhetoCheckBox.IsChecked = true;
            autokDataGrid.SelectedItem = null;
        }

        /// <summary>
        /// A "Keresés" gomb eseménykezelője.
        /// A kiválasztott mező és a megadott keresőszöveg alapján
        /// szűri az autókat. A szűrt eredmények a DataGrid-ben jelennek meg.
        /// </summary>
        private void KeresesGomb_Click(object sender, RoutedEventArgs e)
        {
            // Ellenőrizzük, hogy van-e keresőszöveg megadva
            if (string.IsNullOrWhiteSpace(keresoszovegTextBox.Text))
            {
                MessageBox.Show("Adjon meg keresőszöveget!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                keresoszovegTextBox.Focus();
                return;
            }

            try
            {
                // A kiválasztott keresési mező Tag értékének kiolvasása
                var kivalasztottElem = keresesiMezoComboBox.SelectedItem as ComboBoxItem;
                string mezo = kivalasztottElem?.Tag?.ToString() ?? "Rendszam";

                // A keresés végrehajtása és az eredmények megjelenítése
                autokDataGrid.ItemsSource = AdatbazisKezelo.AutokKeresese(
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
        /// Törli a keresőszöveget és újratölti az összes autó adatát,
        /// visszaállítva a szűrés előtti állapotot.
        /// </summary>
        private void OsszesGomb_Click(object sender, RoutedEventArgs e)
        {
            keresoszovegTextBox.Clear();
            AdatokBetoltese();
        }
    }
}
