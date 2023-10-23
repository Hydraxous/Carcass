using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemIdentifier : MonoBehaviour
{
    public bool infiniteSource;
    public bool pickedUp;
    public bool reverseTransformSettings;
    public Vector3 putDownPosition;
    public Vector3 putDownRotation;
    public Vector3 putDownScale = Vector3.one;
    public GameObject pickUpSound;
    public ItemType itemType;
    public bool noHoldingAnimation;
    public UltrakillEvent onPickUp;
    public UltrakillEvent onPutDown;
}

[Serializable]
public enum ItemType
{
    None,
    SkullBlue,
    SkullRed,
    SkullGreen,
    Readable,
    Torch,
    Soap,
    CustomKey1,
    CustomKey2,
    CustomKey3
}

[Serializable]
public class UltrakillEvent
{
    public GameObject[] toActivateObjects;
    public GameObject[] toDisActivateObjects;
    public UnityEvent onActivate;
    public UnityEvent onDisActivate;
}