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
    private const float CLOUD_DESTROY_X_POSITION = -160f;
    private const float CLOUD_SPAWN_X_POSITION = +160f;
    private const float CLOUD_SPAWN_Y_POSITION = +30f;
    private const float GROUND_DESTROY_X_POSITION = -200f;
    private const float BIRD_X_POSITION = 0f;

    private float cloudSpawnTimer;
    private float pipeSpawnTimer;
    private float pipeSpawnTimerMax;
    private float gapSize;

    private int pipePassedCount;
    private int pipeSpawned;
    
    private List<Transform> groundList;
    private List<Transform> cloudList;
    private List<Pipe> pipeList;

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
        SpawnInitalGround();
        SpawnInitialClouds();
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
            HandleGround();
            HandleClouds();
        }
    }

    private void SpawnInitialClouds() {
        cloudList = new List<Transform>();
        Transform cloudTransform;

        cloudTransform = Instantiate(GetCloudPrefabTransform(), new Vector3(0, CLOUD_SPAWN_Y_POSITION, 0), Quaternion.identity);
        cloudList.Add(cloudTransform);
    }

    private Transform GetCloudPrefabTransform() {
        switch (Random.Range(0, 3)) {
            default:
            case 0: return GameAssets.GetInstance().pfCloud_1; 
            case 1: return GameAssets.GetInstance().pfCloud_2;
            case 2: return GameAssets.GetInstance().pfCloud_3;
        }
    }

    private void HandleClouds() {
        // Handle clouds spawning
        cloudSpawnTimer -= Time.deltaTime;
        if (cloudSpawnTimer < 0) {
            // Time to spawn another cloud
            float cloudSpawnTimerMax = 5f;

            cloudSpawnTimer = cloudSpawnTimerMax;
            Transform cloudTransform = Instantiate(GetCloudPrefabTransform(), new Vector3(CLOUD_SPAWN_X_POSITION, CLOUD_SPAWN_Y_POSITION, 0), Quaternion.identity);
            cloudList.Add(cloudTransform);
        }

        // Handle clouds moving
        for (int i = 0; i < cloudList.Count; i++) {
            Transform cloudTransform = cloudList[i];

            // Move clouds by less speed than pipes for Parallax
            cloudTransform.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime * .7f; 

            if(cloudTransform.position.x < CLOUD_DESTROY_X_POSITION) {
                // Cloud past destroy point, destroy self
                Destroy(cloudTransform.gameObject);
                cloudList.RemoveAt(i);
                i--;
            }
        }
    }

    private void SpawnInitalGround() {
        groundList = new List<Transform>();
        Transform groundTransform;
        float groundY = -48f;
        float groundWidth = 190f;

        groundTransform = Instantiate(GameAssets.GetInstance().pfGround, new Vector3(0, groundY, 0), Quaternion.identity);
        groundList.Add(groundTransform);

        groundTransform = Instantiate(GameAssets.GetInstance().pfGround, new Vector3(groundWidth, groundY, 0), Quaternion.identity);
        groundList.Add(groundTransform);


        groundTransform = Instantiate(GameAssets.GetInstance().pfGround, new Vector3(groundWidth * 2f, groundY, 0), Quaternion.identity);
        groundList.Add(groundTransform);

    }

    private void HandleGround() {
        foreach (Transform groundTransform in groundList) {
            groundTransform.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime; 

            if(groundTransform.position.x < GROUND_DESTROY_X_POSITION) {
                // Ground passed the left side, relocate on right side
                // Find right most X position
                float rightMostXPosition = -100f;
                for (int i = 0; i < groundList.Count; i++) {
                    if(groundList[i].position.x > rightMostXPosition) {
                        rightMostXPosition = groundList[i].position.x;
                    }
                }

                // Place Ground on the right most position
                float groundWidth = 190f;
                groundTransform.position = new Vector3(rightMostXPosition + groundWidth, groundTransform.position.y, groundTransform.position.z);
            }
        }
    }

    private void HandlePipeSpawning() {
        pipeSpawnTimer -= Time.deltaTime;
        if(pipeSpawnTimer < 0) {
            pipeSpawnTimer += pipeSpawnTimerMax;

            float totalHeight = 2 * CAMERA_ORTOGRPAHIC_SIZE;
            float heightEdgeLimit = 15f;

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
                SoundManager.PlaySound(SoundManager.Sound.  Score);//tocar
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
                gapSize = 35f;
                pipeSpawnTimerMax = 2f;
                break;
            case Difficulty.Medium:
                gapSize = 30f;
                pipeSpawnTimerMax = 1.9f;
                break;
            case Difficulty.Hard:
                gapSize = 25f;
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
