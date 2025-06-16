# MauiChatApp

## 🗨️ Projekt leírása

A **MauiChatApp** egy valós idejű, többfelhasználós csevegőalkalmazás .NET MAUI keretrendszerrel, amely a **SignalR** technológiát használja kommunikációra. A projekt célja, hogy bemutassa, hogyan valósítható meg egy platformfüggetlen chatalkalmazás modern .NET környezetben, adminisztrációs felülettel és szerveroldali vezérléssel.

A Solution három fő komponensből áll:

- **MauiChatApp** – Felhasználói csevegőalkalmazás.
- **MauiAdminApp** – Adminisztrációs felület, ahol szobák hozhatók létre, felhasználók figyelhetők és moderálhatók.
- **ChatServer** – ASP.NET Core SignalR-alapú háttérrendszer.

---

## ⚙️ Funkciók

### Általános funkciók:
- Szobák létrehozása és törlése
- Felhasználók csatlakozása szobákhoz
- Üzenetek valós idejű küldése és fogadása
- Képek és élő videókeretek küldése

### Admin funkciók:
- Szobák kezelése (létrehozás, törlés)
- Felhasználók kilistázása szobánként
- Felhasználók kirúgása a szobákból
- Automatikus 5 másodpercenkénti frissítés a szobákhoz tartozó üzenetekről és felhasználókról

---

## 🏗️ Technológiák

- **.NET 7 / 8**
- **.NET MAUI**
- **ASP.NET Core SignalR**
- **XAML** (UI)
- **MVVM** architektúra
- **C#**
- **Entity-less memóriaalapú adatkezelés**

---

## 🧑‍💻 Használat fejlesztőként

### 1. Klónozás

```bash
git clone https://github.com/oroszcsabi26/MauiChatApp_2.git
```

### 2. Projekt megnyitása

Nyisd meg Visual Studio-ban (ajánlott: 2022 vagy újabb), és állítsd be a következőket:

- Indítsd a `ChatServer` projektet elsőként.
- Indítsd el párhuzamosan a `MauiChatApp` és/vagy `MauiAdminApp` klienseket.

### 3. Csatlakozás a szerverhez

Alapértelmezett SignalR szervercím:  
```
http://localhost:5000/chatHub
```

Ezt a címet szükség esetén módosítani kell a MAUI alkalmazások konfigurációs fájljaiban.

---

## 🧪 Fejlesztési jellemzők

- Az üzenetek időbélyeggel (`Timestamp`) kerülnek mentésre a szerveroldalon.
- Az admin felületen csak a legutóbbi frissítés óta érkezett üzenetek jelennek meg.
- A felhasználók és üzenetek szinkronizálása időzítéssel történik.
- Minden logika MVVM mintára épül.

---

## 📄 Licenc

Ez a projekt szabadon felhasználható, oktatási vagy saját tanulási célokra.

---

## 📫 Kapcsolat

Fejlesztő: **Orosz Csaba**  
GitHub: [@oroszcsabi26](https://github.com/oroszcsabi26)

