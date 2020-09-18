using UnityEngine;

// Возможно, на данный момент использование ScriptableObject это как из пушки по воробьям, но
// в будущем может пригодиться
[CreateAssetMenu(fileName = "New EnemyData", menuName = "Enemy Data", order = 51)]
public class EnemyData : ScriptableObject
{
    [SerializeField] private string objectName;
    [SerializeField] private float speed;
    [SerializeField] private int health;

    // C# 6.0 - огонь!
    public string ObjectName => objectName;
    public float Speed => speed;
    public int Health => health;
}
