using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI{

    public class Wander : TaskBase{

        [Header("Wandering Settings")]
        [SerializeField] private float _wanderRadius = 5f;
        [SerializeField] private float _wanderTime = 3f;

        //WANDERING BEHAVIOR
        public override void Enable(){
            _aiController.MoveToRandomLocation(_wanderRadius);
            EventMessenger.StartListening(EM_AIC_ON_TASK_DONE, OnTaskCompleted_Wander);
        }

        public override void Disable(){
            EventMessenger.StopListening(EM_AIC_ON_TASK_DONE, OnTaskCompleted_Wander);
        }

        private void OnTaskCompleted_Wander(){
            StartCoroutine(WaitToMoveRandomly(_wanderTime, _wanderRadius));
        }

        IEnumerator WaitToMoveRandomly(float time, float radius){
            yield return new WaitForSeconds(time);
            _aiController.MoveToRandomLocation(radius);
        }
    }
}

