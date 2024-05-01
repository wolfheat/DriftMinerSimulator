using System;
using System.Linq;
using UnityEngine;

namespace Wolfheat.StartMenu
{
    public class MeteorController : MonoBehaviour
{
    [SerializeField] RectTransform[] meteors;
    private float moveSpeed = 10f;
    private Vector2 moveDirection = new Vector3(-1,-1);

    private void Start()
    {
        
    }

    private void Update()
    {
        foreach (var meteor in meteors)
        {
            Vector2 pos = meteor.anchoredPosition;
                pos += moveDirection.normalized*moveSpeed;
            if (pos.y < -900 || pos.x < -20f)
            {
                pos = GetRandomStart();
            }
            meteor.anchoredPosition = pos;
        }
    }

    private Vector3 GetRandomStart()
    {
        return new Vector3(UnityEngine.Random.Range(10, 2500), 20f,0);
    }
}
}
