# ROAE_2

Acesta este spatiul principal pentru continutul propriu al jocului. Aici se afla codul, prefabs, datele narative si asset-urile create pentru ROAE.

Nu muta aici pachete externe precum `AdventureCreator` sau `TextMesh Pro`. Ele raman la radacina `Assets`, ca sa fie clar ce este third-party si ce este cod propriu.

## Foldere principale

- `Scripts`: cod runtime/editor propriu, organizat pe module.
- `Data`: configurari, dialogue assets, narrative assets si continut data-driven.
- `Prefabs`: prefabs reutilizabile pentru UI, interactiuni si gameplay.
- `Resources`: asset-uri incarcate prin mecanisme Unity Resources.
- `_Archive`: cod vechi, generatoare istorice sau experimente pastrate pentru referinta.
- `_GeneratedBackups`: copii generate automat; nu se construieste gameplay nou aici.

## Regula de organizare

Codul nou trebuie sa fie plasat dupa responsabilitate, nu dupa scena. De exemplu, o regula de decizie pentru NPC intra in `Scripts/NPC_AI`, iar un trigger de hotspot intra in `Scripts/Interaction`.
