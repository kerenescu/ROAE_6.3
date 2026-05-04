# ROAE 6.3

Acest repo contine versiunea updatata a proiectului meu Unity, in care am inceput sa mut logica narativa dintr-o structura veche, bazata pe multe `if-else` greu de urmarit, intr-un sistem mai clar si mai scalabil.

Focusul principal al upgrade-ului este trecerea catre un sistem de decizie pentru NPC-uri bazat pe `value iteration`, plus un sistem de dialog mai organizat, cu panel dedicat si alegeri care influenteaza atat stats-urile playerului, cat si relatia 1v1 cu fiecare NPC.

## Ce s-a schimbat fata de varianta initiala

Varianta veche a proiectului era construita in mare parte din conditii hardcodate si ramuri de dialog greu de mentinut. In aceasta versiune:

- am inceput sa inlocuiesc logica dezorganizata de tip `if-else` cu un planner bazat pe stari si actiuni
- am creat un panel de dialog dedicat, gestionat printr-un `DialogueManager`
- am legat replicile si alegerile de cele 3 stats principale ale playerului
- am adaugat relatie 1v1 intre player si NPC, urmarita separat pentru fiecare personaj
- am separat mai bine UI-ul, state-ul narativ si logica de decizie

## Cele 3 stats ale playerului

Sistemul curent foloseste 3 axe principale:

- `Creativity`
- `Empathy`
- `Plant Corruption`

Aceste valori sunt tinute in `CreativeCore`, iar alegerile din dialog pot modifica direct aceste stats prin `StatsEffect`.

## Relatia 1v1 cu NPC-ul

Pe langa stats-urile globale ale playerului, fiecare NPC important poate avea propria relatie cu playerul. Aceasta este gestionata prin `NpcRelationshipState`.

Asta inseamna ca dialogul nu mai reactioneaza doar la "cine este playerul in general", ci si la istoricul lui cu un personaj anume. Practic, doua interactiuni cu stats similare pot produce tonuri diferite daca relatia cu NPC-ul este buna, neutra sau proasta.

## Cum functioneaza Value Iteration aici

In loc sa aleg manual fiecare reactie prin ramuri fixe, sistemul construieste o stare simplificata a contextului curent:

- bucket pentru `empathy`
- bucket pentru `creativity`
- bucket pentru `corruption`
- bucket pentru `relationship`

Acestea sunt combinate intr-un `NpcDecisionState`.

Mai departe:

- `NpcStateDiscretizer` transforma valorile curente in bucket-uri usor de evaluat
- `ValueIterationSolver` calculeaza ce actiune are valoarea asteptata cea mai buna pentru fiecare stare
- `NpcPolicySolver` construieste si cache-uieste politica rezultata
- `NpcActionSelector` cere actiunea potrivita pentru NPC-ul curent

Pe scurt, in loc sa spun "daca empathy > x si relationship < y atunci replica Z", las sistemul sa aleaga tonul optim pe baza unei politici calculate.

## Sistemul de dialog

Dialogul este gestionat prin `DialogueManager`, care:

- afiseaza panelul de dialog
- reda liniile de text in ordine
- afiseaza alegerile disponibile
- aplica efectele alese
- poate lega dialogul de portrete, flags si alte efecte narative

Fiecare `DialogueChoice` poate:

- duce la un alt nod de dialog
- modifica stats-urile playerului
- modifica relatia cu NPC-ul
- declansa efecte narative suplimentare

## De ce upgrade-ul asta conteaza

Scopul nu este doar "sa mearga", ci sa pot extinde proiectul fara sa il stric de fiecare data cand adaug o interactiune noua.

Trecerea de la `if-else` hardcodate la un sistem bazat pe stare + policy ajuta la:

- cod mai usor de inteles
- dialoguri mai coerente
- NPC-uri mai usor de extins
- separare mai buna intre continut, UI si logica
- iteratie mai rapida pe comportamente narative

## Structura curata a proiectului

Codul propriu al jocului locuieste in `Assets/ROAE_2`. Pachetele externe sau generate de Unity, cum ar fi `AdventureCreator`, `TextMesh Pro`, `Packages` si `ProjectSettings`, nu trebuie reorganizate manual.

Zonele principale sunt:

- `Assets/ROAE_2/Scripts`: cod runtime/editor propriu, impartit pe module.
- `Assets/ROAE_2/Data`: asset-uri de configurare si continut narativ.
- `Assets/ROAE_2/Prefabs`: prefabs folosite de UI, gameplay si sisteme.
- `Assets/ROAE_2/_Archive`: cod istoric sau experimente care nu sunt parte din runtime-ul curent.

Pentru orientare rapida, folderele importante au README local. Regula de baza: daca un script nou apartine unui sistem existent, intra in folderul acelui sistem; daca introduce o categorie noua de responsabilitate, creeaza un folder nou cu README scurt.

## Stare curenta

Momentan proiectul este in tranzitie: unele zone au fost deja mutate pe structura noua, iar altele inca pastreaza urme din sistemul vechi. Repo-ul documenteaza tocmai aceasta trecere catre o arhitectura narativa mai curata si mai usor de scalat.
