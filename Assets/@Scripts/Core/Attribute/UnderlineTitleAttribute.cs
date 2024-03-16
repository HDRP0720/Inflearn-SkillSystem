using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderlineTitleAttribute : PropertyAttribute
{
  // Properties
  public string Title { get; private set; }
  public int Space { get; private set; }
  
  // Constructor
  public UnderlineTitleAttribute(string title, int space = 12)
  {
    Title = title;
    Space = space;
  }
}
