using UnityEngine;
using System;
using System.Collections.Generic;

public class tehtävät : MonoBehaviour
{
  
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      Kissa();
      Debug.Log(MultiplyInts(2, 5));
      float test =  MultiplyFloats(1.2f, 2.3f);
      Debug.Log(test);
    }
    void Kissa()
  {
    Debug.Log("miauuuuu!!!!");
  }
    int MultiplyInts(int value1, int value2)
    {
      return value1 * value2;
    }
    float MultiplyFloats(float value1, float value2)
    {
      return value1 * value2;
    }
}
