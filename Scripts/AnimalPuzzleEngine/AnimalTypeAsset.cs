using UnityEngine;

[CreateAssetMenu(menuName = "Animal Puzzle Engine/Animal Type")]
public class AnimalTypeAsset : ScriptableObject
{
    public enum AnimalType { Wolf, Pig, Chick, Chicken, Crocodile, Fox, Giraffe }
    [SerializeField] private AnimalType animalType;

    public Sprite animalSprite;
    public AnimalType Type => animalType;
    public Sprite AnimalSprite => animalSprite;
}