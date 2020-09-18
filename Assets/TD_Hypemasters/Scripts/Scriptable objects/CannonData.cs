using UnityEngine;

// В рамках прототипа нет большого профита от использования ScriptableObject, но гипотетически в дальнейшем пригодится
[CreateAssetMenu(fileName = "New CannonData", menuName = "Canon Data", order = 51)]
public class CannonData : ScriptableObject
{
    [SerializeField] private string objectName;
    [SerializeField] private int damage;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackDistance;
    [SerializeField] private float angleToHorizon;

    public string ObjectName => objectName;
    
    // ToDo проверку на валидность сделать при присвоении значений или делать один раз при обращении к свойству
    public int Damage => Mathf.Max(damage, 1);
    public float AttackDelay => Mathf.Max(attackDelay, 0.1f); 
    public float AttackDistance => Mathf.Max(attackDistance, 0.1f);
    public float AngleToHorizon => Mathf.Clamp(angleToHorizon, 0, 90);
}
