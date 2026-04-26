// 🌸 PressedFlower.cs – obiect colecționabil ce apare în jurnal
using UnityEngine;

[CreateAssetMenu(menuName = "Journal/Pressed Flower")]
public class PressedFlower : ScriptableObject
{
    public string flowerName;
    public Sprite sprite; // imaginea florii
    [TextArea(3, 10)] public string memoryText; // textul gândului când o colectezi
    public bool collected = false;
}