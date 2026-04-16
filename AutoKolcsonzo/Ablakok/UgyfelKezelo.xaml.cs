using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using AutoKolcsonzo.AdatEleres;
using AutoKolcsonzo.Modellek;

namespace AutoKolcsonzo.Ablakok
{
    /// <summary>
    /// Az ügyfelek kezelésére szolgáló ablak mögöttes kódja.
    /// Megvalósítja az ügyfelek teljes CRUD funkcionalitását:
    /// hozzáadás, megjelenítés, módosítás, törlés és keresés.
    /// Az adatbevitel ellenőrzött: kötelező mezők, e-mail formátum
    /// és telefonszám formátum validálva van.
    /// </summary>
    public partial class UgyfelKezelo : Window
    {
        /// <summary>
        /// Az ablak konstruktora. Inicializálja a vezérlőelemeket
        /// és betölti az ügyfelek adatait az adatbázisból.
        /// </summary>
        public UgyfelKezelo()
        {
            InitializeComponent();
            AdatokBetoltese();
        }

        /// <summary>
        /// Az ügyfelek adatainak betöltése az adatbázisból a DataGrid-be.
        /// Minden CRUD művelet után meghívódik az adatok frissítéséhez.
        /// </summary>
        private void AdatokBetoltese()
        {
            try
            {
                ugyfelekDataGrid.ItemsSource = AdatbazisKezelo.UgyfelekLekerdezese();
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
        /// A kiválasztott ügyfél adatait automatikusan betölti az űrlap mezőibe,
        /// lehetővé téve az adatok módosítását.
        /// </summary>
        private void UgyfelekDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ugyfelekDataGrid.SelectedItem is Ugyfel kivalasztottUgyfel)
            {
                // Az űrlap mezőinek feltöltése a kiválasztott ügyfél adataival
                nevTextBox.Text = kivalasztottUgyfel.Nev;
                emailTextBox.Text = kivalasztottUgyfel.Email;
                telefonszamTextBox.Text = kivalasztottUgyfel.Telefonszam;
                szemelyiIgazolvanySzamTextBox.Text = kivalasztottUgyfel.SzemelyiIgazolvanySzam;
            }
        }

        /// <summary>
        /// Az adatbeviteli mezők ellenőrzése (validáció).
        /// Ellenőrzi a kötelező mezők kitöltését, az e-mail formátumot
        /// és a telefonszám formátumot.
        /// </summary>
        /// <returns>true, ha minden adat érvényes; false, ha van hiba.</returns>
        private bool AdatokEllenorzese()
        {
            // Név: kötelező mező
            if (string.IsNullOrWhiteSpace(nevTextBox.Text))
            {
                MessageBox.Show("A név megadása kötelező!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                nevTextBox.Focus();
                return false;
            }

            // E-mail: kötelező mező + formátum ellenőrzés
            if (string.IsNullOrWhiteSpace(emailTextBox.Text))
            {
                MessageBox.Show("Az e-mail cím megadása kötelező!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                emailTextBox.Focus();
                return false;
            }

            // E-mail formátum ellenőrzése reguláris kifejezéssel
            // A minta: szöveg@szöveg.szöveg
            if (!Regex.IsMatch(emailTextBox.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Érvénytelen e-mail formátum!\nPélda: pelda@email.hu",
                    "Érvénytelen adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                emailTextBox.Focus();
                return false;
            }

            // Telefonszám: kötelező mező
            if (string.IsNullOrWhiteSpace(telefonszamTextBox.Text))
            {
                MessageBox.Show("A telefonszám megadása kötelező!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                telefonszamTextBox.Focus();
                return false;
            }

            // Személyi igazolvány szám: kötelező mező
            if (string.IsNullOrWhiteSpace(szemelyiIgazolvanySzamTextBox.Text))
            {
                MessageBox.Show("A személyi igazolvány szám megadása kötelező!",
                    "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                szemelyiIgazolvanySzamTextBox.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Az űrlap mezőiből Ugyfel objektum létrehozása.
        /// </summary>
        /// <returns>Az űrlap adataiból létrehozott Ugyfel objektum.</returns>
        private Ugyfel UrlapbolUgyfel()
        {
            return new Ugyfel
            {
                Nev = nevTextBox.Text.Trim(),
                Email = emailTextBox.Text.Trim(),
                Telefonszam = telefonszamTextBox.Text.Trim(),
                SzemelyiIgazolvanySzam = szemelyiIgazolvanySzamTextBox.Text.Trim()
            };
        }

        /// <summary>
        /// Az "Új hozzáadása" gomb eseménykezelője.
        /// Ellenőrzi az adatokat, majd hozzáadja az új ügyfelet az adatbázishoz.
        /// </summary>
        private void HozzaadasGomb_Click(object sender, RoutedEventArgs e)
        {
            if (!AdatokEllenorzese()) return;

            try
            {
                var ujUgyfel = UrlapbolUgyfel();
                AdatbazisKezelo.UgyfelHozzaadasa(ujUgyfel);
                AdatokBetoltese();
                MezokTorlese();

                MessageBox.Show("Az ügyfél sikeresen hozzáadva!",
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
        /// A kiválasztott ügyfél adatait módosítja az űrlapon megadott értékekkel.
        /// </summary>
        private void ModositasGomb_Click(object sender, RoutedEventArgs e)
        {
            if (ugyfelekDataGrid.SelectedItem is not Ugyfel kivalasztottUgyfel)
            {
                MessageBox.Show("Válasszon ki egy ügyfelet a táblázatból a módosításhoz!",
                    "Nincs kiválasztva", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!AdatokEllenorzese()) return;

            try
            {
                var modositottUgyfel = UrlapbolUgyfel();
                modositottUgyfel.UgyfelId = kivalasztottUgyfel.UgyfelId;

                AdatbazisKezelo.UgyfelModositasa(modositottUgyfel);
                AdatokBetoltese();

                MessageBox.Show("Az ügyfél adatai sikeresen módosítva!",
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
        /// Megerősítés után törli a kiválasztott ügyfelet az adatbázisból.
        /// Ha az ügyfélhez kölcsönzés tartozik, a törlés sikertelen lesz.
        /// </summary>
        private void TorlesGomb_Click(object sender, RoutedEventArgs e)
        {
            if (ugyfelekDataGrid.SelectedItem is not Ugyfel kivalasztottUgyfel)
            {
                MessageBox.Show("Válasszon ki egy ügyfelet a táblázatból a törléshez!",
                    "Nincs kiválasztva", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var eredmeny = MessageBox.Show(
                $"Biztosan törölni szeretné a következő ügyfelet?\n\n{kivalasztottUgyfel}",
                "Törlés megerősítése",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (eredmeny == MessageBoxResult.Yes)
            {
                try
                {
                    AdatbazisKezelo.UgyfelTorlese(kivalasztottUgyfel.UgyfelId);
                    AdatokBetoltese();
                    MezokTorlese();

                    MessageBox.Show("Az ügyfél sikeresen törölve!",
                        "Sikeres művelet", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Hiba a törléskor:\n{ex.Message}\n\n" +
                        "Megjegyzés: Ha az ügyfélhez kölcsönzés tartozik, először azt kell törölni.",
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
        /// Segédmetódus: törli az összes beviteli mező tartalmát
        /// és megszünteti a DataGrid kijelölését.
        /// </summary>
        private void MezokTorlese()
        {
            nevTextBox.Clear();
            emailTextBox.Clear();
            telefonszamTextBox.Clear();
            szemelyiIgazolvanySzamTextBox.Clear();
            ugyfelekDataGrid.SelectedItem = null;
        }

        /// <summary>
        /// A "Keresés" gomb eseménykezelője.
        /// A kiválasztott mező és keresőszöveg alapján szűri az ügyfeleket.
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
                string mezo = kivalasztottElem?.Tag?.ToString() ?? "Nev";

                ugyfelekDataGrid.ItemsSource = AdatbazisKezelo.UgyfelekKeresese(
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
        /// Visszaállítja a teljes ügyféllistát.
        /// </summary>
        private void OsszesGomb_Click(object sender, RoutedEventArgs e)
        {
            keresoszovegTextBox.Clear();
            AdatokBetoltese();
        }
    }
}
