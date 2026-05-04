# NPC_AI

Acest modul contine sistemul modular pentru decizii NPC.

## Flux conceptual

1. `NpcDecisionContext` citeste starea jocului: stats, relationship, scena, chapter, moment si flags.
2. `NpcDefinition` descrie NPC-ul: id, actiuni disponibile, personality profile, planner si response set.
3. `NpcDecisionService` alege un `NpcActionType`.
4. `NpcResponseSet` transforma actiunea aleasa intr-un `DialogueData`.
5. `NpcModularDialogueTrigger` porneste dialogul prin `DialogueManager`.

## Diferentierea NPC-urilor

NPC-urile nu trebuie diferentiate prin cod separat. Ele se diferentiaza prin:

- `NpcDefinition`
- `NpcPersonalityProfile`
- `NpcResponseSet`
- relationship proprie
- flags si conditii narative

## Cand folosesti plannerul

Pentru NPC-uri simple, foloseste doar `NpcPersonalityProfile`.

Pentru NPC-uri care trebuie sa evalueze recompense si tranzitii intre stari, adauga un `NpcPlannerConfig` si profilele din `CoreStates`.

## Loguri utile pentru etapa 3

Filtreaza consola dupa `[ROAE][AI]` ca sa vezi traseul deciziei:

- `[PlannerBuild][SUCCESS/FAIL]`: politica a fost calculata sau s-a folosit fallback. Include modul VI/PI, numarul de stari/actiuni, iteratii, convergenta, delta final si durata.
- `[PlannerCache][SUCCESS]`: politica exista deja in cache si nu este recalculata.
- `[Decision][SUCCESS/FAIL]`: starea curenta a fost mapata la o reactie NPC sau a intrat pe fallback.
- `[NpcDecision][SUCCESS/FAIL]`: reactia aleasa a fost transformata intr-un dialog concret.
- `[Dialogue][SUCCESS/FAIL]` si `[BaristaDialogue][SUCCESS/FAIL]`: dialogul a pornit sau este explicat motivul esecului.

Pentru prezentare, un exemplu bun este: schimbi stats/relationship, declansezi un NPC si arati in consola starea citita, plannerul folosit, cache hit/miss, reactia aleasa, dialogul selectat si timpul total in milisecunde.

## Reset si testare dev

Pentru Barista:

1. In scena, selecteaza copilul `Barista_DebugTools` de sub `Barista`.
2. In Inspector, pe componenta `BaristaWelcomeDebugMenu`, deschide meniul cu trei puncte.
3. Alege `ROAE/Barista AI/Reset dev state and planner cache`.
4. Pentru o verificare rapida, alege `ROAE/Barista AI/Print current decision`.
5. Filtreaza consola dupa `[ROAE][AI]`.

Cel mai rapid:

1. Pe un obiect cu `NpcActionSelector`, foloseste meniul cu trei puncte din Inspector.
2. Alege `ROAE/NPC/Reset AI Dev State`.
3. Alege `ROAE/NPC/Test All Planner States`.
4. Filtreaza consola dupa `[ROAE][AI][DevTest]`.

Resetul pune stats-urile la `creativity=40`, `empathy=0`, `corruption=0`, reseteaza relatia NPC-ului, starea Barista, progresul narativ, cache-ul generic `NpcPolicySolver` si cache-ul plannerului Barista.

Pentru un obiect separat de test, adauga componenta `NpcAIDevTools` pe un GameObject gol, seteaza `Planner Config`, apoi foloseste context menu-urile `ROAE/AI Dev/Reset runtime state and planner cache` si `ROAE/AI Dev/Test all planner states`.

Nu trebuie testate toate valorile numerice brute. Plannerul foloseste bucket-uri, deci acopera starile abstracte:

- `Creativity`: Low / Medium / High
- `Empathy`: Low / Neutral / High
- `Corruption`: Low / Medium / High
- `Relationship`: Bad / Neutral / Good

Asta inseamna `3 x 3 x 3 x 3 = 81` stari pentru modulul generic NPC_AI. Testul automat verifica daca fiecare stare are o actiune in politica si afiseaza distributia reactiilor alese.
