using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;       // retirado do
using CodeMonkey.Utils; // site CodeMonkey

public class GameHandler : MonoBehaviour {
    // Start is called before the first frame update
    private void Start() {
        Debug.Log("GameHandler.Start");
        
        //GameObject gameObject = new GameObject("Pipe", typeof(SpriteRenderer));

        //gameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.GetInstance().PipeHeadSprite;
    }

}
