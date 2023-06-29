using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartScript : MonoBehaviour
{
    public GameObject CutImage;
    // Start is called before the first frame update
    void Start()
    {
        CutImage.GetComponent<CutScript>().CutFadeOut();
        StartCoroutine(EndCutOut(1f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator EndCutOut(float time)
    {
        yield return new WaitForSeconds(time);
        CutImage.SetActive(false);
    }
}
