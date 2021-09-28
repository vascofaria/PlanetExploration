using UnityEngine;
using System.Collections.Generic;
public class Markable : MonoBehaviour {
    public bool isMarked = false;

    public void MarkResource(){
        isMarked = true;
    }

    private void Awake() {
        
    }
}