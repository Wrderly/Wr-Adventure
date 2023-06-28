using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // 虚拟摄像机
    public float shakeDuration = 0.1f; // 镜头抖动的持续时间
    public float shakeAmplitude = 0.1f; // 镜头抖动的幅度
    public float shakeFrequency = 0.1f; // 镜头抖动的频率

    private CinemachineBasicMultiChannelPerlin noise; // 噪声组件

    private void Start()
    {
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>(); // 获取噪声组件
    }

    public void Shake()
    {
        StartCoroutine(DoShake());
    }

    public void Shake(float _shakeAmplitude, float _shakeFrequency, float _shakeDuration = 0.1f)
    {
        StartCoroutine(DoShake(_shakeAmplitude, _shakeFrequency, _shakeDuration));
    }

    private IEnumerator DoShake()
    {
        noise.m_AmplitudeGain = shakeAmplitude; // 设置噪声幅度
        noise.m_FrequencyGain = shakeFrequency; // 设置噪声频率

        yield return new WaitForSeconds(shakeDuration); // 等待一段时间

        noise.m_AmplitudeGain = 0f; // 恢复噪声幅度
        noise.m_FrequencyGain = 0f; // 恢复噪声频率
    }

    private IEnumerator DoShake(float _shakeAmplitude, float _shakeFrequency, float _shakeDuration)
    {
        noise.m_AmplitudeGain = _shakeAmplitude; // 设置噪声幅度
        noise.m_FrequencyGain = _shakeFrequency; // 设置噪声频率

        yield return new WaitForSeconds(_shakeDuration); // 等待一段时间

        noise.m_AmplitudeGain = 0f; // 恢复噪声幅度
        noise.m_FrequencyGain = 0f; // 恢复噪声频率
    }
}
