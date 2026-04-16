namespace AutoKolcsonzo.Modellek
{
    /// <summary>
    /// Az ügyfél entitást reprezentáló modell osztály.
    /// Ez az osztály tárolja az egyes ügyfelek személyes adatait,
    /// amelyeket az adatbázis Ugyfelek táblájából olvasunk be.
    /// Az ügyfelek a kölcsönzésekhez kapcsolódnak idegen kulcson keresztül.
    /// </summary>
    public class Ugyfel
    {
        /// <summary>
        /// Az ügyfél egyedi azonosítója (elsődleges kulcs, automatikusan generált).
        /// </summary>
        public int UgyfelId { get; set; }

        /// <summary>
        /// Az ügyfél teljes neve.
        /// </summary>
        public string Nev { get; set; } = string.Empty;

        /// <summary>
        /// Az ügyfél e-mail címe (pl. "pelda@email.hu").
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Az ügyfél telefonszáma (pl. "+36-30-123-4567").
        /// </summary>
        public string Telefonszam { get; set; } = string.Empty;

        /// <summary>
        /// Az ügyfél személyi igazolvány száma (egyedi azonosító).
        /// </summary>
        public string SzemelyiIgazolvanySzam { get; set; } = string.Empty;

        /// <summary>
        /// Az ügyfél szöveges megjelenítése (név és személyi ig. szám).
        /// A ComboBox-okban és egyéb vezérlőkben jelenik meg.
        /// </summary>
        public override string ToString()
        {
            return $"{Nev} ({SzemelyiIgazolvanySzam})";
        }
    }
}
