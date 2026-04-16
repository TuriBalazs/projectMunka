using System;

namespace AutoKolcsonzo.Modellek
{
    /// <summary>
    /// A kölcsönzés entitást reprezentáló modell osztály.
    /// Ez az osztály tárolja az egyes kölcsönzések adatait, beleértve
    /// a kölcsönzött autó és az ügyfél azonosítóját (idegen kulcsok).
    /// A Kolcsonzesek tábla az Autok és Ugyfelek táblákhoz kapcsolódik.
    /// </summary>
    public class Kolcsonzes
    {
        /// <summary>
        /// A kölcsönzés egyedi azonosítója (elsődleges kulcs, automatikusan generált).
        /// </summary>
        public int KolcsonzesId { get; set; }

        /// <summary>
        /// A kölcsönzött autó azonosítója (idegen kulcs az Autok táblához).
        /// </summary>
        public int AutoId { get; set; }

        /// <summary>
        /// A kölcsönző ügyfél azonosítója (idegen kulcs az Ugyfelek táblához).
        /// </summary>
        public int UgyfelId { get; set; }

        /// <summary>
        /// A kölcsönzés kezdő dátuma.
        /// </summary>
        public DateTime KezdoDatum { get; set; }

        /// <summary>
        /// A kölcsönzés vég dátuma (tervezett visszahozatal).
        /// </summary>
        public DateTime VegDatum { get; set; }

        /// <summary>
        /// A kölcsönzés teljes összege forintban.
        /// Kiszámítása: napok száma * autó napi ára.
        /// </summary>
        public int Osszeg { get; set; }

        /// <summary>
        /// A kölcsönzés állapota: "Aktív" (folyamatban) vagy "Lezárt" (befejezett).
        /// </summary>
        public string Allapot { get; set; } = string.Empty;

        /// <summary>
        /// Az autó megjelenítendő neve (márka + típus + rendszám).
        /// Nem adatbázis mező, csak megjelenítési célra szolgál (JOIN-ból származik).
        /// </summary>
        public string AutoNev { get; set; } = string.Empty;

        /// <summary>
        /// Az ügyfél megjelenítendő neve.
        /// Nem adatbázis mező, csak megjelenítési célra szolgál (JOIN-ból származik).
        /// </summary>
        public string UgyfelNev { get; set; } = string.Empty;
    }
}
