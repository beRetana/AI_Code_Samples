using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// No name space to avoid assembly dependency issues with the messenger system made.

public class ObjectDetected{

    private GameObject _instigator;
    private float _range;
    private float _strength;

    public ObjectDetected(GameObject instigator, float range, float strength){
        _instigator = instigator;
        _range = range;
        _strength = strength;
    }

    public GameObject Instigator => _instigator;
    public float Range => _range;
    public float Strength => _strength;
}

