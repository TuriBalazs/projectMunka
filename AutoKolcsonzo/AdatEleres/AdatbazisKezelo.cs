using Microsoft.Data.SqlClient;
using AutoKolcsonzo.Modellek;
using System;
using System.Collections.Generic;

namespace AutoKolcsonzo.AdatEleres
{
    /// Központi adatbázis-kezelő statikus osztály.
    /// Ez az osztály felelős az összes adatbázis-művelet (CRUD) végrehajtásáért
    /// az MS SQL Server Express adatbázison. Minden entitáshoz (Autók, Ügyfelek,
    /// Kölcsönzések) biztosít lekérdezési, hozzáadási, módosítási, törlési
    /// és keresési metódusokat. Paraméterezett SQL lekérdezéseket használ
    /// az SQL injection támadások megelőzése érdekében.
    public static class AdatbazisKezelo
    {
        /// Kapcsolati karakterlánc az MS SQL Server Express adatbázishoz.
        /// A szerver: .\SQLEXPRESS (helyi SQL Server Express példány).
        /// Az adatbázis: AutoKolcsonzo.
        /// Windows hitelesítést használ (Trusted_Connection=True).
        private static readonly string KapcsolatiKarakterlanc =
            @"Server=.\SQLEXPRESS;Database=AutoKolcsonzo;Trusted_Connection=True;TrustServerCertificate=True;";

        // =====================================================================
        // AUTÓK CRUD MŰVELETEK
        // =====================================================================

        #region Autók CRUD műveletek

        /// Az összes autó lekérdezése az adatbázisból.
        /// Az eredmény AutoId szerint növekvő sorrendben van rendezve.
        /// Minden autó adatát beolvassa és Auto objektumokká alakítja.
        public static List<Auto> AutokLekerdezese()
        {
            // Üres lista létrehozása az eredmények tárolásához
            var autok = new List<Auto>();

            // Adatbázis-kapcsolat megnyitása using blokkban (automatikus lezárás)
            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                // SQL parancs összeállítása: az összes autó lekérdezése
                var parancs = new SqlCommand(
                    "SELECT AutoId, Rendszam, Marka, Tipus, Evjarat, NapiAr, Elerheto " +
                    "FROM Autok ORDER BY AutoId", kapcsolat);

                // Az eredmények beolvasása SqlDataReader segítségével
                using (var olvaso = parancs.ExecuteReader())
                {
                    while (olvaso.Read())
                    {
                        // Minden sor adataiból új Auto objektum létrehozása
                        autok.Add(new Auto
                        {
                            AutoId = olvaso.GetInt32(0),
                            Rendszam = olvaso.GetString(1),
                            Marka = olvaso.GetString(2),
                            Tipus = olvaso.GetString(3),
                            Evjarat = olvaso.GetInt32(4),
                            NapiAr = olvaso.GetInt32(5),
                            Elerheto = olvaso.GetBoolean(6)
                        });
                    }
                }
            }

            return autok;
        }

        /// Új autó hozzáadása az adatbázishoz.
        /// A metódus paraméterezett SQL INSERT parancsot használ
        /// az adatok biztonságos beszúrásához.
        public static void AutoHozzaadasa(Auto auto)
        {
            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                // Paraméterezett INSERT parancs az SQL injection elkerülése érdekében
                var parancs = new SqlCommand(
                    @"INSERT INTO Autok (Rendszam, Marka, Tipus, Evjarat, NapiAr, Elerheto)
                      VALUES (@Rendszam, @Marka, @Tipus, @Evjarat, @NapiAr, @Elerheto)", kapcsolat);

                // Paraméterek hozzáadása a parancshoz
                parancs.Parameters.AddWithValue("@Rendszam", auto.Rendszam);
                parancs.Parameters.AddWithValue("@Marka", auto.Marka);
                parancs.Parameters.AddWithValue("@Tipus", auto.Tipus);
                parancs.Parameters.AddWithValue("@Evjarat", auto.Evjarat);
                parancs.Parameters.AddWithValue("@NapiAr", auto.NapiAr);
                parancs.Parameters.AddWithValue("@Elerheto", auto.Elerheto);

                // A parancs végrehajtása (nem ad vissza eredményhalmazt)
                parancs.ExecuteNonQuery();
            }
        }

        /// Meglévő autó adatainak módosítása az adatbázisban.
        /// Az AutoId alapján azonosítja a módosítandó rekordot.
        public static void AutoModositasa(Auto auto)
        {
            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                // Paraméterezett UPDATE parancs
                var parancs = new SqlCommand(
                    @"UPDATE Autok SET Rendszam=@Rendszam, Marka=@Marka, Tipus=@Tipus,
                      Evjarat=@Evjarat, NapiAr=@NapiAr, Elerheto=@Elerheto
                      WHERE AutoId=@AutoId", kapcsolat);

                // Az összes paraméter megadása, beleértve az azonosítót
                parancs.Parameters.AddWithValue("@AutoId", auto.AutoId);
                parancs.Parameters.AddWithValue("@Rendszam", auto.Rendszam);
                parancs.Parameters.AddWithValue("@Marka", auto.Marka);
                parancs.Parameters.AddWithValue("@Tipus", auto.Tipus);
                parancs.Parameters.AddWithValue("@Evjarat", auto.Evjarat);
                parancs.Parameters.AddWithValue("@NapiAr", auto.NapiAr);
                parancs.Parameters.AddWithValue("@Elerheto", auto.Elerheto);

                parancs.ExecuteNonQuery();
            }
        }

        /// Autó törlése az adatbázisból az azonosító alapján.
        /// Figyelem: ha a törlendő autóhoz kölcsönzés tartozik,
        /// az idegen kulcs megszorítás miatt a törlés sikertelen lesz.
        public static void AutoTorlese(int autoId)
        {
            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                // Paraméterezett DELETE parancs
                var parancs = new SqlCommand(
                    "DELETE FROM Autok WHERE AutoId=@AutoId", kapcsolat);
                parancs.Parameters.AddWithValue("@AutoId", autoId);

                parancs.ExecuteNonQuery();
            }
        }

        /// Autók keresése a megadott mező és keresőszöveg alapján.
        /// A keresés LIKE operátorral történik (részleges egyezés).
        /// Az engedélyezett mezőnevek fehérlistával vannak védve
        /// az SQL injection ellen.
        public static List<Auto> AutokKeresese(string mezo, string keresoszoveg)
        {
            var autok = new List<Auto>();

            // Engedélyezett mezőnevek fehérlistája (SQL injection védelem)
            var engedelyezettMezok = new HashSet<string> { "Rendszam", "Marka", "Tipus" };
            if (!engedelyezettMezok.Contains(mezo)) return autok;

            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                // LIKE operátoros keresés: a keresőszöveg bármely pozícióban előfordulhat
                var parancs = new SqlCommand(
                    $"SELECT AutoId, Rendszam, Marka, Tipus, Evjarat, NapiAr, Elerheto " +
                    $"FROM Autok WHERE {mezo} LIKE @Keresoszoveg ORDER BY AutoId", kapcsolat);
                parancs.Parameters.AddWithValue("@Keresoszoveg", $"%{keresoszoveg}%");

                using (var olvaso = parancs.ExecuteReader())
                {
                    while (olvaso.Read())
                    {
                        autok.Add(new Auto
                        {
                            AutoId = olvaso.GetInt32(0),
                            Rendszam = olvaso.GetString(1),
                            Marka = olvaso.GetString(2),
                            Tipus = olvaso.GetString(3),
                            Evjarat = olvaso.GetInt32(4),
                            NapiAr = olvaso.GetInt32(5),
                            Elerheto = olvaso.GetBoolean(6)
                        });
                    }
                }
            }

            return autok;
        }

        #endregion

        // =====================================================================
        // ÜGYFELEK CRUD MŰVELETEK
        // =====================================================================

        #region Ügyfelek CRUD műveletek

        /// Az összes ügyfél lekérdezése az adatbázisból.
        /// Az eredmény UgyfelId szerint növekvő sorrendben van rendezve.
        public static List<Ugyfel> UgyfelekLekerdezese()
        {
            var ugyfelek = new List<Ugyfel>();

            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                var parancs = new SqlCommand(
                    "SELECT UgyfelId, Nev, Email, Telefonszam, SzemelyiIgazolvanySzam " +
                    "FROM Ugyfelek ORDER BY UgyfelId", kapcsolat);

                using (var olvaso = parancs.ExecuteReader())
                {
                    while (olvaso.Read())
                    {
                        ugyfelek.Add(new Ugyfel
                        {
                            UgyfelId = olvaso.GetInt32(0),
                            Nev = olvaso.GetString(1),
                            Email = olvaso.GetString(2),
                            Telefonszam = olvaso.GetString(3),
                            SzemelyiIgazolvanySzam = olvaso.GetString(4)
                        });
                    }
                }
            }

            return ugyfelek;
        }

        /// Új ügyfél hozzáadása az adatbázishoz.
        public static void UgyfelHozzaadasa(Ugyfel ugyfel)
        {
            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                var parancs = new SqlCommand(
                    @"INSERT INTO Ugyfelek (Nev, Email, Telefonszam, SzemelyiIgazolvanySzam)
                      VALUES (@Nev, @Email, @Telefonszam, @SzemelyiIgazolvanySzam)", kapcsolat);

                parancs.Parameters.AddWithValue("@Nev", ugyfel.Nev);
                parancs.Parameters.AddWithValue("@Email", ugyfel.Email);
                parancs.Parameters.AddWithValue("@Telefonszam", ugyfel.Telefonszam);
                parancs.Parameters.AddWithValue("@SzemelyiIgazolvanySzam", ugyfel.SzemelyiIgazolvanySzam);

                parancs.ExecuteNonQuery();
            }
        }

        /// Meglévő ügyfél adatainak módosítása az adatbázisban.
        public static void UgyfelModositasa(Ugyfel ugyfel)
        {
            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                var parancs = new SqlCommand(
                    @"UPDATE Ugyfelek SET Nev=@Nev, Email=@Email, Telefonszam=@Telefonszam,
                      SzemelyiIgazolvanySzam=@SzemelyiIgazolvanySzam
                      WHERE UgyfelId=@UgyfelId", kapcsolat);

                parancs.Parameters.AddWithValue("@UgyfelId", ugyfel.UgyfelId);
                parancs.Parameters.AddWithValue("@Nev", ugyfel.Nev);
                parancs.Parameters.AddWithValue("@Email", ugyfel.Email);
                parancs.Parameters.AddWithValue("@Telefonszam", ugyfel.Telefonszam);
                parancs.Parameters.AddWithValue("@SzemelyiIgazolvanySzam", ugyfel.SzemelyiIgazolvanySzam);

                parancs.ExecuteNonQuery();
            }
        }

        /// Ügyfél törlése az adatbázisból az azonosító alapján.
        /// Figyelem: ha az ügyfélhez kölcsönzés tartozik,
        /// az idegen kulcs megszorítás miatt a törlés sikertelen lesz.
        public static void UgyfelTorlese(int ugyfelId)
        {
            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                var parancs = new SqlCommand(
                    "DELETE FROM Ugyfelek WHERE UgyfelId=@UgyfelId", kapcsolat);
                parancs.Parameters.AddWithValue("@UgyfelId", ugyfelId);

                parancs.ExecuteNonQuery();
            }
        }

        /// Ügyfelek keresése a megadott mező és keresőszöveg alapján.
        /// A keresés LIKE operátorral történik (részleges egyezés).
        public static List<Ugyfel> UgyfelekKeresese(string mezo, string keresoszoveg)
        {
            var ugyfelek = new List<Ugyfel>();

            // Engedélyezett mezőnevek fehérlistája (SQL injection védelem)
            var engedelyezettMezok = new HashSet<string> { "Nev", "Email", "Telefonszam", "SzemelyiIgazolvanySzam" };
            if (!engedelyezettMezok.Contains(mezo)) return ugyfelek;

            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                var parancs = new SqlCommand(
                    $"SELECT UgyfelId, Nev, Email, Telefonszam, SzemelyiIgazolvanySzam " +
                    $"FROM Ugyfelek WHERE {mezo} LIKE @Keresoszoveg ORDER BY UgyfelId", kapcsolat);
                parancs.Parameters.AddWithValue("@Keresoszoveg", $"%{keresoszoveg}%");

                using (var olvaso = parancs.ExecuteReader())
                {
                    while (olvaso.Read())
                    {
                        ugyfelek.Add(new Ugyfel
                        {
                            UgyfelId = olvaso.GetInt32(0),
                            Nev = olvaso.GetString(1),
                            Email = olvaso.GetString(2),
                            Telefonszam = olvaso.GetString(3),
                            SzemelyiIgazolvanySzam = olvaso.GetString(4)
                        });
                    }
                }
            }

            return ugyfelek;
        }

        #endregion

        // =====================================================================
        // KÖLCSÖNZÉSEK CRUD MŰVELETEK
        // =====================================================================

        #region Kölcsönzések CRUD műveletek

        /// Az összes kölcsönzés lekérdezése az adatbázisból.
        /// INNER JOIN-t használ az Autok és Ugyfelek táblákkal,
        /// hogy az autó és ügyfél nevét is megjelenítse.
        public static List<Kolcsonzes> KolcsonzesekLekerdezese()
        {
            var kolcsonzesek = new List<Kolcsonzes>();

            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                // JOIN lekérdezés: az autó és ügyfél nevének megjelenítéséhez
                var parancs = new SqlCommand(
                    @"SELECT k.KolcsonzesId, k.AutoId, k.UgyfelId, k.KezdoDatum, k.VegDatum,
                             k.Osszeg, k.Allapot,
                             a.Marka + ' ' + a.Tipus + ' (' + a.Rendszam + ')' AS AutoNev,
                             u.Nev AS UgyfelNev
                      FROM Kolcsonzesek k
                      INNER JOIN Autok a ON k.AutoId = a.AutoId
                      INNER JOIN Ugyfelek u ON k.UgyfelId = u.UgyfelId
                      ORDER BY k.KolcsonzesId", kapcsolat);

                using (var olvaso = parancs.ExecuteReader())
                {
                    while (olvaso.Read())
                    {
                        kolcsonzesek.Add(new Kolcsonzes
                        {
                            KolcsonzesId = olvaso.GetInt32(0),
                            AutoId = olvaso.GetInt32(1),
                            UgyfelId = olvaso.GetInt32(2),
                            KezdoDatum = olvaso.GetDateTime(3),
                            VegDatum = olvaso.GetDateTime(4),
                            Osszeg = olvaso.GetInt32(5),
                            Allapot = olvaso.GetString(6),
                            AutoNev = olvaso.GetString(7),
                            UgyfelNev = olvaso.GetString(8)
                        });
                    }
                }
            }

            return kolcsonzesek;
        }

        /// Új kölcsönzés hozzáadása az adatbázishoz.
        /// A kölcsönzés az Autok és Ugyfelek táblákhoz kapcsolódik
        /// idegen kulcsokon (AutoId, UgyfelId) keresztül.
        public static void KolcsonzesHozzaadasa(Kolcsonzes kolcsonzes)
        {
            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                var parancs = new SqlCommand(
                    @"INSERT INTO Kolcsonzesek (AutoId, UgyfelId, KezdoDatum, VegDatum, Osszeg, Allapot)
                      VALUES (@AutoId, @UgyfelId, @KezdoDatum, @VegDatum, @Osszeg, @Allapot)", kapcsolat);

                parancs.Parameters.AddWithValue("@AutoId", kolcsonzes.AutoId);
                parancs.Parameters.AddWithValue("@UgyfelId", kolcsonzes.UgyfelId);
                parancs.Parameters.AddWithValue("@KezdoDatum", kolcsonzes.KezdoDatum);
                parancs.Parameters.AddWithValue("@VegDatum", kolcsonzes.VegDatum);
                parancs.Parameters.AddWithValue("@Osszeg", kolcsonzes.Osszeg);
                parancs.Parameters.AddWithValue("@Allapot", kolcsonzes.Allapot);

                parancs.ExecuteNonQuery();
            }
        }

        /// Meglévő kölcsönzés adatainak módosítása az adatbázisban.
        public static void KolcsonzesModositasa(Kolcsonzes kolcsonzes)
        {
            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                var parancs = new SqlCommand(
                    @"UPDATE Kolcsonzesek SET AutoId=@AutoId, UgyfelId=@UgyfelId,
                      KezdoDatum=@KezdoDatum, VegDatum=@VegDatum,
                      Osszeg=@Osszeg, Allapot=@Allapot
                      WHERE KolcsonzesId=@KolcsonzesId", kapcsolat);

                parancs.Parameters.AddWithValue("@KolcsonzesId", kolcsonzes.KolcsonzesId);
                parancs.Parameters.AddWithValue("@AutoId", kolcsonzes.AutoId);
                parancs.Parameters.AddWithValue("@UgyfelId", kolcsonzes.UgyfelId);
                parancs.Parameters.AddWithValue("@KezdoDatum", kolcsonzes.KezdoDatum);
                parancs.Parameters.AddWithValue("@VegDatum", kolcsonzes.VegDatum);
                parancs.Parameters.AddWithValue("@Osszeg", kolcsonzes.Osszeg);
                parancs.Parameters.AddWithValue("@Allapot", kolcsonzes.Allapot);

                parancs.ExecuteNonQuery();
            }
        }

        /// Kölcsönzés törlése az adatbázisból az azonosító alapján.
        public static void KolcsonzesTorlese(int kolcsonzesId)
        {
            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                var parancs = new SqlCommand(
                    "DELETE FROM Kolcsonzesek WHERE KolcsonzesId=@KolcsonzesId", kapcsolat);
                parancs.Parameters.AddWithValue("@KolcsonzesId", kolcsonzesId);

                parancs.ExecuteNonQuery();
            }
        }

        /// Kölcsönzések keresése a megadott mező és keresőszöveg alapján.
        /// Támogatott keresési mezők: autó neve, ügyfél neve, állapot.
        /// A keresés LIKE operátorral történik, JOIN-t használ a
        /// kapcsolódó táblák adatainak eléréséhez.
        public static List<Kolcsonzes> KolcsonzesekKeresese(string mezo, string keresoszoveg)
        {
            var kolcsonzesek = new List<Kolcsonzes>();

            // A szűrőfeltétel meghatározása a kiválasztott mező alapján
            string szuroFeltetel;
            switch (mezo)
            {
                case "AutoNev":
                    // Az autó teljes megnevezése alapján keres (márka + típus + rendszám)
                    szuroFeltetel = "(a.Marka + ' ' + a.Tipus + ' (' + a.Rendszam + ')') LIKE @Keresoszoveg";
                    break;
                case "UgyfelNev":
                    // Az ügyfél neve alapján keres
                    szuroFeltetel = "u.Nev LIKE @Keresoszoveg";
                    break;
                case "Allapot":
                    // A kölcsönzés állapota alapján keres (Aktív/Lezárt)
                    szuroFeltetel = "k.Allapot LIKE @Keresoszoveg";
                    break;
                default:
                    return kolcsonzesek;
            }

            using (var kapcsolat = new SqlConnection(KapcsolatiKarakterlanc))
            {
                kapcsolat.Open();

                var parancs = new SqlCommand(
                    $@"SELECT k.KolcsonzesId, k.AutoId, k.UgyfelId, k.KezdoDatum, k.VegDatum,
                              k.Osszeg, k.Allapot,
                              a.Marka + ' ' + a.Tipus + ' (' + a.Rendszam + ')' AS AutoNev,
                              u.Nev AS UgyfelNev
                       FROM Kolcsonzesek k
                       INNER JOIN Autok a ON k.AutoId = a.AutoId
                       INNER JOIN Ugyfelek u ON k.UgyfelId = u.UgyfelId
                       WHERE {szuroFeltetel}
                       ORDER BY k.KolcsonzesId", kapcsolat);
                parancs.Parameters.AddWithValue("@Keresoszoveg", $"%{keresoszoveg}%");

                using (var olvaso = parancs.ExecuteReader())
                {
                    while (olvaso.Read())
                    {
                        kolcsonzesek.Add(new Kolcsonzes
                        {
                            KolcsonzesId = olvaso.GetInt32(0),
                            AutoId = olvaso.GetInt32(1),
                            UgyfelId = olvaso.GetInt32(2),
                            KezdoDatum = olvaso.GetDateTime(3),
                            VegDatum = olvaso.GetDateTime(4),
                            Osszeg = olvaso.GetInt32(5),
                            Allapot = olvaso.GetString(6),
                            AutoNev = olvaso.GetString(7),
                            UgyfelNev = olvaso.GetString(8)
                        });
                    }
                }
            }

            return kolcsonzesek;
        }

        #endregion
    }
}
