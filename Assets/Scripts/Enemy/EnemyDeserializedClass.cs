using UnityEngine;
using System;

[Serializable]
public class Enemy
{
    public string name;
    public int sprite;
    public int hp;
    public int speed;
    public int damage;
    /*
       private static EventBus theInstance;
       public static EventBus Instance
       {
           get
           {
               if (theInstance == null)
                   theInstance = new EventBus();
               return theInstance;
           }
       }

       public event Action<Vector3, Damage, Hittable> OnDamage;

       public void DoDamage(Vector3 where, Damage dmg, Hittable target)
       {
           OnDamage?.Invoke(where, dmg, target);
       }
       */

}
