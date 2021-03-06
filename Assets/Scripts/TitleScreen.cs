﻿using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


namespace Circk{
	public class TitleScreen : MonoBehaviour {

		[Header("UI")]
		public GameObject logo;
		public GameObject logoFinalPos;
		public GameObject credits;
		public GameObject creditsFinalPos;
		public GameObject message;
		public GameObject messageFinalPos;
		public Image messageImage;
		public Sprite messageImageMenu;
		public Sprite messageImageRetry;

		protected Vector3 logoBeganPos;
		protected Vector3 creditsBeganPos;
		protected Vector3 messageBeganPos;
		protected Vector3 gameOverBeganPos;

		public GameObject gameOver;
		public Text finalScoreValue;
		public Text maxScoreValue;

		public float introTime = 1.0f;
		protected bool onIntro = true;

		void Awake(){
			logoBeganPos = logo.transform.position;
			creditsBeganPos = credits.transform.position;
			messageBeganPos = message.transform.position;
			gameOverBeganPos = gameOver.transform.position;

		}
			
		void Start () {
			maxScoreValue.text = PlayerPrefs.GetInt("MaxScore").ToString();
			Intro ();
		}

		void Update () {
			if (Input.GetKeyDown(KeyCode.Space)){
				switch (GameManager.Instance.CurrentGameState) {
				case GameManager.GameState.TITLE:
					IntroOut();
					break;

				case GameManager.GameState.RETRY:
					
					GameOverOutro ();
					break;

				default:
					break;
				}

			}
		}

		public void Intro(){

			onIntro = true;

			logo.transform.DOMove (logoFinalPos.transform.position, introTime)
				.SetDelay(.5f)
				.OnComplete(() => { 
					credits.transform.DOMove(creditsFinalPos.transform.position, introTime)
						.OnComplete(() => { 
							message.transform.DOMove(messageFinalPos.transform.position, introTime / 2)
								.OnComplete(() => { onIntro = false; });
						});
				});

			AudioStuff.PlayMusic("mus-menu");
		}

		public void IntroOut(){
			if (onIntro)
				return;

			message.transform.DOMove (messageBeganPos, introTime / 2);
			credits.transform.DOMove (creditsBeganPos, introTime / 2);
			logo.transform.DOMove(logoBeganPos, introTime / 2);

			GameManager.Instance.CurrentGameState = GameManager.GameState.GAME;
			GameManager.Instance.StartFillEnergyBar ();

			AudioStuff.PlaySound("start",AudioStuff.volumeSfx);
			AudioStuff.PlayMusic("mus-fase");
		}

		public void CallGameOver(){
			if(GameManager.Instance.CurrentGameState == GameManager.GameState.GAME){
				if(GameManager.Instance.currentScore > PlayerPrefs.GetInt("MaxScore")){
					PlayerPrefs.SetInt("MaxScore", GameManager.Instance.currentScore);
				}
				maxScoreValue.text = PlayerPrefs.GetInt("MaxScore").ToString();
				finalScoreValue.text = GameManager.Instance.currentScore.ToString();
			}
			
			onIntro = true;
			GameManager.Instance.energyBarFilling = false;
			GameManager.Instance.CurrentGameState = GameManager.GameState.RETRY;

			messageImage.sprite = messageImageRetry;
		
			gameOver.transform.DOMoveX (0f, introTime);
			credits.transform.DOMove (creditsFinalPos.transform.position, introTime);
			message.transform.DOMove (messageFinalPos.transform.position, introTime).OnComplete(() => { onIntro = false; });

			AudioStuff.StopMusic();
			AudioStuff.PlaySound("mus-vinheta",AudioStuff.volumeMusic);
		}

		public void GameOverOutro(){
			
			if (onIntro)
				return;
			
			DOTween.KillAll();
			if (EnemyBase.enemies != null) EnemyBase.enemies.Clear();
			if (ItemObject.items != null) ItemObject.items.Clear();
			SceneManager.LoadScene("Main");
			//AudioStuff.PlaySound("start");

		}
	}
}
