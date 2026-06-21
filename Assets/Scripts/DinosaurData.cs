using UnityEngine;

[CreateAssetMenu(fileName = "NewDinosaur", menuName = "DinoMergeIdle/Dinosaur Data")]
public class DinosaurData : ScriptableObject
{
    [Header("Dinozor Kimliği")]
    public string dinosaurName;
    public int level;

    [Header("Görsel Simgesi")]
    public Sprite dinosaurSprite; 

    [Header("Ekonomi Dengesi")]
    public float goldPerSecond; 
}