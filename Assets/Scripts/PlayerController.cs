﻿using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace Circk{
	
	public class PlayerController : MonoBehaviour {

		public static PlayerController me = null;

		[Header("Values")]
		public float speed;
		public float delayAfterHit = 0.5f;

		[Header("States")]
		protected bool moveLocked = false;

		[Header("Components")]
		private Transform tr;
		private Rigidbody2D rb;
		private Animator an;
		private GameManager gm;
		private ItemManager im;

		[Header("Direction Vector")]
		public Transform originPosition;

		[Header("Item")]
		public GameObject itemHolderSprite;
		public GameObject itemSprite;
		private Tweener tween;
		private ItemManager.ItemType currentItem;
		private bool haveItem;
		private bool canUseItem;
		private int maxUsesOfItem;
		public int currentUseOfItem;

		private void Awake(){
			//Init Components
			tr = GetComponent<Transform>();
			rb = GetComponent<Rigidbody2D>();
			an = GetComponent<Animator>();
			gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
			im = GameObject.FindGameObjectWithTag("GameManager").GetComponent<ItemManager>();

			me = this;
		}

		private void Update(){
			//Only moves if the game is on
			if(gm.CurrentGameState == GameManager.GameState.GAME){
				Move ();

				if(haveItem && canUseItem && Input.GetKeyDown(KeyCode.Space)){
					UseItem();
				}
			}
		}

		public void Move (){
			//Dont move if player has just been pushed
			if(!moveLocked){
				//Get the input
				float horizontalValue = Input.GetAxis("Horizontal") * speed;
				float verticalValue = Input.GetAxis("Vertical") * speed * 0.75f;

				//Rotate
				if(horizontalValue > 0){
					gameObject.GetComponent<SpriteRenderer> ().flipX = true;
				}
				else if(horizontalValue < 0){
					gameObject.GetComponent<SpriteRenderer> ().flipX = false;
				}

				//Walk
				an.SetBool("Walking", (Mathf.Abs(verticalValue) > 0f) || Mathf.Abs(horizontalValue) > 0f);
				tr.Translate (horizontalValue * Time.deltaTime, verticalValue * Time.deltaTime, 0);
			}
		}
   	
		private void OnCollisionEnter2D(Collision2D collision){
			//Gameover when collide to the border
			if (GameManager.Instance.CurrentGameState == GameManager.GameState.GAME && collision.gameObject.tag == "EdgeDeath") {

				//Vector3 aux = new Vector3(tr.localScale.x * 1.2f, tr.localScale.y * 1.2f, tr.localScale.z);

				moveLocked = true;
				tr.DOScale (Vector3.zero, 0.3f).OnComplete (() => {
					GameManager.Instance.titleScreen.CallGameOver ();
				});
				AudioStuff.PlaySound("palhacodead");
			}
		}
		private void OnTriggerEnter2D(Collider2D collider){
			if (collider.gameObject.tag == "EdgeWarning") {
				an.SetBool ("Balance", true);
				GameManager.Instance.CrowdCheerAnimation ();
			}
		}
		private void OnTriggerExit2D(Collider2D collider){
			if (collider.gameObject.tag == "EdgeWarning") {
				an.SetBool ("Balance", false);
			}
		}

		//When the player is hit - called by enemy
		public void Impact(Vector3 orientation, float force){

			an.SetTrigger ("Hit");

			//Only works if the movementLoch is false
			if (!moveLocked) {
				rb.AddForce (force * orientation, ForceMode2D.Impulse);

				moveLocked = true;

				StartCoroutine (WaitAndCall (delayAfterHit, () => {
					moveLocked = false;
				}));

				AudioStuff.PlaySound("palhaco");
			}
		}

		//Wait to restart movement after hit
		private IEnumerator WaitAndCall(float waitTime, Action callback){
			//Set the moventLock to false after the delay
			yield return new WaitForSeconds(waitTime);
			if (callback != null)
				callback ();
		}

		//Called by the ItemObject - Show on the playerHead
		public void TakeItem(ItemManager.ItemType itemType, int maxUses){
			//Set the item visiable and with the right sprite
			if(!itemHolderSprite.activeSelf){
				itemHolderSprite.SetActive(true);
			}
			itemSprite.GetComponent<SpriteRenderer>().sprite = im.GetItemSprite(itemType);

				
			//Animate it to show and them to keep moving slightly
			itemHolderSprite.transform.DOScale(Vector3.one*1.1f, 0.25f).OnComplete(() => {
				itemSprite.transform.DOScale(Vector3.one*0.5f, 0.25f);
			});

			//set the currentItem
			currentItem = itemType;
			maxUsesOfItem = maxUses;
			currentUseOfItem = 0;

			//Set it is usign one
			haveItem = true;
			canUseItem = true;

		}

		private void UseItem(){
			canUseItem = false;

			//If still have the item
			if(currentUseOfItem < maxUsesOfItem){

				//Animate
				if(currentItem == ItemManager.ItemType.BALL){
					an.SetTrigger("Throw");
					AudioStuff.PlaySound("ball");
				}
				else if(currentItem == ItemManager.ItemType.LION){
					an.SetTrigger("Summon");
					AudioStuff.PlaySound("lion");
				}

				//Use it
				im.UseItem(currentItem, originPosition);
				currentUseOfItem++;

				if (currentUseOfItem == maxUsesOfItem) {
					endItem ();
				} else {
					canUseItem = true;
				}
			}
		}

		private void endItem(){
			//Animate it to hide
			itemHolderSprite.transform.DOScale(Vector3.zero,0.25f);
			itemSprite.transform.DOScale(Vector3.zero,0.25f);
		}
	}
}