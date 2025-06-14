using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StarAnimation : MonoBehaviour
{
    public Image img;
    public void ChangeStatus()
    {
        img.DOColor(Color.yellow, 0.5f);
    }
}
