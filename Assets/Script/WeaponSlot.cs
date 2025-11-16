using UnityEngine;

[System.Serializable]
public class WeaponSlot
{
    public string weaponName;
    public int ammo;
    public GameObject bulletPrefab;
    public bool isUnlocked = false;
}
