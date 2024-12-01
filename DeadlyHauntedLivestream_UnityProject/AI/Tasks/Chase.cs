using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI{

    public class Chase : TaskBase{

        [Header("Chase Settings")]
        [SerializeField] private float _chaseSpeed = 5f;
        [SerializeField] private float _chasedSpeed = 3f;

        private const string EM_FAILED_CHASE = "FailedChase";
        private const string EM_FAILED_DISABLE_PLAYER = "DisablePlayerAllIC";

        private GameObject _target;

        private void Update(){
            if(_target != null){
                if((_target.transform.position - transform.position).magnitude > _chasedSpeed){
                    _aiController.MoveTo(_target.transform.position);
                }
                else{
                    CatchPlayer();
                }
            }
        }

        private void CatchPlayer(){
            EventMessenger.TriggerEvent(EM_FAILED_DISABLE_PLAYER);
            _aiController.AbortTask();
        }

        public void SetTarget(GameObject target){
            _target = target;
        }

        public void SetTargetLost(){
            _target = null;
        }

        public override void Enable(){
            _aiController.ChangeSpeed(_chaseSpeed);
            EventMessenger.StartListening(EM_AIC_ON_TASK_DONE, OnCheckedLastLocation);
        }

        public override void Disable(){
            _aiController.DefaultSpeed();
        }

        private void OnCheckedLastLocation(){
            EventMessenger.StopListening(EM_AIC_ON_TASK_DONE, OnCheckedLastLocation);
            EventMessenger.TriggerEvent(EM_FAILED_CHASE);
            Disable();
        }
    }
}