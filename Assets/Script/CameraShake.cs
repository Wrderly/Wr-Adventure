using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // ���������
    public float shakeDuration = 0.1f; // ��ͷ�����ĳ���ʱ��
    public float shakeAmplitude = 0.1f; // ��ͷ�����ķ���
    public float shakeFrequency = 0.1f; // ��ͷ������Ƶ��

    private CinemachineBasicMultiChannelPerlin noise; // �������

    private void Start()
    {
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>(); // ��ȡ�������
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
        noise.m_AmplitudeGain = shakeAmplitude; // ������������
        noise.m_FrequencyGain = shakeFrequency; // ��������Ƶ��

        yield return new WaitForSeconds(shakeDuration); // �ȴ�һ��ʱ��

        noise.m_AmplitudeGain = 0f; // �ָ���������
        noise.m_FrequencyGain = 0f; // �ָ�����Ƶ��
    }

    private IEnumerator DoShake(float _shakeAmplitude, float _shakeFrequency, float _shakeDuration)
    {
        noise.m_AmplitudeGain = _shakeAmplitude; // ������������
        noise.m_FrequencyGain = _shakeFrequency; // ��������Ƶ��

        yield return new WaitForSeconds(_shakeDuration); // �ȴ�һ��ʱ��

        noise.m_AmplitudeGain = 0f; // �ָ���������
        noise.m_FrequencyGain = 0f; // �ָ�����Ƶ��
    }
}
