using UnityEngine;

public class CasinoBootstrap : MonoBehaviour
{
    public PlayerWallet walletPrefab;      // drag a prefab or a scene object
    public PlayerUpgrades upgradesPrefab;  // drag a prefab or a scene object

    void Awake()
    {
        if (PlayerWallet.Instance == null)
        {
            var w = walletPrefab ? Instantiate(walletPrefab) : FindFirstObjectByType<PlayerWallet>(UnityEngine.FindObjectsInactive.Include);

            if (w) DontDestroyOnLoad(w.gameObject);
        }

        if (PlayerUpgrades.Instance == null)
        {
            var u = upgradesPrefab ? Instantiate(upgradesPrefab) : FindFirstObjectByType<PlayerUpgrades>(UnityEngine.FindObjectsInactive.Include);

            if (u) DontDestroyOnLoad(u.gameObject);
        }
    }
}
