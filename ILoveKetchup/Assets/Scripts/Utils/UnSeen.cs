using UnityEngine;
using Random = UnityEngine.Random;

public class UnSeen
{
   private decimal unReachValue;
   private decimal randomizer;
   private decimal saveValue;

   public UnSeen(decimal beginValue = 0.0m)
   {
      randomizer = 1.0m;
      saveValue = beginValue;
   }

   public decimal Get()
   {
      return saveValue / randomizer;
   }

   public void Set(decimal value)
   {
      randomizer = (decimal)Random.value + 0.1m;
      saveValue = value * randomizer;
      unReachValue = value;
   }
}
