using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey;
using CodeMonkey.Utils;

public class GameOverWindow : MonoBehaviour {

    private Text scoreText;
    private Text highscoreText;

    private void Awake () {
        scoreText = transform.Find("ScoreText").GetComponent<Text>();
        highscoreText = transform.Find("HighscoreText").GetComponent<Text>();

        
        transform.Find("RetryButton").GetComponent<Button_UI>().ClickFunc = () => {  
            Loader.Load(Loader.Scene.GameScene);
        };
        transform.Find("RetryButton").GetComponent<Button_UI>().AddButtonSounds();

        transform.Find("MainMenuButton").GetComponent<Button_UI>().ClickFunc = () => {  
            Loader.Load(Loader.Scene.MainMenuScene);
        };
        transform.Find("MainMenuButton").GetComponent<Button_UI>().AddButtonSounds();

    }

    private void Start () {
        Bird.GetInstance().OnDied += Bird_OnDied;
        Hide();
    }

    private void Bird_OnDied(object sender, System.EventArgs e) {
        scoreText.text = Level.GetInstance().GetPipePassedCount().ToString();

        if (Level.GetInstance().GetPipePassedCount() >= Score.GetHighscore()) {
            highscoreText.text = "NEW HIGHSCORE!";
        } else {
            highscoreText.text = "HIGHSCORE " + Score.GetHighscore().ToString();
        }
        Show();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
    }
}
