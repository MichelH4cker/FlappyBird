using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;

public class Level : MonoBehaviour {
    private static Level instance;
    
    public static Level GetInstance() {
        return instance;
    }

    private const float CAMERA_ORTOGRPAHIC_SIZE = 50f;
    private const float PIPE_BODY_WIDTH = 11.7f;
    private const float PIPE_HEAD_HEIGHT = 5f;
    private const float PIPE_MOVE_SPEED = 30f;
    private const float PIPE_DESTROY_X_POSITION = -100f;
    private const float PIPE_SPAWN_X_POSITION = +100f;
    private const float BIRD_X_POSITION = 0f;

    private List<Pipe> pipeList;
    
    private float pipeSpawnTimer;
    private float pipeSpawnTimerMax;
    private float gapSize;

    private int pipePassedCount;
    private int pipeSpawned;

    private State state;

    public enum Difficulty {
        Easy,
        Medium,
        Hard,
        Impossible,
    }

    private enum State {
        WaitingToStart,
        Playing,
        BirdDead,
    } 

    private void Awake() {
        instance = this;
        pipeList = new List<Pipe>();
        pipeSpawnTimerMax = 2f;
        gapSize = 20f;
        setDifficulty(Difficulty.Easy);
        state = State.WaitingToStart;
    }

    private void Start() {
        Bird.GetInstance().OnDied += Bird_OnDied;
        Bird.GetInstance().OnStartedPlaying += Bird_OnStartedPlaying;
    }

    private void Bird_OnStartedPlaying(object sender, System.EventArgs e) { 
        state = State.Playing;
    }

    private void Bird_OnDied(object sender, System.EventArgs e) {
        CMDebug.TextPopupMouse("You Lose!");
        state = State.BirdDead;
    }

    private void Update() {
        if (state == State.Playing) {
            HandlePipeMovement();
            HandlePipeSpawning();   
        }
    }

    private void HandlePipeSpawning() {
        pipeSpawnTimer -= Time.deltaTime;
        if(pipeSpawnTimer < 0) {
            pipeSpawnTimer += pipeSpawnTimerMax;

            float totalHeight = 2 * CAMERA_ORTOGRPAHIC_SIZE;
            float heightEdgeLimit = 10f;

            float minHeight = (gapSize * .5f) + heightEdgeLimit;
            float maxHeight = totalHeight - (gapSize * .5f) - heightEdgeLimit;

            float height = Random.Range(minHeight, maxHeight); 

            CreateGapPipes(PIPE_SPAWN_X_POSITION, height, gapSize);
        }
    }

    private void HandlePipeMovement() {
        for (int i = 0; i < pipeList.Count; i++){
            Pipe pipe = pipeList[i];

            // RIGHT
            bool isToTheRightOfBird = false;
            if (pipe.GetXPosition() > BIRD_X_POSITION){
                isToTheRightOfBird = true;
            }

            // MOVE PIPES
            pipe.Move();
            //

            // LEFT
            bool isToTheLeftOfBird = false;
            if (pipe.GetXPosition() <= BIRD_X_POSITION){
                isToTheLeftOfBird = true;
            }

            if (isToTheRightOfBird && isToTheLeftOfBird && pipe.IsBottom()) {
                // PIPE PASSED BIRD
                pipePassedCount++;
            }

            if (pipe.GetXPosition() < PIPE_DESTROY_X_POSITION) {
                pipe.DestroySelf();
                pipeList.Remove(pipe);
                i--; 
            }
        }
    }

    private void setDifficulty(Difficulty difficulty) {
        switch (difficulty) {
            case Difficulty.Easy:
                gapSize = 50f;
                pipeSpawnTimerMax = 2f;
                break;
            case Difficulty.Medium:
                gapSize = 40f;
                pipeSpawnTimerMax = 1.9f;
                break;
            case Difficulty.Hard:
                gapSize = 30f;
                pipeSpawnTimerMax = 1.8f;
                break;
            case Difficulty.Impossible:
                gapSize = 20f;
                pipeSpawnTimerMax = 1.5f;
                break;
        }
    }

    private Difficulty GetDifficulty() {
        if(pipeSpawned >= 30) return Difficulty.Impossible;
        if(pipeSpawned >= 20) return Difficulty.Hard;
        if(pipeSpawned >= 10) return Difficulty.Medium;
        return Difficulty.Easy;
    }

    private void CreateGapPipes(float xPosition, float gapY, float gapSize) {
        CreatePipe(xPosition, gapY - gapSize * .5f, true);
        CreatePipe(xPosition, (CAMERA_ORTOGRPAHIC_SIZE * 2f) - gapY - (gapSize* .5f), false);
        pipeSpawned++;

        Difficulty currentDifficulty = GetDifficulty();
        setDifficulty(currentDifficulty);
    }

    private void CreatePipe(float xPosition, float pipeBodyHeight, bool createBottom){
        // Set Up Pipe Head
        Transform pipeHead = Instantiate(GameAssets.GetInstance().pfPipeHead);

        float pipeHeadYPosition;
        if (createBottom) {
            pipeHeadYPosition = -CAMERA_ORTOGRPAHIC_SIZE + pipeBodyHeight - PIPE_HEAD_HEIGHT * .5f;
        } else {
            pipeHeadYPosition = +CAMERA_ORTOGRPAHIC_SIZE - pipeBodyHeight + PIPE_HEAD_HEIGHT * .5f;
        }
        pipeHead.position = new Vector3(xPosition, pipeHeadYPosition);

        // Set Up Pipe Body 
        Transform pipeBody = Instantiate(GameAssets.GetInstance().pfPipeBody);

        float pipeBodyYPosition;
        if (createBottom) {
            pipeBodyYPosition = -CAMERA_ORTOGRPAHIC_SIZE;
        } else {
            pipeBodyYPosition = +CAMERA_ORTOGRPAHIC_SIZE - pipeBodyHeight;
        }
        pipeBody.position = new Vector3(xPosition, pipeBodyYPosition);

        SpriteRenderer pipeBodySpriteRenderer = pipeBody.GetComponent<SpriteRenderer>();
        pipeBodySpriteRenderer.size = new Vector2(PIPE_BODY_WIDTH, pipeBodyHeight);

        // Set UP Box Collider
        BoxCollider2D pipeBodyBoxCollider2D = pipeBody.GetComponent<BoxCollider2D>();
        pipeBodyBoxCollider2D.size = new Vector2(PIPE_BODY_WIDTH, pipeBodyHeight);
        pipeBodyBoxCollider2D.offset = new Vector2(0, pipeBodyHeight * 0.5f);

        Pipe pipe = new Pipe(pipeHead, pipeBody, createBottom);
        pipeList.Add(pipe);
    }

    public int GetPipeSpawned() { 
        return pipeSpawned;
    }

    public int GetPipePassedCount() {
        return pipePassedCount;
    }

    // represents a single pipe class
    private class Pipe {
        private Transform pipeHeadTransform;
        private Transform pipeBodyTransform;

        private bool isBottom;

        public Pipe(Transform pipeHeadTransform, Transform pipeBodyTransform, bool isBottom) {
            this.pipeHeadTransform = pipeHeadTransform;
            this.pipeBodyTransform = pipeBodyTransform;
            this.isBottom = isBottom;
        }

        public void Move() {
            pipeHeadTransform.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime;
            
            pipeBodyTransform.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime;
        }

        public bool IsBottom() {
            return isBottom;
        }

        public float GetXPosition() {
            return pipeHeadTransform.position.x;
        }

        public void DestroySelf() {
            Destroy(pipeHeadTransform.gameObject);
            Destroy(pipeBodyTransform.gameObject);
        }         
    }
}
