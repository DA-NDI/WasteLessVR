using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreBoard1 : MonoBehaviour {

	public int playTime = 120;
	// public resetPos resetPos;
	bool isGameRunning = false;
	// public int currentHighScore = 0;
	public int currentScore = 0;
	public float currentTime = 0.0f;
	public AudioClip playMusic;
	public AudioClip idleMusic;
	public AudioClip endGameSound;
	// public AudioClip highScoreSound;
	public AudioClip scoreSound;
	public AudioClip errorScore;
	public AudioSource musicAudioSource;
	bool displayTransition = false;
	
	// public Text txtHighscore;
	public Text txtTime;
	public Text txtScore;
	// public Wall wall;
	//constants
	// const string HIGHSCORE="HIGHSCORE";

	private AssetBundle myLoadedAssetBundle;
    private string[] scenePaths;

	public void handleGameStart(){
		if(!isGameRunning){
			currentScore = 0;
			currentTime = 0;
			isGameRunning = true;
			startMusic(musicAudioSource, playMusic);
		}
	}
	void startMusic(AudioSource audioSrc, AudioClip clip){
		if(audioSrc.isPlaying){
			audioSrc.Stop();
		}
		if(clip!=null){
			audioSrc.clip=clip;
			audioSrc.Play();
		}
	}
	void startSound(AudioClip clip){
		if(clip!=null){
			AudioSource.PlayClipAtPoint(clip, transform.position);
		}
	}

	void handleGameEnd(){
		isGameRunning = false;
		// if(currentScore >currentHighScore){
		// 	handleNewHighScore();
		// }
		currentTime = playTime;
		currentScore = 0;
		startSound(endGameSound);
		startMusic(musicAudioSource, idleMusic);
		// wall.wallFall();
	}
	// void handleNewHighScore(){
	// 	saveHighScore(currentScore);
	// 	currentHighScore = currentScore;
	// 	startSound(highScoreSound);
	// 	//TODO: Make some highscore transition;
	// }
	// void loadingHighScore(){
	// 	currentHighScore = PlayerPrefs.GetInt(HIGHSCORE);
	// }
	// void saveHighScore(int newHighscore){
	// 	 PlayerPrefs.SetInt(HIGHSCORE, newHighscore);
	// }

	public void handleScored(){
		if(isGameRunning){
			startSound(scoreSound);
			currentScore++;
		}
	}

	public void handleWrongScored(){
		if(isGameRunning){
			startSound(errorScore);
			currentScore--;
		}
	}
	// Use this for initialization
	void Start () {
		myLoadedAssetBundle = AssetBundle.LoadFromFile("Assets/Scene2");
   //     scenePaths = myLoadedAssetBundle.GetAllScenePaths();
		musicAudioSource = GetComponent<AudioSource>();
		// loadingHighScore();
		// handleGameEnd();
		handleGameStart();
	}
	
	// Update is called once per frame
	void Update () {
		if(isGameRunning){
			handleGamePlay();
		}
		
	}

	void handleGamePlay(){
		currentTime += Time.deltaTime;
		if(currentTime>playTime){
			handleGameEnd();
		}
		updateTexts();
	}
	
	void updateTexts(){
		if(!displayTransition){
			string timeDisplay = "";
	//		if(currentTime > playTime - 5){
	///			timeDisplay = string.Format(("0:0#.00"), ((float)playTime-currentTime));
	//		}else{
				timeDisplay = "" + (int)((float)playTime-currentTime);
	//		}
	if (playTime - currentTime <= 0)
	{
		Application.LoadLevel(2);
	//	 SceneManager.LoadScene(scenePaths[0], LoadSceneMode.Single);
	// Time.timeScale = 0;
	//		 SceneManager.LoadScene("Scene2", LoadSceneMode.Additive);
	}
			showText("" + timeDisplay, "" + currentScore);
		}
	}

	void showText( string newTime, string newScore){
		txtTime.text = newTime;
		txtScore.text = newScore;
	}

	// IEnumerator newHighScoreTransition(){
	// 	displayTransition = true;
	// 	showText("***", "***", "***");
	// 	yield return new WaitForSeconds(1f);
	// 	showText("NEW", "HI", "SCR");
	// 	yield return new WaitForSeconds(1f);
	// 	showText("***", "***", "***");
	// 	yield return new WaitForSeconds(1f);

	// 	displayTransition = false;
	// 	updateTexts();
	// }

	
}
