# Scripts

Acest folder contine codul propriu al jocului. Structura este modulara: fiecare folder reprezinta o responsabilitate clara.

## Module

- `Core`: utilitare runtime generale, flags, persistenta usoara, blocari de input/interactiune.
- `Interaction`: hotspot-uri, click targets, trigger-e de scena si feedback pentru interactiuni.
- `Inventory`: sisteme simple de inventar si obiecte colectabile.
- `Presentation`: efecte vizuale, parallax, typewriter, scaling si helpers vizuali.
- `Puzzles`: logica pentru puzzle-uri concrete.
- `Dialogue`: baza sistemului de dialog si efecte de alegere.
- `NPC_AI`: modelul modular pentru decizii NPC.
- `NarrativeV2`: progres narativ, momente, rute si sistemul Barista Welcome.
- `StatsUI`: stats globale si HUD-ul lor.
- `Phone`, `Jurnal`, `MainMenu`, `BarOutside`, `Bar_Inside`, `Anticariat`: sisteme sau zone deja delimitate.
- `_Archive`: cod vechi sau experimental care nu trebuie folosit ca sursa pentru sisteme noi.

## Regula de adaugare

Daca un script este reutilizabil intre scene, pune-l intr-un modul functional (`Core`, `Interaction`, `NPC_AI`, etc.). Daca este specific unui loc sau capitol, pune-l in folderul acelei zone.
