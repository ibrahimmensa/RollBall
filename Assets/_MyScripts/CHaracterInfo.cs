using UnityEngine;
using System;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class CHaracterInfo : ScriptableObject
{
    public CharacterDetails[] CD = new CharacterDetails[11];
}
[Serializable]
public class CharacterDetails
{
    public string Character_Name;
    public bool CanUnlockByAds;
    public bool ComingSoon;
    public bool IsLocked = true;
    public int AdsToWatch_ToUnlock;
    public int AdsWatched;
    public int NeedCoins_ToUnlock;
    public bool isSelected = false;
    public Sprite CHaracterImg;
}
