#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

[CustomEditor(typeof(SoundsSO))]
public class SoundsSOEditor : Editor
{
    private void OnEnable()
    {
        ref SoundList[] soundList = ref ((SoundsSO)target).sounds;
        if (soundList == null) return;

        string[] names = Enum.GetNames(typeof(SoundType));
        bool differentSize = names.Length != soundList.Length;
        Dictionary<string, SoundList> sounds = new();

        if (differentSize)
        {
            for (int i = 0; i < soundList.Length; ++i)
                sounds.Add(soundList[i].name, soundList[i]);
        }

        Array.Resize(ref soundList, names.Length);

        for (int i = 0; i < soundList.Length; i++)
        {
            string currentName = names[i];
            soundList[i].name = currentName;
            if (soundList[i].volume == 0) soundList[i].volume = 1;

            if (differentSize)
            {
                if (sounds.ContainsKey(currentName))
                {
                    SoundList current = sounds[currentName];
                    soundList[i].volume = current.volume;
                    soundList[i].sounds = current.sounds;
                    soundList[i].mixer = current.mixer;
                }
                else
                {
                    soundList[i].volume = 1;
                    soundList[i].sounds = new AudioClip[0];
                    soundList[i].mixer = null;
                }
            }
        }
    }
}
#endif