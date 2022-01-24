using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ScoreWindow : MonoBehaviour {

    private Text scoreText;
    private Text highscoreText;


    private void Awake() {
        scoreText = transform.Find("ScoreText").GetComponent<Text>();
        highscoreText = transform.Find("HighscoreText").GetComponent<Text>();
    }

    private void Start() {
        highscoreText.text = "HIGHSCORE: " + Score.GetHighscore().ToString();
    }

    private void Update() {
        scoreText.text = Level.GetInstance().GetPipePassedCount().ToString();
    }
}
