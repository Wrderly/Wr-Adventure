using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodBarController : MonoBehaviour
{
    // Start is called before the first frame update
    public Image bloodBar;
    public Image bloodBarBackground;
    public GameObject entity;
    public Transform follow;
    Vector3 offset;

    private const float bloobBarLerpSpeed = 3f;
    public float healthPercentage;

    private EnemyAttribute attribute;
    void Start()
    {
        if (follow == null) return;
        offset = transform.position - follow.position;
        attribute = entity.GetComponent<IEnemyAttribute>().attribute;
    }

    // Update is called once per frame
    void Update()
    {
        if (follow == null) return;


        transform.position = follow.position + offset;
        SetBloodBar();
    }



    public void SetBloodBar()
    {
        bloodBar.fillAmount = Mathf.Lerp(bloodBar.fillAmount, attribute.health / attribute.maxHealth, bloobBarLerpSpeed * Time.deltaTime);
    }

    public void SetBloodBar(float newHealthPercentage)
    {
        healthPercentage = newHealthPercentage;
        StartCoroutine(UpDateBloodBar());
    }

    private IEnumerator UpDateBloodBar()
    {
        float timer = 1f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            bloodBar.fillAmount = Mathf.Lerp(bloodBar.fillAmount, healthPercentage, bloobBarLerpSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
