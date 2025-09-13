using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterVoiceSet", menuName = "Audio/Character Voice Set")]
public class CharacterVoiceSet : ScriptableObject
{
    [Header("Jump Sounds")]
    public List<AudioClip> jumpLow;
    public List<AudioClip> jumpMid;
    public List<AudioClip> jumpHigh;
}
