# Prefabs

Contine prefabs reutilizabile pentru UI, gameplay si sisteme.

## Organizare

- Prefabs generale sau istorice pot ramane la radacina pana sunt migrate.
- Prefabs de telefon intra in `Phone`.
- Prefabs de decizii intra in `Decisions`.
- Prefabs de inventar intra in `Inventory`.
- Prefabs de NPC intra in `NPC`.

Daca adaugi o categorie noua de prefabs, creeaza un folder cu nume clar.

## Barista

Barista din scena `Assets/Scenes/Bar_Interior.unity` poate fi transformat in prefab cu:

`Tools/ROAE/NPC/Create Or Update Barista Prefab From Scene`

Prefab-ul rezultat este salvat la `Assets/ROAE_2/Prefabs/NPC/Barista.prefab`. Utilitarul lasa `NpcMomentRouter.dialogueManager` neasignat in prefab, pentru ca este referinta de scena si se rezolva automat la runtime.
