﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eSongName { nineteenForty, never, ninja, soap, things, startBattle, win, lose };
public enum eSoundName { dialogue, selection, damage1, damage2, damage3, death, confirm, deny, 
	run, fireball, fireblast, buff1, buff2, highBeep1, highBeep2};

public class AudioManager : MonoBehaviour {
	[Header ("Set in Inspector")]
	public List <AudioSource>	bgmCS = new List<AudioSource>();
	public List <AudioSource>	sfxCS = new List<AudioSource>();

	[Header("Set Dynamically")]
	public int 					currentSongNdx;

	public AudioListener		audioListenerCS;

	// Singleton
	private static AudioManager _S;
	public static AudioManager S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	void Start() {
		audioListenerCS = GetComponent<AudioListener>();

		// Add Loop() to UpdateManager
		UpdateManager.updateDelegate += Loop;
	}

    public void Loop(){
		if (Input.GetKeyDown (KeyCode.M)) {
			PauseMuteSong();
		}
	}

	public void PlaySong(int ndx = 0){
		// Change index
		currentSongNdx = ndx;

		// Stop ALL BGM
		for (int i = 0; i < bgmCS.Count; i++) {
			bgmCS[i].Stop();
		}

		if (!AudioListener.pause) {
			bgmCS[ndx].Play();
		}
	}

	public void PlaySong(eSongName songName) {
		// Change index
		currentSongNdx = (int)songName;

		// Stop ALL BGM
		for (int i = 0; i < bgmCS.Count; i++) {
			bgmCS[i].Stop();
		}

		switch (songName) {
			case eSongName.nineteenForty:
				bgmCS[0].Play();
				break;
			case eSongName.never:
				bgmCS[1].Play();
				break;
			case eSongName.ninja:
				bgmCS[2].Play();
				break;
			case eSongName.soap:
				bgmCS[3].Play();
				break;
			case eSongName.things:
				bgmCS[4].Play();
				break;
			case eSongName.startBattle:
				bgmCS[5].Play();
				break;
			case eSongName.win:
				bgmCS[6].Play();
				break;
			case eSongName.lose:
				bgmCS[7].Play();
				break;
		}
	}

	public void PauseMuteSong(){
		if (!AudioListener.pause) {
			AudioListener.volume = 0;
			AudioListener.pause = true;

			bgmCS[currentSongNdx].Pause();
		} else {
			AudioListener.volume = 1;
			AudioListener.pause = false;

			bgmCS[currentSongNdx].Play();
		}
	}

	public void PlaySFX(int ndx) {
 		sfxCS[ndx].Play();
    }

	public void PlaySFX(eSoundName soundName) {
        switch (soundName) {
			case eSoundName.dialogue:
				sfxCS[0].Play();
				break;
			case eSoundName.selection:
				sfxCS[1].Play();
				break;
			case eSoundName.damage1:
				sfxCS[2].Play();
				break;
			case eSoundName.damage2:
				sfxCS[3].Play();
				break;
			case eSoundName.damage3:
				sfxCS[4].Play();
				break;
			case eSoundName.death:
				sfxCS[5].Play();
				break;
			case eSoundName.confirm:
				sfxCS[6].Play();
				break;
			case eSoundName.deny:
				sfxCS[7].Play();
				break;
			case eSoundName.run:
				sfxCS[8].Play();
				break;
			case eSoundName.fireball:
				sfxCS[9].Play();
				break;
			case eSoundName.fireblast:
				sfxCS[10].Play();
				break;
			case eSoundName.buff1:
				sfxCS[11].Play();
				break;
			case eSoundName.buff2:
				sfxCS[12].Play();
				break;
			case eSoundName.highBeep1:
				sfxCS[13].Play();
				break;
			case eSoundName.highBeep2:
				sfxCS[14].Play();
				break;
		}
	}
}
