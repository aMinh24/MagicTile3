using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : BaseManager<AudioManager>
{
	// public AudioClip[] bgmList;
	// public AudioClip[] seList;
	private float bgmFadeSpeedRate = CONST.BGM_FADE_SPEED_RATE_HIGH;

	//Next BGM name, SE name
	private string nextBGMName;
	private string nextSEName;

	//Is the background music fading out?
	private bool isFadeOut = false;

	//Separate audio sources for BGM and SE
	public AudioSource AttachBGMSource;
	public AudioSource AttachSESource;

	//Keep All Audio
	private Dictionary<string, AudioClip> bgmDic, seDic;

    protected override void Awake()
    {
        base.Awake();
		//Load all SE & BGM files from resource folder
		bgmDic = new Dictionary<string, AudioClip>();
		seDic = new Dictionary<string, AudioClip>();

		
		object[] bgmList = Resources.LoadAll("Audio/BGM");
		object[] seList = Resources.LoadAll("Audio/SE");

		foreach (AudioClip bgm in bgmList)
		{
			bgmDic[bgm.name] = bgm;
		}
		foreach (AudioClip se in seList)
		{
			seDic[se.name] = se;
		}
	}

    private void Start()
	{
		AttachBGMSource.volume = CONST.BGM_VOLUME_DEFAULT;
		AttachSESource.volume = CONST.SE_VOLUME_DEFAULT;
		AttachBGMSource.mute = CONST.BGM_MUTE_DEFAULT;
		AttachSESource.mute = CONST.SE_MUTE_DEFAULT;
	}

	public void PlaySE(string seName, float delay = 0.0f)
	{
		if (!seDic.ContainsKey(seName))
		{
			Debug.Log(seName + " There is no SE named");
			return;
		}

		nextSEName = seName;
		if(delay > 0f)
		{
        	Invoke(nameof(DelayPlaySE), delay);
		}
		else if (delay == 0f){
			DelayPlaySE();
		}
	}

	private void DelayPlaySE()
	{
		AttachSESource.PlayOneShot(seDic[nextSEName] as AudioClip);
	}

	public void PlayBGM(string bgmName, float fadeSpeedRate = CONST.BGM_FADE_SPEED_RATE_HIGH)
	{
		if (!bgmDic.ContainsKey(bgmName))
		{
			return;
		}

		//If BGM is not currently playing, play it as is
		if (!AttachBGMSource.isPlaying)
		{
			nextBGMName = "";
			AttachBGMSource.clip = bgmDic[bgmName];
			AttachBGMSource.loop = true;
			AttachBGMSource.Play();
		}
		//When a different BGM is playing, fade out the BGM that is playing before playing the next one.
        //Ignore when the same BGM is playing
		else if (AttachBGMSource.clip.name != bgmName)
		{
			nextBGMName = bgmName;
			FadeOutBGM(fadeSpeedRate);
		}

	}

	public void FadeOutBGM(float fadeSpeedRate = CONST.BGM_FADE_SPEED_RATE_LOW)
	{
		bgmFadeSpeedRate = fadeSpeedRate;
		isFadeOut = true;
	}

	private void Update()
	{
		if (!isFadeOut)
		{
			return;
		}

		//Gradually lower the volume, and when the volume reaches 0
		//return the volume and play the next song
		AttachBGMSource.volume -= Time.deltaTime * bgmFadeSpeedRate;
		if (AttachBGMSource.volume <= 0)
		{
			AttachBGMSource.Stop();
			AttachBGMSource.volume = CONST.BGM_VOLUME_DEFAULT;
			isFadeOut = false;

			if (!string.IsNullOrEmpty(nextBGMName))
			{
				PlayBGM(nextBGMName);
			}
		}
	}

	public void ChangeBGMVolume(float BGMVolume)
	{
		AttachBGMSource.volume = BGMVolume;
	}

	public void ChangeSEVolume(float SEVolume)
	{
		AttachSESource.volume = SEVolume;
	}

	public void MuteBGM(bool isMute)
    {
		AttachBGMSource.mute = isMute;
	}

	public void MuteSE(bool isMute)
	{
		AttachSESource.mute = isMute;
	}
}