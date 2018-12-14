using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour {

    #region 单例
    private static AudioController instance;
    public static AudioController Instance
    {
        get { return instance; }
    }
    #endregion

    #region 私有字段
    List<AudioSource> audioSources;
    Coroutine strengthen;

    [Header("音乐音量"), Range(0f, 1f), SerializeField]
    private float musicVolume = 1f;
    [Header("淡出/入时间"), Range(1f, 5f), SerializeField]
    private float musicFadeOutTime = 1f;
    #endregion

    // Use this for initialization
    void Start () {

		if(instance)
        {
            DestroyImmediate(gameObject);
            return;
        }
        else
        {
            audioSources = new List<AudioSource>();
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

	}

    #region 播放音乐方法重载
    public void PlayMusic(AudioClip audioClip)
    {
        AudioSource audioSource = GetFreeAudioSource();
        audioSource.clip = audioClip;
        audioSource.loop = true;
        //audioSource.volume = 0;
        audioSource.Play();
        //if (strengthen != null)
        //    StopCoroutine(strengthen);
        //strengthen = StartCoroutine(Strengthen(audioSource));
    }

    public void PlayMusic(AudioClip audioClip, float volumn)
    {
        musicVolume = volumn;
        PlayMusic(audioClip);
    }

    public void PlayMusic(string str)
    {
        Debug.Log("PlayMusic");
        AudioClip ac = Resources.Load<AudioClip>(str);
        PlayMusic(ac);
    }

    public void PlayMusic(string str,float volumn)
    {
        AudioClip ac = Resources.Load<AudioClip>(str);
        musicVolume = volumn;
        PlayMusic(ac);
    }

    IEnumerator Strengthen(AudioSource audioSource)
    {
        float speed = (musicVolume - audioSource.volume) / musicFadeOutTime;
        while (audioSource.volume < musicVolume)
        {
            audioSource.volume += Time.deltaTime * speed;
            yield return new WaitForFixedUpdate();
        }
        yield return 0;
    }
    
    public AudioSource GetFreeAudioSource()
    {
        AudioSource ret = null;
        foreach(AudioSource a in audioSources)
        {
            if(!a.isPlaying && ret==null)
            {
                ret = a;
                break;
            }
            //else if(!a.isPlaying)
            //{
            //    Destroy(a);
            //}
        }
        if (ret == null)
        {
            ret = gameObject.AddComponent<AudioSource>();
            audioSources.Add(ret);
        }
        return ret;
    }
    #endregion

    #region 播放音效
    public void PlayAudioEffect(AudioClip audioClip,float volume)
    {
        AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position,volume);
    }
    public void PlayAudioEffect(string str, float volume)
    {
        AudioClip ac = Resources.Load<AudioClip>(str);
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position, volume);
    }
    public void PlayAudioEffect(string str)
    {
        PlayAudioEffect(str, 1);
    }
    public void PlayAudioEffect(AudioClip audioClip)
    {
        PlayAudioEffect(audioClip,1);
    }
    #endregion

    #region 停止音乐
    public void StopGivenNumOfCertainMusic(string str,int num)
    {
        AudioClip ac = Resources.Load<AudioClip>(str);
        if (ac == null)
            return;
        foreach(AudioSource a in audioSources)
        {
            if(a.clip==ac && a.isPlaying && num>0)
            {
                a.Stop();
                num--;
            }
            if(num<=0)
            {
                break;
            }
        }
    }

    public void StopCertainMusic(string str)
    {
        StopGivenNumOfCertainMusic(str, 1);
    }

    public void StopCertainMusics(string str)
    {
        StopGivenNumOfCertainMusic(str, 100);
    }
    
    public void StopAllMusics()
    {
        foreach (AudioSource a in audioSources)
        {
            if(a.isPlaying)
                a.Stop();
        }
    }
    #endregion

}
