using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;

public class Bird : MonoBehaviour {
    
    public static Bird GetInstance() {
        return instance;
    }

    private static Bird instance;

    private const float JUMP_AMOUNT = 100f;

    private Rigidbody2D birdRigidBody2D;

    public event EventHandler OnDied;
    public event EventHandler OnStartedPlaying;

    private State state;

    private enum State {
        WaitingToStart,
        Playing,
        Dead,
    }

    private void Awake() {
        instance = this;
        birdRigidBody2D = GetComponent<Rigidbody2D>();
        birdRigidBody2D.bodyType = RigidbodyType2D.Static;

    }

    private void Update() {
        switch (state) {
            default:
                break;
            case State.WaitingToStart: 
                if ((Input.GetKeyDown(KeyCode.Space)) || (Input.GetKeyDown(KeyCode.W)) || (Input.GetKeyDown(KeyCode.UpArrow)) || (Input.GetMouseButtonDown(0))) {
                    state = State.Playing;
                    birdRigidBody2D.bodyType = RigidbodyType2D.Dynamic;
                    Jump();     
                    if (OnStartedPlaying != null) OnStartedPlaying(this, EventArgs.Empty);     
                }                    
                break;
            case State.Playing: 
                if ((Input.GetKeyDown(KeyCode.Space)) || (Input.GetKeyDown(KeyCode.W)) || (Input.GetKeyDown(KeyCode.UpArrow)) || (Input.GetMouseButtonDown(0))) {
                    Jump();            
                }
                break;
            case State.Dead: 
                break;
        }
        
        
    }        

    private void Jump() {
        birdRigidBody2D.velocity = Vector2.up * JUMP_AMOUNT; 
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        birdRigidBody2D.bodyType = RigidbodyType2D.Static;
        if (OnDied != null) OnDied(this, EventArgs.Empty);
    }
}
