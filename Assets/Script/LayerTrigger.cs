﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    //when object exit the trigger, put it to the assigned layer and sorting layers
    //used in the stair objects for player to travel between layers
    public class LayerTrigger : MonoBehaviour
    {
        public string layer;
        public string sortingLayer;
        /*
        private void OnTriggerExit2D(Collider2D other)
        {
            //这是一个私有的方法，
            //有一个参数other，它是一个Collider2D类型的对象，表示触发器碰撞到的其他对象。
            //这个方法会在其他对象离开触发器时自动调用。
            other.gameObject.layer = LayerMask.NameToLayer(layer);

            other.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
            SpriteRenderer[] srs = other.gameObject.GetComponentsInChildren<SpriteRenderer>();
            foreach ( SpriteRenderer sr in srs)
            {
                sr.sortingLayerName = sortingLayer;
            }
        }
        */
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other == null) return;

            other.gameObject.layer = LayerMask.NameToLayer(layer);

            SpriteRenderer sr = other.gameObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = sortingLayer;
            }


            /*
            SpriteRenderer[] srs = other.gameObject.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer childSr in srs)
            {
                if (childSr != null)
                {
                    childSr.sortingLayerName = sortingLayer;
                }
            }
            */

            Transform[] children = other.gameObject.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child != null)
                {
                    child.gameObject.layer = LayerMask.NameToLayer(layer);

                    SpriteRenderer childSr = child.gameObject.GetComponent<SpriteRenderer>();
                    if (childSr != null)
                    {
                        childSr.sortingLayerName = sortingLayer;
                    }
                    Canvas childCa = child.gameObject.GetComponent<Canvas>();
                    if (childCa != null)
                    {
                        childCa.sortingLayerName = sortingLayer;
                    }
                }
            }
        }

    }
}
