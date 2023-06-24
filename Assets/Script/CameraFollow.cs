using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    //摄像头跟随脚本
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;//要跟随的物体、角色
        public float lerpSpeed = 1.0f;//跟随速度

        private Vector3 offset;

        private Vector3 targetPos;

        private void Start()
        {
            if (target == null) return;

            offset = transform.position - target.position;//摄像头相对物体的偏移值
        }

        private void Update()
        {
            if (target == null) return;

            targetPos = target.position + offset;//物体位置加偏移值即为摄像头的目标位置
            //每一帧使摄像头向目标位置移动"速度*每帧时间"的长度
            transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);
        }

    }
}
