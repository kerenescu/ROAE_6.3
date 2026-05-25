# ROAE

Repo pentru versiunea Unity actualizata a proiectului **Root of All Evil**, cu focus pe un sistem AI narativ mai clar: NPC-urile nu mai aleg replici doar prin ramuri hardcodate, ci printr-un pipeline de stare, bias, planner si routing de dialog.

## Pipeline-ul AI pentru NPC-uri

Fluxul principal este:

```text
raw player stats
-> relationship cu NPC-ul
-> bias afin specific NPC-ului
-> context bucketizat
-> model shared VI/PI
-> actiune NPC
-> ton narativ
-> varianta de dialog
```

Pe scurt, sistemul porneste de la valorile reale ale playerului:

- `Creativity`
- `Empathy`
- `Plant Corruption`

La acestea se adauga relatia 1v1 cu NPC-ul curent. Apoi fiecare personaj poate aplica un **bias afin** propriu, adica o transformare controlata a felului in care acel NPC "citeste" aceleasi stats. Astfel, acelasi player poate fi interpretat diferit de Barista, Madame Lichenia sau Anticar.

## Context bucketizat

Dupa bias, valorile sunt transformate in bucket-uri:

- creativity: `Low / Medium / High`
- empathy: `Low / Neutral / High`
- corruption: `Low / Medium / High`
- relationship: `Bad / Neutral / Good`

Aceste bucket-uri formeaza starea compacta folosita de planner. In loc sa existe multe conditii de tip `if-else`, starea este evaluata de un model comun.

## VI / PI shared model

NPC-urile pot folosi acelasi model de decizie cu:

- `Value Iteration`
- `Policy Iteration`

Plannerul calculeaza o politica peste starile posibile si alege o actiune NPC precum `WarmOffer`, `ObserveNeutral`, `MischievousProbe`, `RevealHint` etc. Actiunea este apoi mapata catre un ton narativ:

- `Warm`
- `Neutral`
- `Mischievous`

Tonul rezultat face routing catre varianta concreta de dialog pentru momentul narativ curent.

## Routing narativ

Dialogul final nu este ales direct din stats. El trece prin:

```text
planner action -> tone -> exact tone variant / fallback variant
```

Asta permite aceluiasi moment narativ sa aiba versiuni diferite de text, fara sa duplicam toata logica de decizie in fiecare NPC.

## Companion / Snail

Upgrade-ul adauga si un companion nou: melcul.

Companionul are propriul sistem de:

- summon points
- manifestare vizuala
- stare emotionala
- planner VI/PI simplificat
- feedback din emotiile NPC-urilor

Melcul poate rula in mod `Stats + NPC signals` sau `NPC Signals Only`. In modul `NPC Signals Only`, summon-ul si starea companionului sunt legate de semnalele sociale observate din NPC-uri, nu doar de stats-urile brute ale playerului.

## De ce conteaza

Scopul acestei versiuni este sa faca AI-ul narativ mai usor de testat si demonstrat:

- aceleasi stats pot produce reactii diferite prin bias de personaj
- VI si PI pot fi comparate pe acelasi context local
- dialogurile sunt routate prin tonuri, nu prin ramuri hardcodate
- companionul reactioneaza la climatul emotional creat de NPC-uri

