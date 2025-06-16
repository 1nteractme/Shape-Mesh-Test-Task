using UnityEngine;

[CreateAssetMenu(menuName = "Animal Puzzle Engine/Shape Type")]
public class ShapeTypeAsset : ScriptableObject
{
    public enum ShapeType { Circle, Square, Triangle, Heavy }
    [SerializeField] private ShapeType type;
    
    public Sprite shapeSprite, borderSprite;
    public ShapeType Type => type;
    public Sprite ShapeSprite => shapeSprite;
    public Sprite BorderSprite => borderSprite;
}