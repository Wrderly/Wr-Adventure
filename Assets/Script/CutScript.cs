using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutScript : MonoBehaviour
{
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CutFadeIn()
    {
        animator.Play("CutFadeIn");
    }

    public void CutFadeOut()
    {
        animator.Play("CutFadeOut");
    }
}
