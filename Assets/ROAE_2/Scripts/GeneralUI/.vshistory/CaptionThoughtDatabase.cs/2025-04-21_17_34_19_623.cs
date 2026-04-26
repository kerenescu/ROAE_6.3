using System.Collections.Generic;
using UnityEngine;

public class CaptionThoughtDatabase : MonoBehaviour
{
    public static CaptionThoughtDatabase Instance;

    private Dictionary<string, string> thoughts = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        thoughts = new Dictionary<string, string>()
        {
            { "necunoscut", "Ăsta nu e spam. Cineva știe prea multe. Prea exact." },
            { "Curator", "Feedback-ul ei nu m-a ucis. Doar mi-a amorțit tot ce simțeam." },
            { "Iza", "Iza știe când să vorbească. Și mai ales când să tacă." },
            { "The Originals", "Nu vreau să îi îngrijorez mai tare decât sunt deja." },
            { "Revel", "Mă distrez cu ei. Dar mereu cu un strat de vinovăție sub glume." },
            { "Leo", "Leo. Mereu ironic. Nu cred că am vorbit niciodată nimic serios cu el." },
            { "Fostu", "După un an și zeci de alegeri proaste, abia acum s-a prins ce-a pierdut. Nu știu de ce nu l-am blocat încă." },
            { "Illuminati", "CACA" }
        };
    }

    public string GetThought(string contactName)
    {
        if (string.IsNullOrEmpty(contactName)) return null;

        foreach (var pair in thoughts)
        {
            if (contactName.Contains(pair.Key))
                return pair.Value;
        }

        return null;
    }
}
