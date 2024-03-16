using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
  [SerializeField][UnderlineTitle("Option")]
  private bool _isOn;
  
  [SerializeField][UnderlineTitle("Number")]
  private int _primary;

  [SerializeField][UnderlineTitle("String")]
  private string _text;
}
