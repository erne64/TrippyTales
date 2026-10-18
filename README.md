# 🧟 TrippyTales

Un videogioco 3D d'azione e sopravvivenza contro orde di zombie, sviluppato in **Unity** e **C#**.

Il giocatore deve sopravvivere a ondate crescenti di nemici, gestire con cura munizioni e salute, ed esplorare scenari interconnessi attraverso portali dimensionali fino al raggiungimento della vittoria finale.

---

## 👥 Autori (Development Team)

* **Ernesto Gligora**
* **Riccardo Lavorato**

---

## 📄 Documentazione di Progetto

Il **Game Design Document (GDD)** completo è consultabile direttamente nel repository:
👉 [**Leggi il Game Design Document (PDF)**](Docs/GDD.pdf)

---

## 🎮 Caratteristiche Principali (Features)

* **Sistema di Combattimento & Mira**: Visuale dinamica in terza persona con transizione fluida alla modalità mira (*Cinemachine*), sparo a raffica con raycast, rinculo e feedback d'impatto particellare.
* **Intelligenza Artificiale Nemica (NavMesh)**: Zombie con calcolo di inseguimento ottimizzato (*throttled pathfinding*), rilevamento distanza, animazioni di colpo subito e attacco in mischia con tolleranza d'impatto.
* **Sistema Livelli & Progressione Ondate**: Difficoltà scalare calibrata su 15 livelli progressivi (aumento progressivo di nemici e velocità) e attivazione di portali per la transizione tra scenari (*Quattromiglia, Piane Crati, Nicotera*).
* **Gestione Risorse**: Casse di munizioni interattive con indicatori fluttuanti e oggetti curativi con rigenerazione a tempo (*cooldown*).
* **Audio & Opzioni Persistenti**: Playlist musicale continua tra le scene (*DontDestroyOnLoad*), effetti sonori d'ambiente e passi casuali, gestione del volume in decibel tramite AudioMixer e salvataggio preferenze in *PlayerPrefs*.
* **Interfaccia Grafica Reattiva**: Health bar sfumata con gradiente dinamico, contatore colpi ottimizzato e menu di pausa con ripresa immediata.

---

## 🛠️ Tecnologie Utilizzate

* **Engine**: Unity 3D
* **Linguaggio**: C#
* **Input**: Unity New Input System (UnityEngine.InputSystem)
* **Cinematica & Camere**: Cinemachine Virtual Camera
* **Navigazione AI**: Unity NavMesh Agent
* **Audio**: Unity AudioMixer con curve logaritmiche

---

## 🕹️ Controlli di Gioco

| Comando | Azione |
| :--- | :--- |
| **W, A, S, D** | Movimento del personaggio |
| **Mouse (Movimento)** | Rotazione telecamera / Visuale |
| **Tasto Destro (Tieni premuto)** | Modalità Mira (Aim Camera) |
| **Tasto Sinistro** | Sparo |
| **Spazio** | Salto |
| **R** | Ricarica arma |
| **E** | Interazione (Raccogli caricatori dalla cassa) |
| **ESC** | Menu di Pausa |

---

## 🚀 Come Eseguire il Progetto

### Per Giocare (Build Standalone)

1. Vai nella sezione [**Releases**](https://github.com/erne64/TrippyTales/releases) del repository.
2. Scarica il file `.zip` dell'ultima versione.
3. Estrai la cartella e avvia `TrippyTales.exe`.

### Per lo Sviluppo (Unity Editor)

1. Clona il repository:

   ```bash
   git clone https://github.com/erne64/TrippyTales.git
   ```

2. Apri **Unity Hub** e aggiungi il progetto selezionando la cartella clonata.
3. Apri la scena `Assets/Scenes/MainMenu.unity` e premi **Play**.
