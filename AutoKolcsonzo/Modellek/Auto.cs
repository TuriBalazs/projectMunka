namespace AutoKolcsonzo.Modellek
{
    /// <summary>
    /// Az autó entitást reprezentáló modell osztály.
    /// Ez az osztály tárolja az egyes autók adatait, amelyeket az adatbázis
    /// Autok táblájából olvasunk be, illetve oda írunk vissza.
    /// Az osztály tulajdonságai megfelelnek az adatbázis tábla oszlopainak.
    /// </summary>
    public class Auto
    {
        /// <summary>
        /// Az autó egyedi azonosítója (elsődleges kulcs, automatikusan generált).
        /// </summary>
        public int AutoId { get; set; }

        /// <summary>
        /// Az autó rendszáma (egyedi, pl. "ABC-123").
        /// </summary>
        public string Rendszam { get; set; } = string.Empty;

        /// <summary>
        /// Az autó márkája (pl. "Toyota", "BMW").
        /// </summary>
        public string Marka { get; set; } = string.Empty;

        /// <summary>
        /// Az autó típusa (pl. "Corolla", "320i").
        /// </summary>
        public string Tipus { get; set; } = string.Empty;

        /// <summary>
        /// Az autó gyártási éve.
        /// </summary>
        public int Evjarat { get; set; }

        /// <summary>
        /// Az autó napi bérleti díja forintban.
        /// </summary>
        public int NapiAr { get; set; }

        /// <summary>
        /// Jelzi, hogy az autó jelenleg elérhető-e kölcsönzésre.
        /// true = elérhető, false = nem elérhető (pl. kölcsönzés alatt vagy szervizben).
        /// </summary>
        public bool Elerheto { get; set; }

        /// <summary>
        /// Az autó szöveges megjelenítése (márka, típus és rendszám).
        /// A ComboBox-okban és egyéb vezérlőkben jelenik meg.
        /// </summary>
        public override string ToString()
        {
            return $"{Marka} {Tipus} ({Rendszam})";
        }
    }
}
