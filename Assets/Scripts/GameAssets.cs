using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour {
    private static GameAssets instance;
    // Instantiate(GameAssets.GetInstance().pfPipeHead);
    public static GameAssets GetInstance() {
        return instance;
    }

    private void Awake(){
        instance = this;
    }

    public Sprite PipeHeadSprite;
    public Transform pfPipeHead;
    public Transform pfPipeBody;
    public Transform pfGround;
    public Transform pfCloud_1;
    public Transform pfCloud_2;
    public Transform pfCloud_3;

    public AudioClip BirdJump;

    public SoundAudioClip[] soundAudioClipArray;

    [Serializable]
    public class SoundAudioClip {
        public SoundManager.Sound sound;
        public AudioClip audioClip;
    }
}
