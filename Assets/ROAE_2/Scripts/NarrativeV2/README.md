# NarrativeV2

Contine sistemul narativ modern: progres global, selectie de momente, contracte pentru planners si sistemul Barista Welcome.

## Structura

- `Shared`: cod comun pentru progres narativ, moment selection, flags si contracte.
- `BaristaWelcome`: implementarea concreta pentru momentul Barista Welcome.

## Regula arhitecturala

Codul din `Shared` trebuie sa fie reutilizabil pentru mai multe NPC-uri sau momente. Codul specific Barista ramane in `BaristaWelcome`.

Daca un sistem nou de NPC foloseste aceleasi concepte ca Barista, extrage partea comuna in `Shared` inainte sa copiezi logica.
