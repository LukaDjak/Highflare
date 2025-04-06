using UnityEngine;
using TMPro;
using System.Collections;

public class GunUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI ammoText;
    public Transform playerSocket;

    private Gun currentGun;
    private Color defaultColor;
    private bool isFlashing = false;

    // Ammo low threshold to trigger the flashing
    public int lowAmmoThreshold = 5;
    public float flashDuration = 0.5f; // How fast it flashes

    void Start()
    {
        // Store the default text color
        defaultColor = ammoText.color;
    }

    void Update()
    {
        if (playerSocket.childCount > 0)
        {
            if (currentGun == null || currentGun.gameObject != playerSocket.GetChild(0).gameObject)
            {
                currentGun = playerSocket.GetChild(0).GetComponent<Gun>();
            }

            if (currentGun != null)
            {
                // Format the text: larger bullet count, smaller magazine size
                ammoText.text = $"<size=72>{currentGun.bulletsLeft}</size> <size=56>/ {currentGun.magazineSize}</size>";

                // Check if ammo is low and start flashing
                if (currentGun.bulletsLeft <= lowAmmoThreshold && !isFlashing)
                    StartCoroutine(FlashAmmoUI());
                else if (currentGun.bulletsLeft > lowAmmoThreshold && isFlashing)
                {
                    StopCoroutine(FlashAmmoUI());
                    ammoText.color = defaultColor; // Reset color when ammo is not low
                    isFlashing = false;
                }
            }
        }
        else
        {
            currentGun = null;
            ammoText.text = "<size=72>--</size> <size=56>/ --</size>";
            ammoText.color = defaultColor; // Reset color if no weapon is picked up
        }
    }

    // Coroutine to handle the flashing effect
    private IEnumerator FlashAmmoUI()
    {
        isFlashing = true;

        while (isFlashing)
        {
            ammoText.color = Color.red; // Flash red
            yield return new WaitForSeconds(flashDuration);

            ammoText.color = defaultColor; // Reset to default color
            yield return new WaitForSeconds(flashDuration);
        }
    }
}