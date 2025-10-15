using UnityEngine;
using System.Reflection;

public class GodArsenalCheat : MonoBehaviour
{
    [Header("References (optional - will try to auto-find)")]
    public PlayerHealth playerHealth;
    public PlayerShooting playerShooting;
    public WeaponSlotManager slotManager;

    [Header("Cheat Settings")]
    public KeyCode toggleKey = KeyCode.F5;
    public int infiniteAmmoAmount = 9999;

    bool isOn = false;

    // saved originals
    bool origInvincible = false;
    int origMGAmmo = -1, origSGAmmo = -1, origHGAmmo = -1;

    void Awake()
    {
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerShooting == null) playerShooting = FindObjectOfType<PlayerShooting>();
        if (slotManager == null) slotManager = FindObjectOfType<WeaponSlotManager>();

        // read original ammo values (best-effort: try properties then fields)
        origMGAmmo = GetIntFromPlayerShootingFieldOrProp("machineGunAmmo", "MachineGunAmmo", defaultValue: -1);
        origSGAmmo = GetIntFromPlayerShootingFieldOrProp("shotgunAmmo", "ShotgunAmmo", defaultValue: -1);
        origHGAmmo = GetIntFromPlayerShootingFieldOrProp("homingAmmo", "HomingAmmo", defaultValue: -1);

        // read original invincible (best-effort)
        origInvincible = GetBoolFromPlayerHealthFieldOrProp("isInvincible", "IsInvincible", defaultValue: false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isOn = !isOn;
            if (isOn) EnableGodArsenal();
            else DisableGodArsenal();

            Debug.Log($"GodArsenalCheat: {(isOn ? "ON" : "OFF")}");
        }
    }

    void EnableGodArsenal()
    {
        // 1) Set invincible true if possible
        SetPlayerHealthBool("isInvincible", "IsInvincible", true);

        // 2) Give all guns + set large ammo
        if (playerShooting != null)
        {
            // call public pickup methods if available
            MethodInfo mg = playerShooting.GetType().GetMethod("PickupMachineGun", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo sg = playerShooting.GetType().GetMethod("PickupShotgun", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo hg = playerShooting.GetType().GetMethod("PickupHomingGun", BindingFlags.Instance | BindingFlags.Public);

            // if pickup methods exist and bullet prefabs available on playerShooting, call them
            object[] mgArgs = null, sgArgs = null, hgArgs = null;

            // try to read prefab fields
            var mgPrefab = GetObjectFieldOrProp(playerShooting, "machineGunBulletPrefab", "machineGunBulletPrefab", "machineGunBulletPrefab");
            var sgPrefab = GetObjectFieldOrProp(playerShooting, "shotgunBulletPrefab", "shotgunBulletPrefab", "shotgunBulletPrefab");
            var hgPrefab = GetObjectFieldOrProp(playerShooting, "homingBulletPrefab", "homingBulletPrefab", "homingBulletPrefab");

            if (mg != null)
                mgArgs = new object[] { infiniteAmmoAmount, mgPrefab as GameObject };
            if (sg != null)
                sgArgs = new object[] { infiniteAmmoAmount, sgPrefab as GameObject };
            if (hg != null)
                hgArgs = new object[] { infiniteAmmoAmount, hgPrefab as GameObject };

            // call pickups (best-effort)
            if (mg != null && mgArgs != null) mg.Invoke(playerShooting, mgArgs);
            else SetIntOnPlayerShooting("machineGunAmmo", infiniteAmmoAmount);
            if (sg != null && sgArgs != null) sg.Invoke(playerShooting, sgArgs);
            else SetIntOnPlayerShooting("shotgunAmmo", infiniteAmmoAmount);
            if (hg != null && hgArgs != null) hg.Invoke(playerShooting, hgArgs);
            else SetIntOnPlayerShooting("homingAmmo", infiniteAmmoAmount);

            // also directly set ammo fields/properties to ensure infinite
            SetIntOnPlayerShooting("machineGunAmmo", infiniteAmmoAmount);
            SetIntOnPlayerShooting("shotgunAmmo", infiniteAmmoAmount);
            SetIntOnPlayerShooting("homingAmmo", infiniteAmmoAmount);
        }

        // 3) If weapon slot manager exists, add weapons to slots (so UI shows them)
        if (slotManager != null)
        {
            // try to add MachineGun, Shotgun, HomingGun using known method AddWeaponToSlot(string,int,GameObject)
            MethodInfo addMethod = slotManager.GetType().GetMethod("AddWeaponToSlot", BindingFlags.Instance | BindingFlags.Public);
            if (addMethod != null)
            {
                var mgPrefab = GetObjectFieldOrProp(playerShooting, "machineGunBulletPrefab", "machineGunBulletPrefab", "machineGunBulletPrefab") as GameObject;
                var sgPrefab = GetObjectFieldOrProp(playerShooting, "shotgunBulletPrefab", "shotgunBulletPrefab", "shotgunBulletPrefab") as GameObject;
                var hgPrefab = GetObjectFieldOrProp(playerShooting, "homingBulletPrefab", "homingBulletPrefab", "homingBulletPrefab") as GameObject;

                addMethod.Invoke(slotManager, new object[] { "MachineGun", infiniteAmmoAmount, mgPrefab });
                addMethod.Invoke(slotManager, new object[] { "Shotgun", infiniteAmmoAmount, sgPrefab });
                addMethod.Invoke(slotManager, new object[] { "HomingGun", infiniteAmmoAmount, hgPrefab });
            }
        }
    }

    void DisableGodArsenal()
    {
        // restore invincible
        SetPlayerHealthBool("isInvincible", "IsInvincible", origInvincible);

        // restore ammo values if we read them earlier
        if (origMGAmmo >= 0) SetIntOnPlayerShooting("machineGunAmmo", origMGAmmo);
        if (origSGAmmo >= 0) SetIntOnPlayerShooting("shotgunAmmo", origSGAmmo);
        if (origHGAmmo >= 0) SetIntOnPlayerShooting("homingAmmo", origHGAmmo);

        Debug.Log("GodArsenalCheat: restored ammo/invincible where possible.");
    }

    // ---- helpers (best-effort using reflection) ----
    int GetIntFromPlayerShootingFieldOrProp(string fieldName, string propName, int defaultValue = 0)
    {
        if (playerShooting == null) return defaultValue;
        var prop = playerShooting.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null)
        {
            object v = prop.GetValue(playerShooting);
            if (v is int) return (int)v;
        }

        var field = playerShooting.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            object v = field.GetValue(playerShooting);
            if (v is int) return (int)v;
        }
        return defaultValue;
    }

    void SetIntOnPlayerShooting(string fieldName, int value)
    {
        if (playerShooting == null) return;
        var prop = playerShooting.GetType().GetProperty(propNameToProp(fieldName), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(playerShooting, value);
            return;
        }
        var field = playerShooting.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null) field.SetValue(playerShooting, value);
    }

    // helper to try common prop name mapping (machineGunAmmo -> MachineGunAmmo)
    string propNameToProp(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) return fieldName;
        string s = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
        return s;
    }

    void SetPlayerHealthBool(string fieldName, string propName, bool value)
    {
        if (playerHealth == null) return;
        var f = playerHealth.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (f != null) { f.SetValue(playerHealth, value); return; }
        var p = playerHealth.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanWrite) p.SetValue(playerHealth, value);
    }

    bool GetBoolFromPlayerHealthFieldOrProp(string fieldName, string propName, bool defaultValue = false)
    {
        if (playerHealth == null) return defaultValue;
        var p = playerHealth.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null)
        {
            object v = p.GetValue(playerHealth);
            if (v is bool) return (bool)v;
        }
        var f = playerHealth.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null)
        {
            object v = f.GetValue(playerHealth);
            if (v is bool) return (bool)v;
        }
        return defaultValue;
    }

    object GetObjectFieldOrProp(object obj, string fieldName, string propName, string fallback)
    {
        if (obj == null) return null;
        var p = obj.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null) return p.GetValue(obj);
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null) return f.GetValue(obj);

        // try fallback name
        var f2 = obj.GetType().GetField(fallback, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f2 != null) return f2.GetValue(obj);

        return null;
    }

    bool GetBoolFromPlayerShooting(string propName)
    {
        if (playerShooting == null) return false;
        var p = playerShooting.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null)
        {
            object v = p.GetValue(playerShooting);
            if (v is bool) return (bool)v;
        }
        var f = playerShooting.GetType().GetField(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null)
        {
            object v = f.GetValue(playerShooting);
            if (v is bool) return (bool)v;
        }
        return false;
    }

    // wrapper to read bool from playerHealth (used for saving)
    bool GetBoolFromPlayerHealthFieldOrProp(string fieldName, string propName)
    {
        return GetBoolFromPlayerHealthFieldOrProp(fieldName, propName, false);
    }
}
