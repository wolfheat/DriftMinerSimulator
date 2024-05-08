using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] Light lights;

    float perlinPosX = 0f;
    float perlinPosY = 0f;

    // Update is called once per frame
    void Update()
    {
        float perlinA = Mathf.PerlinNoise(perlinPosX, 0.1f);
        float perlinB = Mathf.PerlinNoise(perlinPosX, 10f);
        //Vector3 newPosition = new Vector3(perlinA-0.5f, 0, 0);
        Vector3 newPosition = new Vector3(perlinA-0.5f, 0, perlinB-0.5f)*0.2f;
        transform.localPosition = newPosition;

        lights.intensity = 1 + 2*Mathf.PerlinNoise(perlinA, 0.3f);
            
        if(perlinPosX >= 50)
            perlinPosY = (perlinPosY + Random.Range(0,5f)) % 50;
        perlinPosX = (perlinPosX + Time.deltaTime)%50;
    }
}
