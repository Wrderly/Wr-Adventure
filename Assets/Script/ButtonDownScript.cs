using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonDownScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Image buttonOriImage;
    //public Sprite 
    public Sprite buttonDownSprite;
    private Sprite buttonUpSprite;
    public GameObject text;
    private Vector3 upPosition;
    private Vector3 downPosition;
    private Quaternion quaternion;

    //private TextMeshPro text;
    void Start()
    {
        //text = textObject.GetComponent<TextMeshPro>();
        buttonUpSprite = buttonOriImage.sprite;
        upPosition = text.GetComponent<RectTransform>().localPosition;
        downPosition = upPosition;
        quaternion = text.GetComponent<RectTransform>().localRotation;
        downPosition.y *= -1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonDown()
    {
        buttonOriImage.sprite = buttonDownSprite;
        text.GetComponent<RectTransform>().SetLocalPositionAndRotation(downPosition, quaternion);


        StartCoroutine(ButtonUp());
    }

    private IEnumerator ButtonUp()
    {
        yield return new WaitForSeconds(0.1f);
        buttonOriImage.sprite = buttonUpSprite;
        text.GetComponent<RectTransform>().SetLocalPositionAndRotation(upPosition, quaternion);
    }
}
