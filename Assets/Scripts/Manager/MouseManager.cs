using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class MouseManager : Singleton<MouseManager>
{
    //public EventVector3 OnMouseClicked;
    public event Action<Vector3> OnMouseClicked;

    public event Action<GameObject> OnEnemyClicked;

    public Texture2D point, attack, arrow, target;

    RaycastHit hitInfo;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }
    void SetCursorTexture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray,out hitInfo))
        {
            switch(hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(point, new Vector2(8, 0),CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(0, 0), CursorMode.Auto);
                    break;
            }
        }
    }
    void MouseControl()
    {
        if (Input.GetMouseButtonDown(0)&&hitInfo.collider!=null)
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
                OnMouseClicked?.Invoke(hitInfo.point);
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
        }
    }
}
