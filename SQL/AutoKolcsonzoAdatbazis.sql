IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'AutoKolcsonzo')
BEGIN
    CREATE DATABASE AutoKolcsonzo;
    PRINT 'Az AutoKolcsonzo adatbázis sikeresen létrehozva.';
END
ELSE
BEGIN
    PRINT 'Az AutoKolcsonzo adatbázis már létezik.';
END
GO

USE AutoKolcsonzo;
GO

IF OBJECT_ID('dbo.Kolcsonzesek', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Kolcsonzesek;
    PRINT 'A Kolcsonzesek tábla törölve.';
END
GO

IF OBJECT_ID('dbo.Autok', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Autok;
    PRINT 'Az Autok tábla törölve.';
END
GO

IF OBJECT_ID('dbo.Ugyfelek', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Ugyfelek;
    PRINT 'Az Ugyfelek tábla törölve.';
END
GO

CREATE TABLE Autok (
    AutoId                  INT IDENTITY(1,1) PRIMARY KEY,  -- Egyedi azonosító, automatikusan növekvő
    Rendszam                NVARCHAR(10) NOT NULL UNIQUE,    -- Rendszám (egyedi, pl. ABC-123)
    Marka                   NVARCHAR(50) NOT NULL,           -- Autó márkája (pl. Toyota, BMW)
    Tipus                   NVARCHAR(50) NOT NULL,           -- Autó típusa (pl. Corolla, 320i)
    Evjarat                 INT NOT NULL,                    -- Gyártási év
    NapiAr                  INT NOT NULL,                    -- Napi bérleti díj forintban
    Elerheto                BIT NOT NULL DEFAULT 1           -- Elérhető-e (1=igen, 0=nem)
);
GO

PRINT 'Az Autok tábla sikeresen létrehozva.';
GO

CREATE TABLE Ugyfelek (
    UgyfelId                INT IDENTITY(1,1) PRIMARY KEY,       -- Egyedi azonosító, automatikusan növekvő
    Nev                     NVARCHAR(100) NOT NULL,               -- Ügyfél teljes neve
    Email                   NVARCHAR(100) NOT NULL,               -- E-mail cím
    Telefonszam             NVARCHAR(20) NOT NULL,                -- Telefonszám
    SzemelyiIgazolvanySzam  NVARCHAR(20) NOT NULL UNIQUE          -- Személyi ig. szám (egyedi)
);
GO

PRINT 'Az Ugyfelek tábla sikeresen létrehozva.';
GO

CREATE TABLE Kolcsonzesek (
    KolcsonzesId            INT IDENTITY(1,1) PRIMARY KEY,  -- Egyedi azonosító, automatikusan növekvő
    AutoId                  INT NOT NULL,                    -- Kölcsönzött autó azonosítója (idegen kulcs)
    UgyfelId                INT NOT NULL,                    -- Kölcsönző ügyfél azonosítója (idegen kulcs)
    KezdoDatum              DATE NOT NULL,                   -- Kölcsönzés kezdő dátuma
    VegDatum                DATE NOT NULL,                   -- Kölcsönzés vég dátuma
    Osszeg                  INT NOT NULL,                    -- Kölcsönzés teljes összege forintban
    Allapot                 NVARCHAR(20) NOT NULL DEFAULT N'Aktív',  -- Állapot: Aktív vagy Lezárt

    CONSTRAINT FK_Kolcsonzesek_Autok
        FOREIGN KEY (AutoId) REFERENCES Autok(AutoId),

    CONSTRAINT FK_Kolcsonzesek_Ugyfelek
        FOREIGN KEY (UgyfelId) REFERENCES Ugyfelek(UgyfelId)
);
GO

PRINT 'A Kolcsonzesek tábla sikeresen létrehozva.';
GO

INSERT INTO Autok (Rendszam, Marka, Tipus, Evjarat, NapiAr, Elerheto) VALUES
    (N'ABC-123', N'Toyota',      N'Corolla',   2020, 8000,  1),
    (N'DEF-456', N'BMW',         N'320i',      2021, 15000, 1),
    (N'GHI-789', N'Volkswagen',  N'Golf',      2019, 9000,  1),
    (N'JKL-012', N'Ford',        N'Focus',     2018, 7000,  0),  -- Nem elérhető (szervizben)
    (N'MNO-345', N'Audi',        N'A4',        2022, 18000, 1),
    (N'PQR-678', N'Opel',        N'Astra',     2017, 6000,  1),
    (N'STU-901', N'Mercedes',    N'C200',      2023, 22000, 1),
    (N'VWX-234', N'Skoda',       N'Octavia',   2020, 8500,  0),  -- Nem elérhető (kölcsönözve)
    (N'YZA-567', N'Renault',     N'Megane',    2019, 7500,  1),
    (N'BCD-890', N'Hyundai',     N'i30',       2021, 8000,  1);
GO

PRINT '10 autó mintaadat sikeresen beszúrva.';
GO

INSERT INTO Ugyfelek (Nev, Email, Telefonszam, SzemelyiIgazolvanySzam) VALUES
    (N'Kovács János',      N'kovacs.janos@email.hu',      N'+36-30-123-4567', N'123456AB'),
    (N'Nagy Mária',        N'nagy.maria@email.hu',        N'+36-20-234-5678', N'234567CD'),
    (N'Tóth Péter',        N'toth.peter@email.hu',        N'+36-70-345-6789', N'345678EF'),
    (N'Szabó Anna',        N'szabo.anna@email.hu',        N'+36-30-456-7890', N'456789GH'),
    (N'Horváth László',    N'horvath.laszlo@email.hu',    N'+36-20-567-8901', N'567890IJ'),
    (N'Varga Katalin',     N'varga.katalin@email.hu',     N'+36-70-678-9012', N'678901KL'),
    (N'Kiss Gábor',        N'kiss.gabor@email.hu',        N'+36-30-789-0123', N'789012MN'),
    (N'Molnár Eszter',     N'molnar.eszter@email.hu',     N'+36-20-890-1234', N'890123OP');
GO

PRINT '8 ügyfél mintaadat sikeresen beszúrva.';
GO

INSERT INTO Kolcsonzesek (AutoId, UgyfelId, KezdoDatum, VegDatum, Osszeg, Allapot) VALUES
    (1, 1, '2024-01-15', '2024-01-20', 40000,  N'Lezárt'),   -- Toyota Corolla, 5 nap * 8000 Ft
    (2, 2, '2024-02-01', '2024-02-05', 60000,  N'Lezárt'),   -- BMW 320i, 4 nap * 15000 Ft
    (3, 3, '2024-03-10', '2024-03-15', 45000,  N'Lezárt'),   -- VW Golf, 5 nap * 9000 Ft
    (5, 4, '2024-04-01', '2024-04-10', 162000, N'Lezárt'),   -- Audi A4, 9 nap * 18000 Ft
    (4, 5, '2024-05-20', '2024-05-25', 35000,  N'Lezárt'),   -- Ford Focus, 5 nap * 7000 Ft
    (7, 1, '2024-06-01', '2024-06-08', 154000, N'Aktív'),    -- Mercedes C200, 7 nap * 22000 Ft
    (8, 6, '2024-06-10', '2024-06-15', 42500,  N'Aktív'),    -- Skoda Octavia, 5 nap * 8500 Ft
    (1, 7, '2024-06-12', '2024-06-18', 48000,  N'Aktív'),    -- Toyota Corolla, 6 nap * 8000 Ft
    (9, 8, '2024-06-15', '2024-06-22', 52500,  N'Aktív'),    -- Renault Megane, 7 nap * 7500 Ft
    (10, 2, '2024-06-18', '2024-06-25', 56000, N'Aktív');    -- Hyundai i30, 7 nap * 8000 Ft
GO

PRINT '10 kölcsönzés mintaadat sikeresen beszúrva.';
GO

PRINT '';
PRINT '=== AUTÓK TARTALMA ===';
SELECT * FROM Autok;

PRINT '';
PRINT '=== ÜGYFELEK TARTALMA ===';
SELECT * FROM Ugyfelek;

PRINT '';
PRINT '=== KÖLCSÖNZÉSEK TARTALMA (részletes) ===';
SELECT
    k.KolcsonzesId,
    a.Marka + ' ' + a.Tipus + ' (' + a.Rendszam + ')' AS [Autó],
    u.Nev AS [Ügyfél],
    k.KezdoDatum AS [Kezdő dátum],
    k.VegDatum AS [Vég dátum],
    k.Osszeg AS [Összeg (Ft)],
    k.Allapot AS [Állapot]
FROM Kolcsonzesek k
INNER JOIN Autok a ON k.AutoId = a.AutoId
INNER JOIN Ugyfelek u ON k.UgyfelId = u.UgyfelId
ORDER BY k.KolcsonzesId;

PRINT '';
PRINT 'Az adatbázis létrehozása és feltöltése sikeresen befejeződött!';
GO
