using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float range;
    [SerializeField] private float impactForce;
    [SerializeField] private float fireRate;
    [SerializeField] private float reloadTime;
    private float nextTimeToFire = 0f;

    [SerializeField] private int maxAmo;
    private int currentAmo;

    private bool isReloading;

    private Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public Animator gunAnimator;
    public Animator ammoIconAnimator;
    private Text ammoCounter;

    void Start()
    {
        currentAmo = maxAmo;
        isReloading = false;
        fpsCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        ammoIconAnimator.SetInteger("AmmoCount", currentAmo);
        ammoCounter = GameObject.FindGameObjectWithTag("AmmoCounter").GetComponent<Text>();
    }

    private void OnEnable()
    {
        isReloading = false;
        gunAnimator.SetBool("Reloading", false);
        ammoIconAnimator.SetBool("IsReloading", false);
    }

    void Update()
    {
        ammoIconAnimator.SetInteger("AmmoCount", currentAmo);

        if (isReloading) 
        {
            return;
        }

        if (currentAmo <= 0 || Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
            ammoIconAnimator.SetBool("IsShooting", false);
        }

        ShowAmmo();
    }

    void Shoot() 
    {
        ammoIconAnimator.SetBool("IsShooting", true);
        muzzleFlash.Play();

        currentAmo--;

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.transform.name);

            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            impactGO.GetComponent<ParticleSystem>().Play();
            Destroy(impactGO, 2f);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;

        gunAnimator.SetBool("Reloading", true);
        ammoIconAnimator.SetBool("IsReloading", true);

        yield return new WaitForSeconds(reloadTime - 0.25f);

        gunAnimator.SetBool("Reloading", false);
        ammoIconAnimator.SetBool("IsReloading", false);
        yield return new WaitForSeconds(0.25f);

        currentAmo = maxAmo;
        isReloading = false;
    }

    private void ShowAmmo()
    {
        ammoCounter.text = "" + currentAmo;
    }
}
